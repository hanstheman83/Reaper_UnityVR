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
       
    [SerializeField] Renderer renderTexture_00_Renderer;
    [SerializeField] Renderer renderTexture_01_Renderer;
    [SerializeField] Renderer renderTexture_02_Renderer;
    [SerializeField] Renderer renderTexture_03_Renderer;
    [SerializeField] Renderer renderTexture_04_Renderer;
    [SerializeField] Renderer renderTexture_05_Renderer;
    [SerializeField] Renderer renderTexture_06_Renderer;
    [SerializeField] Renderer renderTexture_07_Renderer;
    [SerializeField] Renderer renderTexture_08_Renderer;
    [SerializeField] Renderer renderTexture_09_Renderer;
    [SerializeField] Renderer renderTexture_10_Renderer;
    [SerializeField] Renderer renderTexture_11_Renderer;
    [SerializeField] Renderer renderTexture_12_Renderer;
    [SerializeField] Renderer renderTexture_13_Renderer;
    [SerializeField] Renderer renderTexture_14_Renderer;
    [SerializeField] Renderer renderTexture_15_Renderer;
    [SerializeField][Tooltip("Multiple of 2 - 1024, 2048, 4096, ...")] int m_RenderTextureWidth = 1024; // 
    [SerializeField][Tooltip("Multiple of 2 - 1024, 2048, 4096, ...")] int m_RenderTextureHeight = 1024; //
    [SerializeField][Range(0.02f, 2f)][Tooltip("How fast stroke moves towards brush (slow value = delayed brush stroke)")] float drawSpeed = 0.02f;
    [SerializeField][Range(0.02f, 1f)] float renderTextureMipsRefreshRate = 0.03f;
    [SerializeField] GameObject strokePosition;
    [SerializeField] GameObject targetPosition;
    [SerializeField] GameObject depthPosition;
    [SerializeField] Color drawingColor;
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
    
    // update per update call. 
    ComputeBuffer GPU_BrushStrokeShapeBuffer; // 1D array of _Pixel structs! (later blend their colors)

    // update per update call. 
    ComputeBuffer GPU_BrushStrokePositionsBuffer; // 1D array of _Pixel structs (positions are from 0,0)
    ComputeBuffer GPU_JobDoneBuffer;

    // CPU buffers : arrays for compute buffers 
    private uint[] m_CPU_JobDoneBuffer;
    private _Pixel[] m_CPU_PointsOnLineBuffer;
    private _Pixel[] m_CPU_BrushStrokeShapeBuffer;
    private Vector4[] m_CPU_ActiveLayerBuffer; // update and reset per stroke
    private RenderTexture renderTexture_00;
    private RenderTexture renderTexture_01;
    private RenderTexture renderTexture_02;
    private RenderTexture renderTexture_03;
#if !UNITY_EDITOR

    private RenderTexture renderTexture_04;
    private RenderTexture renderTexture_05;
    private RenderTexture renderTexture_06;
    private RenderTexture renderTexture_07;
    private RenderTexture renderTexture_08;
    private RenderTexture renderTexture_09;
    private RenderTexture renderTexture_10;
    private RenderTexture renderTexture_11;
    private RenderTexture renderTexture_12;
    private RenderTexture renderTexture_13;
    private RenderTexture renderTexture_14;
    private RenderTexture renderTexture_15;
#endif
    private int m_ImageWidth;
    private int m_ImageHeight;

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
        m_ImageWidth = m_RenderTextureWidth * 4;
        m_ImageHeight = m_RenderTextureHeight * 4;

        layerManager.InitializeAllLayers(m_ImageWidth, m_ImageHeight);

        InitRenderTexture(renderTexture_00_Renderer, ref renderTexture_00,"_RenderTexture00");
        InitRenderTexture(renderTexture_01_Renderer, ref renderTexture_01,"_RenderTexture01");
        InitRenderTexture(renderTexture_02_Renderer, ref renderTexture_02,"_RenderTexture02");
        InitRenderTexture(renderTexture_03_Renderer, ref renderTexture_03,"_RenderTexture03");
#if !UNITY_EDITOR
        InitRenderTexture(renderTexture_04_Renderer, ref renderTexture_04,"_RenderTexture04");
        InitRenderTexture(renderTexture_05_Renderer, ref renderTexture_05,"_RenderTexture05");
        InitRenderTexture(renderTexture_06_Renderer, ref renderTexture_06,"_RenderTexture06");
        InitRenderTexture(renderTexture_07_Renderer, ref renderTexture_07,"_RenderTexture07");
        InitRenderTexture(renderTexture_08_Renderer, ref renderTexture_08,"_RenderTexture08");
        InitRenderTexture(renderTexture_09_Renderer, ref renderTexture_09,"_RenderTexture09");
        InitRenderTexture(renderTexture_10_Renderer, ref renderTexture_10,"_RenderTexture10");
        InitRenderTexture(renderTexture_11_Renderer, ref renderTexture_11,"_RenderTexture11");
        InitRenderTexture(renderTexture_12_Renderer, ref renderTexture_12,"_RenderTexture12");
        InitRenderTexture(renderTexture_13_Renderer, ref renderTexture_13,"_RenderTexture13");
        InitRenderTexture(renderTexture_14_Renderer, ref renderTexture_14,"_RenderTexture14");
        InitRenderTexture(renderTexture_15_Renderer, ref renderTexture_15,"_RenderTexture15");
#endif
        drawOnTexture_Compute.SetInt("_TextureWidth", m_RenderTextureWidth);
        drawOnTexture_Compute.SetInt("_TextureHeight", m_RenderTextureHeight);
        drawOnTexture_Compute.SetInt("_ImageWidth", m_ImageWidth);
        drawOnTexture_Compute.SetInt("_ImageHeight", m_ImageHeight);

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        depthPositionTransform = depthPosition.transform;
    }

    void InitRenderTexture(Renderer renderer, ref RenderTexture renderTexture, string name){
        // setting up RenderTexture
        int kernel = drawOnTexture_Compute.FindKernel("CSMain");

        renderTexture = new RenderTexture(m_RenderTextureWidth, m_RenderTextureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1;
        renderTexture.anisoLevel = 0;
        renderTexture.useMipMap = true;
        renderTexture.autoGenerateMips = false;
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Trilinear;
        //Debug.Log($"RT filter : {renderTexture.filterMode}"); 
        renderTexture.Create();
        renderer.material.mainTexture = renderTexture;
        // https://docs.unity3d.com/ScriptReference/ComputeShader.SetTexture.html
        
        drawOnTexture_Compute.SetTexture(kernel, name, renderTexture);
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

            if(lastStroke.x < 0){ 
                Debug.Log("Skipping first frame in new brush stroke..."); 
                lastStroke = canvasCoordinates;
                return;
            }

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
                // set dummy data all done buffer
                m_CPU_JobDoneBuffer = new uint[m_CPU_PointsOnLineBuffer.Length * m_CPU_BrushStrokeShapeBuffer.Length];
                GPU_JobDoneBuffer = new ComputeBuffer(m_CPU_JobDoneBuffer.Length, sizeof(uint));
                drawOnTexture_Compute.SetBuffer(kernel, "_JobDoneBuffer", GPU_JobDoneBuffer);

                // DISPATCH
                drawOnTexture_Compute.Dispatch(kernel, m_CPU_PointsOnLineBuffer.Length * m_CPU_BrushStrokeShapeBuffer.Length, 1, 1);

                // GET DATA will block update thread, 
                // TODO: replace with dummy bool array - per kernel run

                // Dummy call - to stop update loop when kernel is done - to delay .Release() calls!
                GPU_JobDoneBuffer.GetData(m_CPU_JobDoneBuffer);

                GPU_JobDoneBuffer.Release();
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

    // _Pixel[] CalculateBrushStroke(int width){
    //     _Pixel[] pixelsArray = new _Pixel[width * width];
    //     int count = 0;
    //     for (var i = 0; i < width; i++)
    //     {
    //         for (var j = 0; j < width; j++)
    //         {
    //             pixelsArray[count] = new _Pixel(
    //                 new Vector2Int(j, i), 
    //                 Color.blue);  
    //             count++;     
    //         }
    //     }
    //     return pixelsArray;
    // }

    _Pixel[] CalculatePointsOnLine_Pixels2D(Vector2[] pointsOnLine){
        //
        _Pixel[] pixelsArray = new _Pixel[pointsOnLine.Length];
        for (var i = 0; i < pointsOnLine.Length; i++)
        {
            pixelsArray[i].position_x = (uint) Mathf.Round(pointsOnLine[i].x * m_ImageWidth); 
            pixelsArray[i].position_y = (uint) Mathf.Round(pointsOnLine[i].y * m_ImageHeight); 
            pixelsArray[i].color_r = drawingColor.r;
            pixelsArray[i].color_g = drawingColor.g;
            pixelsArray[i].color_b = drawingColor.b;
            pixelsArray[i].color_a = drawingColor.a;
        }
        return pixelsArray;
    }

    // TODO: gradient between points, trigger = value, depth = size
    Vector2[] CalculatePointsOnLine(Vector2 start, Vector2 end){
        if(start.x < 0) Debug.LogError("Only positive values!");
        //Debug.Log("Calculating in between points".Colorize(Color.white));
        float r = .001f; // TODO: dynamic, brush radius
        float distanceBetweenBrushHits = Vector2.Distance(end, start);
        int numberOfStamps = Mathf.RoundToInt( (distanceBetweenBrushHits / r) );// 
        //Debug.Log("number of stamps :".Colorize(Color.blue) + numberOfStamps);
        float percentageIncrease = 1f/numberOfStamps;
        Vector2 deltaVector = end - start;
        Vector2[] inBetweenPoints = new Vector2[numberOfStamps];

        for (var i = 1; i <= numberOfStamps; i++) // TODO: include start of line
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
        drawingColor = drawingStickController.DrawingColor;
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
        // int kernel = drawOnTexture_Compute.FindKernel("CSMain");
        // int sizeOfVector4 = System.Runtime.InteropServices.Marshal.SizeOf((object)Vector4.zero);
        // GPU_ActiveLayerBuffer = new ComputeBuffer(textureHeight * textureWidth, sizeOfVector4); // bytesize of struct = all floats is struct!
        // m_CPU_ActiveLayerBuffer = layerManager.ActiveLayer.Pixels;
        // GPU_ActiveLayerBuffer.SetData(m_CPU_ActiveLayerBuffer);
        // drawOnTexture_Compute.SetBuffer(kernel, "_ActiveLayerBuffer", GPU_ActiveLayerBuffer);
    }
    void StopStroke(Collider other){
        isDrawing = false;
        otherObject = null;
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
        drawingStickController.StopResistance();
        drawingStickController = null;
        StartCoroutine(UpdateTexturesOnce()); // need to wait
        // GPU_ActiveLayerBuffer.GetData(m_CPU_ActiveLayerBuffer);
        // m_CPU_ActiveLayerBuffer.CopyTo(layerManager.ActiveLayer.Pixels, 0);
        // GPU_ActiveLayerBuffer.Release();


        // TODO: update active layer and final update of render texture
    }

    /// <summary>
    /// Transfer the X and Y coordinates in a 2D pixel grid to a 1D array coordinate.
    /// </summary>
    int TransferXYtoN(int x, int y){
        int n = x + (m_RenderTextureWidth * y);
        if(n >= m_RenderTextureWidth * m_RenderTextureHeight) return -1; 
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
            renderTexture_00.GenerateMips();
            renderTexture_01.GenerateMips();
            renderTexture_02.GenerateMips();
            renderTexture_03.GenerateMips();
#if !UNITY_EDITOR
            renderTexture_04.GenerateMips();
            renderTexture_05.GenerateMips();
            renderTexture_06.GenerateMips();
            renderTexture_07.GenerateMips();
            renderTexture_08.GenerateMips();
            renderTexture_09.GenerateMips();
            renderTexture_10.GenerateMips();
            renderTexture_11.GenerateMips();
            renderTexture_12.GenerateMips();
            renderTexture_13.GenerateMips();
            renderTexture_14.GenerateMips();
            renderTexture_15.GenerateMips();
#endif
            yield return new WaitForSeconds(renderTextureMipsRefreshRate);
        }
    }

    IEnumerator UpdateTexturesOnce(){
        yield return new WaitForEndOfFrame();
        renderTexture_00.GenerateMips();
        renderTexture_01.GenerateMips();
        renderTexture_02.GenerateMips();
        renderTexture_03.GenerateMips();
#if !UNITY_EDITOR
        renderTexture_04.GenerateMips();
        renderTexture_05.GenerateMips();
        renderTexture_06.GenerateMips();
        renderTexture_07.GenerateMips();
        renderTexture_08.GenerateMips();
        renderTexture_09.GenerateMips();
        renderTexture_10.GenerateMips();
        renderTexture_11.GenerateMips();
        renderTexture_12.GenerateMips();
        renderTexture_13.GenerateMips();
        renderTexture_14.GenerateMips();
        renderTexture_15.GenerateMips();
#endif
    }
#endregion Drawing Methods

}
