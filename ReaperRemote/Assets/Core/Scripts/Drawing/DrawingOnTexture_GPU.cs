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
    [SerializeField] Renderer renderTexture_16_Renderer;
    [SerializeField] Renderer renderTexture_17_Renderer;
    [SerializeField] Renderer renderTexture_18_Renderer;
    [SerializeField] Renderer renderTexture_19_Renderer;
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
    DrawingStickController m_DrawingStickController;
    Coroutine refreshRenderTextureMips;
    ChildTrigger childTrigger; // for XR interaction
    Collider childCollider; // XR interaction

    // For ComputeShader CPU-GPU communication 

    // Init on start stroke, save on end stroke
    ComputeBuffer GPU_ActiveLayer_Buffer; // 1D array of _Pixel structs
    
    // update per stroke start stop
    ComputeBuffer GPU_BrushStrokeShapeSize0_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize1_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize2_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize3_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize4_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeSizesArrayLengths_Buffer;
    
    
    // update per update call. 
    ComputeBuffer GPU_BrushStrokePositions_Buffer; // 1D array of _Pixel structs (positions are from 0,0)
    ComputeBuffer GPU_JobDone_Buffer;

    // CPU buffers : arrays for compute buffers 
    private uint[] m_CPU_JobDone_Buffer;
    private Pixel[] m_CPU_PointsOnLine_Buffer;
    // 1D array 0-1f brush alpha, a x a dimension when converted to 2D
    private float[] m_CPU_BrushStrokeShapeSize0_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize1_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize2_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize3_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize4_Buffer;
    private uint[] m_CPU_BrushStrokeSizesArrayLengths_Buffer;
    private Vector4[] m_CPU_ActiveLayer_Buffer; // update and reset per stroke
    private RenderTexture renderTexture_00;
    private RenderTexture renderTexture_01;
    private RenderTexture renderTexture_02;
    private RenderTexture renderTexture_03;
#if !UNITY_EDITOR // DirectX11 doesn't support many textures in a compute shader!
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
    private RenderTexture renderTexture_16;
    private RenderTexture renderTexture_17;
    private RenderTexture renderTexture_18;
    private RenderTexture renderTexture_19;
#endif
    private int m_ImageWidth;
    private int m_ImageHeight;

    private enum BiggestBrushSize {ThisFrame, LastFrame, Idem}
    private enum ActiveQuadrant {Q1, Q2, Q3, Q4}
    ActiveQuadrant activeQuadrant = default;

    bool m_IsDrawing = false;
    
    Transform m_OtherObject;

    Vector2 m_LastStroke = new Vector2(-1f, -1f); // init
    int m_LastActiveBrushSize = 0;

    

#region Unity Methods
    private void Awake() {
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        childTrigger.childTriggeredEnterEvent += StartStroke;
        childTrigger.childTriggeredExitEvent += StopStroke;
    }
    
    void Start()
    {
        m_ImageWidth = m_RenderTextureWidth * 4;
        m_ImageHeight = m_RenderTextureHeight * 5;

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
        InitRenderTexture(renderTexture_16_Renderer, ref renderTexture_16,"_RenderTexture16");
        InitRenderTexture(renderTexture_17_Renderer, ref renderTexture_17,"_RenderTexture17");
        InitRenderTexture(renderTexture_18_Renderer, ref renderTexture_18,"_RenderTexture18");
        InitRenderTexture(renderTexture_19_Renderer, ref renderTexture_19,"_RenderTexture19");
#endif
        drawOnTexture_Compute.SetInt("_TextureWidth", m_RenderTextureWidth);
        drawOnTexture_Compute.SetInt("_TextureHeight", m_RenderTextureHeight);
        drawOnTexture_Compute.SetInt("_ImageWidth", m_ImageWidth);
        drawOnTexture_Compute.SetInt("_ImageHeight", m_ImageHeight);

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        depthPositionTransform = depthPosition.transform;
    }
    // Init methods for Start()
    void InitRenderTexture(Renderer renderer, ref RenderTexture renderTexture, string name){
        // setting up RenderTexture
        int kernel = drawOnTexture_Compute.FindKernel("CSMain");

        renderTexture = new RenderTexture(m_RenderTextureWidth, m_RenderTextureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1;
        renderTexture.anisoLevel = 0;
        renderTexture.useMipMap = true;
        renderTexture.autoGenerateMips = false;
        renderTexture.enableRandomWrite = true;
        
        Debug.Log("Rendertexture sRGB :" + renderTexture.sRGB); 
        renderTexture.filterMode = FilterMode.Trilinear;
        //Debug.Log($"RT filter : {renderTexture.filterMode}"); 
        renderTexture.Create();
        renderer.material.mainTexture = renderTexture;
        // https://docs.unity3d.com/ScriptReference/ComputeShader.SetTexture.html
        
        drawOnTexture_Compute.SetTexture(kernel, name, renderTexture);
    }
    // TODO: release texture when they are not needed- free resources [also on loading another scene!]:
    // RenderTexture.Release()  Releases the RenderTexture.
    // This function releases the hardware resources used by the render texture. The texture itself is not destroyed, and will be automatically created again when being used.
    // As with other "native engine object" types, it is important to pay attention to the lifetime of any render textures and release them when you are finished using them, as they will not be garbage collected like normal managed types.

    private void OnDestroy() {
        childTrigger.childTriggeredEnterEvent += null;
        childTrigger.childTriggeredExitEvent += null;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_IsDrawing){
            UpdateStrokeAndTargetAndDepth();
            UpdateResistance();
            Vector2 canvasCoordinates = CalculateCanvasCoordinates();

            // int brushSizeIndex = drawingStickController.ActiveBrushSize;// index of brush array
            // float[] brushArray = drawingStickController.Brush.BrushSizes[brushSizeIndex];

            // save last frame trigger press
            // new function calculate brush strokes on line 
            // save new buffer with brush sizes matching number of strokes
            // 
#region return break
            if(m_LastStroke.x < 0){ 
                Debug.Log("Skipping first frame in new brush stroke..."); 
                m_LastStroke = canvasCoordinates;
                return;
            }
#endregion return break


            // Conversion based on RenderTextures , 4x5 - thus scale y dimension from 1 to 1.25, basically unscalling stretch in canvas dimensions. 
            canvasCoordinates = new Vector2(canvasCoordinates.x, canvasCoordinates.y * 1.25f );
            m_LastStroke = new Vector2(m_LastStroke.x, m_LastStroke.y * 1.25f);

            (Pixel[], int[]) pointsOnLineTuple; // pixel positions, brush width [in pixels] per point
            pointsOnLineTuple = CalculatePointsOnLine(m_LastStroke, canvasCoordinates, // TODO: convert to unscaled coordinates
                                        m_DrawingStickController.Brush.WidthOfBrushSize[m_LastActiveBrushSize], 
                                        m_DrawingStickController.Brush.WidthOfBrushSize[m_DrawingStickController.ActiveBrushSize]); // if lastStroke = -1 calculate only 1 point
            Pixel[] m_CPU_PointsOnLineBuffer = pointsOnLineTuple.Item1;
            int[] pointSizes = pointsOnLineTuple.Item2; // TODO: save in buffer!
            
            // TODO: set buffer
            //uint _BrushStrokePointSizesOnLine_BufferLength;

            

            if(m_CPU_PointsOnLineBuffer.Length > 0){
                Debug.Log($"Points on line {m_CPU_PointsOnLineBuffer.Length}:".Colorize(Color.magenta));
                //m_CPU_PointsOnLineBuffer = CalculatePointsOnLine_Pixels2D(pointsOnLine);

                int kernel = drawOnTexture_Compute.FindKernel("CSMain");
                // SET BUFFERS
                var structSize = sizeof(float)*4 + sizeof(uint)*2; // for all _Pixel 

               // set points on line
                GPU_BrushStrokePositions_Buffer = new ComputeBuffer(m_CPU_PointsOnLineBuffer.Length, structSize);
                GPU_BrushStrokePositions_Buffer.SetData(m_CPU_PointsOnLineBuffer);
                drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokePositionsBuffer", GPU_BrushStrokePositions_Buffer);
                // set sizes of points on line
                // TODO:
                
                // Calculate number of kernel runs 
                // 
                int numberOfRuns = 0; // pointSizes - radius of each point
                for (var i = 0; i < m_CPU_PointsOnLineBuffer.Length; i++)
                {
                    numberOfRuns += pointSizes[i] * pointSizes[i];
                }
                // set dummy data all done buffer
                m_CPU_JobDone_Buffer = new uint[numberOfRuns];
                
                GPU_JobDone_Buffer = new ComputeBuffer(m_CPU_JobDone_Buffer.Length, sizeof(uint));
                drawOnTexture_Compute.SetBuffer(kernel, "_JobDoneBuffer", GPU_JobDone_Buffer);
                // DISPATCH!
                drawOnTexture_Compute.Dispatch(kernel, numberOfRuns, 1, 1);

                // --------------------------------- GET DATA AND BUFFER RELEASE--------------------
                // GET DATA will block update thread, 
                // TODO: replace with dummy bool array - per kernel run

                // Dummy call - to stop update loop when kernel is done - to delay .Release() calls!
                GPU_JobDone_Buffer.GetData(m_CPU_JobDone_Buffer);
                GPU_JobDone_Buffer.Release();
                GPU_BrushStrokePositions_Buffer.Release();
                // TODO: release stroke sizes on line buffer
            }
            m_LastStroke = canvasCoordinates;
            m_LastActiveBrushSize = m_DrawingStickController.ActiveBrushSize;

        }
    }// end Update()
#endregion Unity Methods



            // -------------------------------------------------------------------- //
    // ------------------------------- START - STOP STROKES --------------------------------- //
#region Start Stop strokes

    void StartStroke(Collider other){
        m_IsDrawing = true;
        m_LastStroke = new Vector2(-1f, -1f); // skip first frame, no stroke length!
        m_OtherObject = other.transform.Find("DrawPoint");
        m_DrawingStickController = other.GetComponentInParent<DrawingStickController>();
        drawingColor = m_DrawingStickController.DrawingColor;
        m_DrawingStickController.StartResistance(); 
        strokePositionTransform.position = m_OtherObject.position;
        depthPositionTransform.position = m_OtherObject.position;
        strokePositionTransform.localPosition = new Vector3(strokePositionTransform.localPosition.x, 
                                                            strokePositionTransform.localPosition.y, 0f);
        if(refreshRenderTextureMips == null) refreshRenderTextureMips = StartCoroutine(UpdateRendureTextureMips());
        targetPositionTransform.position = m_OtherObject.position;
        targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, 
                                                            targetPositionTransform.localPosition.y, 0f);
        int kernel = drawOnTexture_Compute.FindKernel("CSMain");

        // brush stroke shapes
        m_CPU_BrushStrokeShapeSize0_Buffer = m_DrawingStickController.Brush.BrushSizes[0];
        m_CPU_BrushStrokeShapeSize1_Buffer = m_DrawingStickController.Brush.BrushSizes[1];
        m_CPU_BrushStrokeShapeSize2_Buffer = m_DrawingStickController.Brush.BrushSizes[2];
        m_CPU_BrushStrokeShapeSize3_Buffer = m_DrawingStickController.Brush.BrushSizes[3];
        m_CPU_BrushStrokeShapeSize4_Buffer = m_DrawingStickController.Brush.BrushSizes[4];

        m_CPU_BrushStrokeSizesArrayLengths_Buffer = new uint[m_DrawingStickController.Brush.NumberOfSizes];
        for (var i = 0; i < m_CPU_BrushStrokeSizesArrayLengths_Buffer.Length; i++)
        {
            m_CPU_BrushStrokeSizesArrayLengths_Buffer[i] = (uint)m_DrawingStickController.Brush.BrushSizes[i].Length;
        }

        GPU_BrushStrokeShapeSize0_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize0_Buffer.Length, sizeof(float) ); 
        GPU_BrushStrokeShapeSize1_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize1_Buffer.Length, sizeof(float) ); 
        GPU_BrushStrokeShapeSize2_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize2_Buffer.Length, sizeof(float) ); 
        GPU_BrushStrokeShapeSize3_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize3_Buffer.Length, sizeof(float) ); 
        GPU_BrushStrokeShapeSize4_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize4_Buffer.Length, sizeof(float) ); 

        GPU_BrushStrokeSizesArrayLengths_Buffer = new ComputeBuffer(m_CPU_BrushStrokeSizesArrayLengths_Buffer.Length, sizeof(uint));
        
        GPU_BrushStrokeShapeSize0_Buffer.SetData(m_CPU_BrushStrokeShapeSize0_Buffer);
        GPU_BrushStrokeShapeSize1_Buffer.SetData(m_CPU_BrushStrokeShapeSize1_Buffer);
        GPU_BrushStrokeShapeSize2_Buffer.SetData(m_CPU_BrushStrokeShapeSize2_Buffer);
        GPU_BrushStrokeShapeSize3_Buffer.SetData(m_CPU_BrushStrokeShapeSize3_Buffer);
        GPU_BrushStrokeShapeSize4_Buffer.SetData(m_CPU_BrushStrokeShapeSize4_Buffer);

        GPU_BrushStrokeSizesArrayLengths_Buffer.SetData(m_CPU_BrushStrokeSizesArrayLengths_Buffer);
        
        drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize0_Buffer", GPU_BrushStrokeShapeSize0_Buffer);
        drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize1_Buffer", GPU_BrushStrokeShapeSize1_Buffer);
        drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize2_Buffer", GPU_BrushStrokeShapeSize2_Buffer);
        drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize3_Buffer", GPU_BrushStrokeShapeSize3_Buffer);
        drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize4_Buffer", GPU_BrushStrokeShapeSize4_Buffer);

        drawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeSizesArrayLengths_Buffer", GPU_BrushStrokeSizesArrayLengths_Buffer);


        // SET ACTIVE LAYER
        // int sizeOfVector4 = System.Runtime.InteropServices.Marshal.SizeOf((object)Vector4.zero);
        // GPU_ActiveLayerBuffer = new ComputeBuffer(textureHeight * textureWidth, sizeOfVector4); // bytesize of struct = all floats is struct!
        // m_CPU_ActiveLayerBuffer = layerManager.ActiveLayer.Pixels;
        // GPU_ActiveLayerBuffer.SetData(m_CPU_ActiveLayerBuffer);
        // drawOnTexture_Compute.SetBuffer(kernel, "_ActiveLayerBuffer", GPU_ActiveLayerBuffer);
    }
    void StopStroke(Collider other){
        m_IsDrawing = false;
        m_OtherObject = null;
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
        m_DrawingStickController.StopResistance();
        m_DrawingStickController = null;
        StartCoroutine(UpdateTexturesOnce()); // need to wait

        GPU_BrushStrokeShapeSize0_Buffer.Release();
        GPU_BrushStrokeShapeSize1_Buffer.Release();
        GPU_BrushStrokeShapeSize2_Buffer.Release();
        GPU_BrushStrokeShapeSize3_Buffer.Release();
        GPU_BrushStrokeShapeSize4_Buffer.Release();

        GPU_BrushStrokeSizesArrayLengths_Buffer.Release();

        // GPU_ActiveLayerBuffer.GetData(m_CPU_ActiveLayerBuffer);
        // m_CPU_ActiveLayerBuffer.CopyTo(layerManager.ActiveLayer.Pixels, 0);
        // GPU_ActiveLayerBuffer.Release();


        // TODO: update active layer and final update of render texture
    }
#endregion Start Stop strokes


#region Drawing Methods
            // ------------------------------------------------------------------ //
    // ---------------------- DRAWING METHODS -------------------------------------------- //

    // _Pixel[] CalculatePointsOnLine_Pixels2D(Vector2[] pointsOnLine){
    //     //
    //     _Pixel[] pixelsArray = new _Pixel[pointsOnLine.Length];
    //     for (var i = 0; i < pointsOnLine.Length; i++)
    //     {
    //         pixelsArray[i].position_x = (uint) Mathf.Round(pointsOnLine[i].x * m_ImageWidth); 
    //         pixelsArray[i].position_y = (uint) Mathf.Round(pointsOnLine[i].y * m_ImageHeight); 
    //         pixelsArray[i].color_r = drawingColor.r;
    //         pixelsArray[i].color_g = drawingColor.g;
    //         pixelsArray[i].color_b = drawingColor.b;
    //         pixelsArray[i].color_a = drawingColor.a;
    //     }
    //     return pixelsArray;
    // }

    // TODO: gradient between points, trigger = value, depth = size
    /// <summary>
    /// brush widths in pixel sizes
    /// </summary>
    (Pixel[], int[]) CalculatePointsOnLine(Vector2 startPoint, Vector2 endPoint, int lastFrameBrushSize, int thisFrameBrushSize){
        BiggestBrushSize biggestBrushSize = default;
        if(lastFrameBrushSize == thisFrameBrushSize) { biggestBrushSize = BiggestBrushSize.Idem; }
        else if(lastFrameBrushSize > thisFrameBrushSize) { biggestBrushSize = BiggestBrushSize.LastFrame; }
        else if(lastFrameBrushSize < thisFrameBrushSize) { biggestBrushSize = BiggestBrushSize.ThisFrame; }

        float distanceBetweenBrushHits = Vector2.Distance(endPoint, startPoint); // magnitude of delta vector
        List<int> sizeOfBrushPerPoint = new List<int>(); // from biggest brush stroke to smallest
        List<Pixel> pixelCoordinates = new List<Pixel>(); // from biggest brush stroke to smallest
        
        // flip vector direction if size is reversed! Iteration will also be reversed
        Vector2 deltaVector = Vector2.zero;
        switch(biggestBrushSize){
            case BiggestBrushSize.Idem: // normal, iterate starting from lastFrameStroke to thisFrameStroke
            case BiggestBrushSize.LastFrame:
                deltaVector = endPoint - startPoint;
                sizeOfBrushPerPoint.Add(lastFrameBrushSize);
                // pixelCoordinates TODO: from canvas to pixel coordinates function - with unscaled y. 
                // TODO: center the pixel - avoid rounding error. Subtract .5 of pixel width and length for correct position!
                // pixelCoordinates.Add(new Pixel());
                break;
            case BiggestBrushSize.ThisFrame:
                sizeOfBrushPerPoint.Add(thisFrameBrushSize);
                deltaVector = startPoint - endPoint;
                break;
        }
        // TODO: edge case, one component has 0 increase!!
        if(deltaVector.x == 0) { deltaVector.x = 0.000001f; }
        if(deltaVector.y == 0) { deltaVector.y = 0.000001f; }

        // Calculate radiuses. Brush size is in pixel width (diameter)
        float radiusLastStroke = ( (float)m_DrawingStickController.Brush.WidthOfBrushSize[lastFrameBrushSize] / (float)m_ImageWidth ) /2f;
        float radiusThisStroke = ( (float)m_DrawingStickController.Brush.WidthOfBrushSize[thisFrameBrushSize] / (float)m_ImageWidth ) /2f;
        // Calculate stepSize based on smallest brush size radius
        float stepSize = ( ((float)m_DrawingStickController.Brush.WidthOfBrushSize[0]) / (float)m_ImageWidth ) /2f;
        // scaling normalized deltaVector by stepSize
        Vector2 normalizedDeltaVector = (deltaVector/distanceBetweenBrushHits);
        Vector2 stepSizedDeltaVector =  normalizedDeltaVector * stepSize; 
        
        // calculate line on which to iterate - 
        Vector2 iterationLineStart = Vector2.zero;
        Vector2 iterationLineEnd = Vector2.zero;  
        switch(biggestBrushSize){
            case BiggestBrushSize.LastFrame:
            case BiggestBrushSize.Idem:
                // P0 : start from last frame
                iterationLineStart = startPoint + (normalizedDeltaVector * radiusLastStroke);
                iterationLineEnd = (normalizedDeltaVector * radiusThisStroke) - endPoint;
                break;
            case BiggestBrushSize.ThisFrame:
                iterationLineStart = endPoint + (normalizedDeltaVector * radiusThisStroke);
                iterationLineEnd = (normalizedDeltaVector * radiusLastStroke) - startPoint;
                break;
        }
        float lengthOfIterationLine = Vector2.Distance(iterationLineEnd, iterationLineStart);

        // Determine what quadrant the delta vector is moving in 
        if(normalizedDeltaVector.x > 0 && normalizedDeltaVector.y > 0){
            activeQuadrant = ActiveQuadrant.Q1;
        }else if(normalizedDeltaVector.x < 0 && normalizedDeltaVector.y > 0){
            activeQuadrant = ActiveQuadrant.Q2;
        }else if(normalizedDeltaVector.x < 0 && normalizedDeltaVector.y < 0){
            activeQuadrant = ActiveQuadrant.Q3;
        }else if(normalizedDeltaVector.x > 0 && normalizedDeltaVector.y < 0){
            activeQuadrant = ActiveQuadrant.Q4;
        }else {
            Debug.LogError("No active quadrant set for delta vector!");
        }

        // Iteration
        bool shouldIterate = true; // stop iteration when overlaping last point + radius of that point's brush size. 
        float addedSteps = 0f;
        Vector2 currentPosition = iterationLineStart;
        Vector2 lastAddedBrushStrokePlusRadiusPosition = iterationLineStart; // 
        float radiusOfCurrentPositionBrushStroke = 0f;

        while(shouldIterate){
            switch(biggestBrushSize){
                case BiggestBrushSize.Idem:
                case BiggestBrushSize.LastFrame:
                    // calculate next step - (start of line is on circumference of first brushstroke)
                    currentPosition += stepSizedDeltaVector;
                    addedSteps += stepSize;
                    // end ? :
                    if(addedSteps >= lengthOfIterationLine){
                        shouldIterate = false;
                        break;
                    }
                    // 
                    int currentBrushSize = GetCurrentBrushSize(); // get current brush size by calculation - interpolation
                    float radiusOfCurrentPositionBrushStroke = RadiusOfCurrentPositionBrushStroke(currentPosition);
                    Vector2 vectorRadiusOfCurrentPositionBrushStroke = normalizedDeltaVector * radiusOfCurrentPositionBrushStroke;
                    switch(activeQuadrant){
                        case ActiveQuadrant.Q1:
                            if( (currentPosition - vectorRadiusOfCurrentPositionBrushStroke).x > lastAddedBrushStrokePlusRadiusPosition.x && 
                                (currentPosition - vectorRadiusOfCurrentPositionBrushStroke).y > lastAddedBrushStrokePlusRadiusPosition.y )
                            {
                                sizeOfBrushPerPoint.Add(currentBrushSize);
                                lastAddedBrushStrokePlusRadiusPosition = currentPosition + vectorRadiusOfCurrentPositionBrushStroke;
                                //TODO: add pixel
                            }
                            break;
                        case ActiveQuadrant.Q2:
                            if( (currentPosition - vectorRadiusOfCurrentPositionBrushStroke).x < lastAddedBrushStrokePlusRadiusPosition.x && 
                                (currentPosition - vectorRadiusOfCurrentPositionBrushStroke).y > lastAddedBrushStrokePlusRadiusPosition.y )
                            {
                                sizeOfBrushPerPoint.Add(currentBrushSize);
                                lastAddedBrushStrokePlusRadiusPosition = currentPosition + vectorRadiusOfCurrentPositionBrushStroke;
                                //TODO: add pixel
                            }
                            break;
                        case ActiveQuadrant.Q3:
                            break;
                        case ActiveQuadrant.Q4:
                            break;
                    }
                    break;
                case BiggestBrushSize.ThisFrame:
                    break;
            }
        }

        // lookup size function : 
        
        return (new Pixel[2], new int[2]);
    }

    float PixelLengthToFloatLengthConversion(int pixelLength){
        // remember scale factor - height > width!!
        
        return 0f;
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
        m_DrawingStickController.HandleResistance(resistance);
    }

    void UpdateStrokeAndTargetAndDepth(){
        targetPositionTransform.position = m_OtherObject.position;
        depthPositionTransform.position = m_OtherObject.position;
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
            renderTexture_16.GenerateMips();
            renderTexture_17.GenerateMips();
            renderTexture_18.GenerateMips();
            renderTexture_19.GenerateMips();
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
        renderTexture_16.GenerateMips();
        renderTexture_17.GenerateMips();
        renderTexture_18.GenerateMips();
        renderTexture_19.GenerateMips();
#endif
    }
#endregion Drawing Methods

}
