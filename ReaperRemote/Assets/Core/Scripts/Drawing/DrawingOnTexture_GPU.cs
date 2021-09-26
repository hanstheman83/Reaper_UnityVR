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
    public struct _Pixel{
        public uint position_x, position_y;
        public float color_r, color_g, color_b, color_a;
        public _Pixel(Vector2Int position, Color color){
            position_x = (uint)position.x;
            position_y = (uint)position.y;
            color_r = color.r;
            color_g = color.g;
            color_b = color.b;
            color_a = color.a;
        }
    }    

    [SerializeField] int textureHeight = 1024, textureWidth = 1024; // of all texture arrays in layers!!
    [SerializeField][Range(0.02f, 2f)] float drawSpeed = 0.02f;
    [SerializeField][Range(0.02f, 1f)] float renderTextureMipsRefreshRate = 0.03f;
    [SerializeField] GameObject strokePosition;
    [SerializeField] GameObject targetPosition;
    [SerializeField] GameObject depthPosition;
    [SerializeField] RenderTexture renderTexture;
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
    ComputeBuffer activeLayer_CB; // 1D array of _Pixel structs
    
    // Init on start stroke, update per update call. 
    ComputeBuffer pointsOnLine_CB; // 1D array of _Pixel structs! (later blend their colors)

    // Init on start stroke, update per update call. 
    ComputeBuffer brushStroke_CB; // 1D array of _Pixel structs (positions are from 0,0)
    // arrays for compute buffers 
    private _Pixel[] m_pointsOnLine_BufferCPU;
    private _Pixel[] m_brushStroke_BufferCPU;
    private Vector4[] m_activeLayer_BufferCPU; // update and reset per stroke

    int pos = 0;
    bool isDrawing = false;
    bool isCalculatingPixels = false;
    bool[] hasColor;
    
    Transform otherObject;

    Color32[] colors1D;
    // last stroke, in float %
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
        
        int sizeOfVector4 = System.Runtime.InteropServices.Marshal.SizeOf((object)Vector4.zero);
        activeLayer_CB = new ComputeBuffer(textureHeight * textureWidth, sizeOfVector4); // bytesize of struct = all floats is struct!
        activeLayer_CB.SetData(layerManager.ActiveLayer.Pixels);
        int kernel = drawOnTexture_Compute.FindKernel("CSMain");
        drawOnTexture_Compute.SetBuffer(kernel, "_ActiveLayerBuffer", activeLayer_CB);
        // https://docs.unity3d.com/ScriptReference/ComputeShader.SetTexture.html
        renderTexture.enableRandomWrite = true;
        drawOnTexture_Compute.SetTexture(kernel, "Result", renderTexture);

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        depthPositionTransform = depthPosition.transform;
        // strokePositionController = strokePosition.GetComponent<StrokePositionController>();

        Brush someB = new Brush();

        // init color array
        colors1D = new Color32[25];
        Color c = Colors.Spring;
        for (var i = 0; i < colors1D.Length; i++)
        {
            colors1D[i] = c;
        }




    }
    private void OnDestroy() {
        childTrigger.childTriggeredEnterEvent += null;
        childTrigger.childTriggeredExitEvent += null;
    }

    // Update is called once per frame
    void Update()
    {
        if(isDrawing)
        {
            // will block update thread, 
            activeLayer_CB.GetData(m_activeLayer_BufferCPU);
            // update mips on rendertexture
            

            UpdateStrokeAndTargetAndDepth();
            UpdateResistance();
            Vector2 canvasCoordinates = CalculateCanvasCoordinates();

            Vector2[] pointsOnLine = CalculatePointsOnLine(lastStroke, canvasCoordinates); // if lastStroke = -1 calculate only 1 point
            Vector2Int[] pointsOnLine_pixels2D = CalculatePointsOnLine_Pixels2D(pointsOnLine);

            var structSize = sizeof(float)*4 + sizeof(uint)*2;
            brushStroke_CB = new ComputeBuffer(25, structSize); // TODO: dispose old buffer ?
            var m_brushStroke_BufferCPU = CalculateBrushStroke(5);
            brushStroke_CB.SetData(m_brushStroke_BufferCPU);
            //ComputeBufferType.Counter;

            int kernel = drawOnTexture_Compute.FindKernel("CSMain");

            drawOnTexture_Compute.Dispatch(kernel, pointsOnLine_pixels2D.Length * m_brushStroke_BufferCPU.Length, 1, 1);

            lastStroke = canvasCoordinates;
            



            //pointsOnLine_CB.
            // GPU_VertexBuffer = new ComputeBuffer(m_vertexBufferCPU.Length, sizeof(float)*8);
            // pointsOnLine_CB.SetData(dataArray) // but only update after all data was calculated ?!

            // wrap code in new method - create bool : calculatingStroke and if statement
            // check job done!


            // 1 point = 1 brushStroke
            // Number of total pixels to draw in compute shader = #points * length of brushStroke
            
            // TODO: store array in ComputeBuffer

            
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


    Vector2Int[] CalculatePointsOnLine_Pixels2D(Vector2[] pointsOnLine){
        throw new System.NotImplementedException();
        //return default;
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
    }
    void StopStroke(Collider other){
        isDrawing = false;
        otherObject = null;
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
        drawingStickController.StopResistance();
        drawingStickController = null;
        StartCoroutine(UpdateTexturesOnce()); // need to wait

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
