using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Core.Interactions;

// COLOR32 //
// Each color component is a byte value with a range from 0 to 255.
// Color32.Lerp

// TODO: Shader to combine background and layers! 
// https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html

// Drawing Stroke handling, set data in layers, get data from layers, update FinalTexture
public class DrawingOnTexture_GPU : MonoBehaviour
{
       
    [SerializeField] Renderer renderTextureRenderer;
    [SerializeField] int textureHeight = 1024, textureWidth = 1024; // of all texture arrays in layers!!
    [SerializeField][Range(0.02f, 2f)] float drawSpeed = 0.02f;
    [SerializeField][Range(0.02f, 1f)] float renderTextureMipsRefreshRate = 0.03f;
    [SerializeField] GameObject strokePosition;
    [SerializeField] GameObject targetPosition;
    [SerializeField] GameObject depthPosition;
    [SerializeField] LayerManager_GPU layerManager;
    [SerializeField] ComputeShader drawOnTexture_Compute;
    Transform strokePositionTransform, targetPositionTransform, depthPositionTransform;
    StrokePositionController strokePositionController;
    DrawingStickController drawingStickController;
    Coroutine refreshRenderTextureMips;
    ChildTrigger childTrigger; // for XR interaction
    Collider childCollider; // XR interaction

    // For ComputeShader CPU-GPU communication 

    // Init on start stroke, save on end stroke
    ComputeBuffer GPU_ActiveLayerBuffer; // 1D array of _Pixel structs
    
    // Init on start stroke, update per update call. 
    ComputeBuffer GPU_BrushStrokeShapeBuffer; // 1D array of _Pixel structs! (later blend their colors)

    // Init on start stroke, update per update call. 
    ComputeBuffer GPU_BrushStrokePositionsBuffer; // 1D array of _Pixel structs (positions are from 0,0)
    // arrays for compute buffers 
    private _Pixel[] m_CPU_PointsOnLineBuffer;
    private _Pixel[] m_CPU_BrushStrokeShapeBuffer;
    private Vector4[] m_CPU_ActiveLayerBuffer; // update and reset per stroke
    private RenderTexture renderTexture;

    int pos = 0;
    bool isDrawing = false;
    bool isCalculatingPixels = false;
    bool[] hasColor;
    
    Transform otherObject;

    Vector2 lastStroke = new Vector2(-1f, -1f); // init

    

    #region Unity Methods
    private void Awake() {
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        //Material textureMaterial = GetComponentInChildren<Material>();
        childTrigger.childTriggeredEnterEvent += StartStroke;
        childTrigger.childTriggeredExitEvent += StopStroke;
    }
    
    void Start()
    {
        layerManager.InitializeAllLayers(textureWidth, textureHeight);

        // setting up RenderTexture
        int kernel = drawOnTexture_Compute.FindKernel("CSMain");

        renderTexture = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1;
        renderTexture.anisoLevel = 0;
        renderTexture.enableRandomWrite = true;
        renderTexture.useMipMap = true;
        renderTexture.autoGenerateMips = false;
        // TODO: filtermode trilenear ?
        renderTexture.filterMode = FilterMode.Trilinear;
        Debug.Log($"RT filter : {renderTexture.filterMode}"); 
        renderTexture.Create();
        renderTextureRenderer.material.mainTexture = renderTexture;
        // https://docs.unity3d.com/ScriptReference/ComputeShader.SetTexture.html
        
        renderTexture.enableRandomWrite = true;
        drawOnTexture_Compute.SetTexture(kernel, "Result", renderTexture);
        drawOnTexture_Compute.SetInt("_TextureWidth", textureWidth);
        drawOnTexture_Compute.SetInt("_TextureHeight", textureHeight);

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        depthPositionTransform = depthPosition.transform;
    }
    private void OnDestroy() {
        childTrigger.childTriggeredEnterEvent += null;
        childTrigger.childTriggeredExitEvent += null;

    }

    // Update is called once per frame
    void Update()
    {
        if(isDrawing){
            UpdateStrokeAndTargetAndDepth();
            UpdateResistance();
            Vector2 canvasCoordinates = CalculateCanvasCoordinates();

            Vector2[] pointsOnLine = CalculatePointsOnLine(lastStroke, canvasCoordinates); // if lastStroke = -1 calculate only 1 point
            
            if(pointsOnLine.Length > 0){
                Debug.Log($"Points on line {pointsOnLine.Length}:".Colorize(Color.magenta));
                m_CPU_PointsOnLineBuffer = CalculatePointsOnLine_Pixels2D(pointsOnLine);

                int kernel = drawOnTexture_Compute.FindKernel("CSMain");
                // SET BUFFERS
                // SET BRUSH STROKE
                int brushStrokeWidth = 3;
                int brushStrokeArrayLength = 9;
                drawOnTexture_Compute.SetInt("_BrushSizeStart", brushStrokeWidth);
                drawOnTexture_Compute.SetInt("_BrushStrokeArrayLength", brushStrokeArrayLength);
                // set brush shape
                var structSize = sizeof(float)*4 + sizeof(uint)*2; // for all _Pixel 
                m_CPU_BrushStrokeShapeBuffer = BrushGenerator.smallBrush3x3;
                GPU_BrushStrokeShapeBuffer = new ComputeBuffer(m_CPU_BrushStrokeShapeBuffer.Length, structSize); // TODO: dispose old buffer ?
                GPU_BrushStrokeShapeBuffer.SetData(m_CPU_BrushStrokeShapeBuffer);
                drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeBuffer", GPU_BrushStrokeShapeBuffer);
                // set points on line
                GPU_BrushStrokePositionsBuffer = new ComputeBuffer(m_CPU_PointsOnLineBuffer.Length, structSize);
                GPU_BrushStrokePositionsBuffer.SetData(m_CPU_PointsOnLineBuffer);
                drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokePositionsBuffer", GPU_BrushStrokePositionsBuffer);
                
                // DISPATCH
                drawOnTexture_Compute.Dispatch(kernel, m_CPU_PointsOnLineBuffer.Length * m_CPU_BrushStrokeShapeBuffer.Length, 1, 1);

                // GET DATA will block update thread, 
                GPU_ActiveLayerBuffer.GetData(m_CPU_ActiveLayerBuffer);

                GPU_BrushStrokePositionsBuffer.Release();
                GPU_BrushStrokeShapeBuffer.Release();
            }

            lastStroke = canvasCoordinates;
        }
    }// end Update()
    #endregion Unity Methods


    #region Drawing Methods
    // ------------------------------------------------------------------ //
    // ------------------------------------------------------------------ //

    _Pixel[] CalculateBrushStroke(int width){
        _Pixel[] pixelsArray = new _Pixel[width * width];
        int count = 0;
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < width; j++)
            {
                pixelsArray[count] = new _Pixel(
                    new Vector2Int(j, i), 
                    Color.blue);  
                count++;     
            }
        }
        return pixelsArray;
    }

    _Pixel[] CalculatePointsOnLine_Pixels2D(Vector2[] pointsOnLine){
        //
        _Pixel[] pixelsArray = new _Pixel[pointsOnLine.Length];
        for (var i = 0; i < pointsOnLine.Length; i++)
        {
            pixelsArray[i].position_x = (uint) Mathf.Round(pointsOnLine[i].x * textureWidth); 
            pixelsArray[i].position_y = (uint) Mathf.Round(pointsOnLine[i].y * textureHeight); 
            pixelsArray[i].color_r = 0f;
            pixelsArray[i].color_g = 0f;
            pixelsArray[i].color_b = 0f;
            pixelsArray[i].color_a = 1f;
        }
        return pixelsArray;
    }



    // TODO: gradient between points, trigger = value, depth = size
    Vector2[] CalculatePointsOnLine(Vector2 start, Vector2 end){
        //Debug.Log("Calculating in between points".Colorize(Color.white));
        float r = .001f; // TODO: dynamic, brush radius
        float distanceBetweenBrushHits = Vector2.Distance(end, start);
        int numberOfStamps = Mathf.RoundToInt( (distanceBetweenBrushHits / r) );// 
        //Debug.Log("number of stamps :".Colorize(Color.blue) + numberOfStamps);
        float percentageIncrease = 1f/numberOfStamps;
        Vector2 deltaVector = end - start;
        Vector2[] inBetweenPoints = new Vector2[numberOfStamps];

        for (var i = 1; i <= numberOfStamps; i++)
        {
            inBetweenPoints[i - 1] = start + deltaVector * (percentageIncrease * i);
        }
        return inBetweenPoints;
    }

    // Brush Strokes
    // TODO: Depth : No new stroke.. 
   // Trigger : erase hasColor array when changed! 
    void StartStroke(Collider other){
        isDrawing = true;
        lastStroke = new Vector2(-1f, -1f); // skip first frame, no stroke length!
        otherObject = other.transform.Find("DrawPoint");
        drawingStickController = other.GetComponentInParent<DrawingStickController>();
        drawingStickController.StartResistance(); 
        strokePositionTransform.position = otherObject.position;
        depthPositionTransform.position = otherObject.position;
        strokePositionTransform.localPosition = new Vector3(strokePositionTransform.localPosition.x, 
                                                            strokePositionTransform.localPosition.y, 0f);
        if(refreshRenderTextureMips == null) refreshRenderTextureMips = StartCoroutine(UpdateRendureTextureMips());
        targetPositionTransform.position = otherObject.position;
        targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, 
                                                            targetPositionTransform.localPosition.y, 0f);
        // SET ACTIVE LAYER
        int kernel = drawOnTexture_Compute.FindKernel("CSMain");
        int sizeOfVector4 = System.Runtime.InteropServices.Marshal.SizeOf((object)Vector4.zero);
        GPU_ActiveLayerBuffer = new ComputeBuffer(textureHeight * textureWidth, sizeOfVector4); // bytesize of struct = all floats is struct!
        m_CPU_ActiveLayerBuffer = layerManager.ActiveLayer.Pixels;
        GPU_ActiveLayerBuffer.SetData(m_CPU_ActiveLayerBuffer);
        drawOnTexture_Compute.SetBuffer(kernel, "_ActiveLayerBuffer", GPU_ActiveLayerBuffer);
    }
    void StopStroke(Collider other){
        isDrawing = false;
        otherObject = null;
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
        drawingStickController.StopResistance();
        drawingStickController = null;
        StartCoroutine(UpdateTexturesOnce()); // need to wait
        m_CPU_ActiveLayerBuffer.CopyTo(layerManager.ActiveLayer.Pixels, 0);
        GPU_ActiveLayerBuffer.Release();


        // TODO: update active layer and final update of render texture
    }

    /// <summary>
    /// Transfer the X and Y coordinates in a 2D pixel grid to a 1D array coordinate.
    /// </summary>
    int TransferXYtoN(int x, int y){
        int n = x + (textureWidth * y);
        if(n >= textureWidth * textureHeight) return -1; 
        return n;
    }

    void UpdateResistance(){
        float resistance = Mathf.Clamp( (depthPositionTransform.localPosition.z + .5f), 0f, 1f );
        // if(resistance > .8f) Debug.Log("Resistance : ".Colorize(Color.white) + resistance);
        drawingStickController.HandleResistance(resistance);
    }

    void UpdateStrokeAndTargetAndDepth(){
        targetPositionTransform.position = otherObject.position;
        depthPositionTransform.position = otherObject.position;
        targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, 
                                                            targetPositionTransform.localPosition.y, 0f);
        strokePositionTransform.localPosition = Vector2.MoveTowards(strokePositionTransform.localPosition, 
                                                                    targetPositionTransform.localPosition, 
                                                                    drawSpeed * Time.deltaTime);
    }

    Vector2 CalculateCanvasCoordinates(){
        return new Vector2(Mathf.Clamp((strokePositionTransform.localPosition.x + .5f), 0, 1), 
                           Mathf.Clamp((strokePositionTransform.localPosition.y + .5f), 0, 1));
    }

    private IEnumerator UpdateRendureTextureMips(){
        while(true){
            renderTexture.GenerateMips();
            yield return new WaitForSeconds(renderTextureMipsRefreshRate);
        }
    }

    IEnumerator UpdateTexturesOnce(){
        yield return new WaitForEndOfFrame();
        renderTexture.GenerateMips();
    }
    #endregion Drawing Methods

}
