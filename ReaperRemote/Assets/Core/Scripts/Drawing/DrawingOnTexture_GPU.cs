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
    [SerializeField][Tooltip("Multiple of 2 -512 1024, 2048, 4096, ...")] int m_RenderTextureWidth = 1024; // 
    [SerializeField][Tooltip("Multiple of 2 -512 1024, 2048, 4096, ...")] int m_RenderTextureHeight = 1024; //
    [SerializeField][Range(0.02f, 2f)][Tooltip("How fast stroke moves towards brush (slow value = delayed brush stroke)")] float m_drawSpeed = 0.02f;
    [SerializeField][Range(0.02f, .2f)] float m_RenderTextureMipsRefreshRate = 0.03f;
    [SerializeField] GameObject m_StrokePosition;
    [SerializeField] GameObject m_TargetPosition;
    [SerializeField] GameObject m_DepthPosition;
    [SerializeField] Transform m_ReleasePosition;
    [SerializeField] Color drawingColor;
    [SerializeField] LayerManager_GPU layerManager;
    [SerializeField] ComputeShader m_DrawOnTexture_Compute;
    Transform m_StrokePositionTransform, m_TargetPositionTransform, m_DepthPositionTransform;
    //StrokePositionController m_StrokePositionController;
    DrawingStickController m_DrawingPencilController;
    Coroutine refreshRenderTextureMips;
    ChildTrigger childTrigger; // for XR interaction
    Collider childCollider; // XR interaction
    Collider m_PencilCollider;

    // For ComputeShader CPU-GPU communication 

    // Global variables for compute shader
    private int m_ImageWidth;
    private int m_ImageHeight;

    // Init on start stroke, save on end stroke
    ComputeBuffer GPU_ActiveLayer_Buffer; // 1D array of _Pixel structs
    
    // update per stroke start stop
    ComputeBuffer GPU_BrushStrokeShapeSize0_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize1_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize2_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize3_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeShapeSize4_Buffer; // 1D array of _Pixel structs! (later blend their colors)
    ComputeBuffer GPU_BrushStrokeSizesArrayLengths_Buffer;
    ComputeBuffer GPU_BrushStrokeShapesWidths_Buffer;
    ComputeBuffer GPU_BrushStrokeShapesOffset_Buffer;
    
    // update per update call. 
    ComputeBuffer GPU_BrushStrokePositionsOnLine_Buffer; // 1D array of _Pixel structs (positions are from 0,0)
    ComputeBuffer GPU_BrushStrokeSizesOnLine_Buffer;
    ComputeBuffer GPU_JobDone_Buffer; // dummy data - to block update loop waiting for data from compute shader


    // ---------------------------------------------
    // ---- CPU buffers : arrays for compute buffers 
    // ---------------------------------------------

    // Per update call : (could make non global ??)
    uint[] m_CPU_BrushStrokeSizesOnLine_Buffer;
    private Pixel[] m_CPU_BrushStrokePositionsOnLine_Buffer;
    private int[] m_CPU_JobDone_Buffer;

    // Per start stop stroke
    // 1D arrays 0-1f brush alpha, a x a dimension when converted to 2D
    uint[] m_CPU_BrushStrokeShapesWidths_Buffer;
    int[] m_CPU_BrushStrokeShapesOffset_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize0_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize1_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize2_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize3_Buffer;
    private float[] m_CPU_BrushStrokeShapeSize4_Buffer;
    private uint[] m_CPU_BrushStrokeSizesArrayLengths_Buffer;
    
    // RENDER TEXTURES, shared with GPU compute buffer
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
    

    private enum BiggestBrushSize {ThisFrame, PreviousFrame, Idem}
    private enum ActiveQuadrant {Q1, Q2, Q3, Q4}
    ActiveQuadrant activeQuadrant = default;

    bool m_IsDrawing = false;
    
    Transform m_OtherObject;

    Vector2 m_PreviousStroke = new Vector2(-1f, -1f); // init
    int m_LastActiveBrushSize = 0;

    

#region Unity Methods
    private void Awake() {
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        childTrigger.childTriggeredEnterEvent += StartStroke;
        
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
        m_DrawOnTexture_Compute.SetInt("_TextureWidth", m_RenderTextureWidth);
        m_DrawOnTexture_Compute.SetInt("_TextureHeight", m_RenderTextureHeight);
        m_DrawOnTexture_Compute.SetInt("_ImageWidth", m_ImageWidth);
        m_DrawOnTexture_Compute.SetInt("_ImageHeight", m_ImageHeight);

        m_StrokePositionTransform = m_StrokePosition.transform;
        m_TargetPositionTransform = m_TargetPosition.transform;
        m_DepthPositionTransform = m_DepthPosition.transform;
    }
    // Init methods for Start()
    void InitRenderTexture(Renderer renderer, ref RenderTexture renderTexture, string name){
        // setting up RenderTexture
        int kernel = m_DrawOnTexture_Compute.FindKernel("CSMain");

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
        
        m_DrawOnTexture_Compute.SetTexture(kernel, name, renderTexture);
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
    void Update(){ // TODO: Brush stops on canvas - the original position = haptics, calculate new position (reset z). -- Child gameobject offset 
        
    #region return break -- Not drawing!
        if(m_IsDrawing == false) { // TODO: add if not holding pencil!!
            return;
        }else if(m_IsDrawing == true && !m_DrawingPencilController.DrawingModeActive){
            // pencil was released during drawing
            StopStroke(m_PencilCollider);
            return;
        }
    #endregion return break

        UpdateStrokeAndTargetAndDepth();
        UpdateDrawingStickOffset(); // based on depth
        UpdateResistance();
        Vector2 currentStroke = CalculateCanvasCoordinatesRaw(); // scaled/stretched in y dimension
        // Conversion based on RenderTextures , 4x5 - thus scale y dimension from 1 to 1.25, basically unscalling stretch in canvas dimensions. 
        currentStroke = new Vector2(currentStroke.x, currentStroke.y * 1.25f );

    #region return break -- first frame, can't calculate line
        if(m_PreviousStroke.x < 0){ 
            Debug.Log("Skipping first frame in new brush stroke..."); 
            m_PreviousStroke = currentStroke;
            m_LastActiveBrushSize = m_DrawingPencilController.ActiveBrushSize;
            return;
        }
    #endregion return break

        // iterating and lerping the line between the known (captured) brush strokes. 
        (Pixel[], uint[]) pointsOnLineTuple; // pixel positions, brush width [in pixels] per point
        pointsOnLineTuple = CalculatePointsOnLine(m_PreviousStroke, currentStroke,
                                    m_LastActiveBrushSize, 
                                    m_DrawingPencilController.ActiveBrushSize); // if lastStroke = -1 calculate only 1 point
        m_CPU_BrushStrokePositionsOnLine_Buffer = pointsOnLineTuple.Item1;
        m_CPU_BrushStrokeSizesOnLine_Buffer = pointsOnLineTuple.Item2;
        if(m_CPU_BrushStrokePositionsOnLine_Buffer.Length != m_CPU_BrushStrokeSizesOnLine_Buffer.Length){
            Debug.LogError("The two arrays have uneven lengths?!");
        }
        
    #region return break -- no calculated strokes, nothing to draw!
        if(m_CPU_BrushStrokePositionsOnLine_Buffer.Length <= 0){ // should never happen!
            m_PreviousStroke = currentStroke;
            return;
        }
    #endregion return break

        // -----------------
        // For compute shader 
        Debug.Log($"Points on line {m_CPU_BrushStrokePositionsOnLine_Buffer.Length}:".Colorize(Color.magenta));
        int kernel = m_DrawOnTexture_Compute.FindKernel("CSMain");
        // SET VARIABLES
        m_DrawOnTexture_Compute.SetInt("_NumberOfBrushStrokesOnLine", m_CPU_BrushStrokePositionsOnLine_Buffer.Length);
        // Stride
        var structSize = sizeof(float)*4 + sizeof(uint)*2; // for all _Pixel 
        // SET BUFFERS
        // set pixel points on line
        GPU_BrushStrokePositionsOnLine_Buffer = new ComputeBuffer(m_CPU_BrushStrokePositionsOnLine_Buffer.Length, structSize);
        GPU_BrushStrokePositionsOnLine_Buffer.SetData(m_CPU_BrushStrokePositionsOnLine_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokePositionsOnLine_Buffer", GPU_BrushStrokePositionsOnLine_Buffer);
        // set sizes of points on line
        GPU_BrushStrokeSizesOnLine_Buffer = new ComputeBuffer(m_CPU_BrushStrokeSizesOnLine_Buffer.Length, sizeof(uint));
        GPU_BrushStrokeSizesOnLine_Buffer.SetData(m_CPU_BrushStrokeSizesOnLine_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeSizesOnLine_Buffer", GPU_BrushStrokeSizesOnLine_Buffer);

        // Calculate number of kernel runs 
        int numberOfRuns = 0; // pointSizes - radius of each point
        for (var i = 0; i < m_CPU_BrushStrokePositionsOnLine_Buffer.Length; i++)
        {
            int brushStrokeSize = (int)m_CPU_BrushStrokeSizesOnLine_Buffer[i];
            Debug.Log("brush stroke size : " + brushStrokeSize);
            numberOfRuns += (   m_DrawingPencilController.Brush.WidthOfBrushSize[brushStrokeSize] * 
                                m_DrawingPencilController.Brush.WidthOfBrushSize[brushStrokeSize] );
            Debug.Log("number or runs : " + numberOfRuns);
        }
        Debug.Log("number of runs total : " + numberOfRuns);

        // set dummy data all done buffer
        m_CPU_JobDone_Buffer = new int[numberOfRuns];
        GPU_JobDone_Buffer = new ComputeBuffer(m_CPU_JobDone_Buffer.Length, sizeof(int));
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_JobDone_Buffer", GPU_JobDone_Buffer);

        // --------
        // DISPATCH!
        m_DrawOnTexture_Compute.Dispatch(kernel, numberOfRuns, 1, 1);

        // --------------------------------- GET DATA AND BUFFER RELEASE--------------------
        // GET DATA will block update thread, 

        // Dummy call - to stop update loop when kernel is done - to delay .Release() calls!
        GPU_JobDone_Buffer.GetData(m_CPU_JobDone_Buffer);
        GPU_JobDone_Buffer.Release();
        GPU_BrushStrokePositionsOnLine_Buffer.Release();
        GPU_BrushStrokeSizesOnLine_Buffer.Release();

        m_PreviousStroke = currentStroke;
        m_LastActiveBrushSize = m_DrawingPencilController.ActiveBrushSize;
    }// end Update()
#endregion Unity Methods




            // -------------------------------------------------------------------- //
            // -------------------------------------------------------------------- //
    // ------------------------------- START - STOP STROKES --------------------------------- //
#region Start Stop strokes

    void StartStroke(Collider other)
    {
        childTrigger.childTriggeredExitEvent += StopStroke;
        m_PencilCollider = other;
        m_PreviousStroke = new Vector2(-1f, -1f); // skip first frame, no stroke length!
        m_OtherObject = other.transform.Find("DrawPoint");
        m_DrawingPencilController = other.GetComponentInParent<DrawingStickController>();
        m_DrawingPencilController.StartDrawingMode();

        if(m_DrawingPencilController.ControlledBy != Core.Controls.ControllerHand.None){
            m_IsDrawing = true;

        }else{
            m_IsDrawing = false;
        }
        if (m_IsDrawing)
        {
            InitStrokeData();
        }
    }

    void StopStroke(Collider other){
        m_IsDrawing = false;
        m_OtherObject = null;
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
        m_DrawingPencilController.StopResistance();
        m_DrawingPencilController.OffsetMainMesh(Vector3.zero);
        m_DrawingPencilController.StopDrawingMode();
        // check if going through page - 
        if(m_DepthPositionTransform.localPosition.z > 0f){
            // TODO: calc correct! - 
            m_ReleasePosition.localPosition = new Vector3(  m_TargetPosition.transform.localPosition.x, 
                                                            m_TargetPosition.transform.localPosition.y,
                                                            m_ReleasePosition.localPosition.z);
            m_DrawingPencilController.ReleasePencil(m_ReleasePosition.position);
        }
        m_DrawingPencilController = null;
        StartCoroutine(UpdateTexturesOnce()); // need to wait

        GPU_BrushStrokeShapeSize0_Buffer.Release();
        GPU_BrushStrokeShapeSize1_Buffer.Release();
        GPU_BrushStrokeShapeSize2_Buffer.Release();
        GPU_BrushStrokeShapeSize3_Buffer.Release();
        GPU_BrushStrokeShapeSize4_Buffer.Release();

        GPU_BrushStrokeSizesArrayLengths_Buffer.Release();

        GPU_BrushStrokeShapesWidths_Buffer.Release();
        GPU_BrushStrokeShapesOffset_Buffer.Release();

        childTrigger.childTriggeredExitEvent -= StopStroke;
    }

    // Helper function for Strokes
    private void InitStrokeData()
    {
        drawingColor = m_DrawingPencilController.DrawingColor;
        m_DrawingPencilController.StartResistance();
        m_StrokePositionTransform.position = m_OtherObject.position;
        m_DepthPositionTransform.position = m_OtherObject.position;
        m_StrokePositionTransform.localPosition = new Vector3(m_StrokePositionTransform.localPosition.x,
                                                            m_StrokePositionTransform.localPosition.y, 0f);
        if (refreshRenderTextureMips == null) refreshRenderTextureMips = StartCoroutine(UpdateRendureTextureMips());
        m_TargetPositionTransform.position = m_OtherObject.position;
        m_TargetPositionTransform.localPosition = new Vector3(m_TargetPositionTransform.localPosition.x,
                                                            m_TargetPositionTransform.localPosition.y, 0f);
        // ------------------------
        // For compute shader - GPU
        int kernel = m_DrawOnTexture_Compute.FindKernel("CSMain");
        // Buffers 
        // brush stroke shapes
        m_CPU_BrushStrokeShapeSize0_Buffer = m_DrawingPencilController.Brush.BrushSizes[0];
        m_CPU_BrushStrokeShapeSize1_Buffer = m_DrawingPencilController.Brush.BrushSizes[1];
        m_CPU_BrushStrokeShapeSize2_Buffer = m_DrawingPencilController.Brush.BrushSizes[2];
        m_CPU_BrushStrokeShapeSize3_Buffer = m_DrawingPencilController.Brush.BrushSizes[3];
        m_CPU_BrushStrokeShapeSize4_Buffer = m_DrawingPencilController.Brush.BrushSizes[4];

        m_CPU_BrushStrokeShapesWidths_Buffer = new uint[m_DrawingPencilController.Brush.NumberOfSizes];
        m_CPU_BrushStrokeShapesOffset_Buffer = new int[m_DrawingPencilController.Brush.NumberOfSizes];

        // TODO: Nice to have : set dynamic
        m_CPU_BrushStrokeShapesOffset_Buffer[0] = -1;
        m_CPU_BrushStrokeShapesOffset_Buffer[1] = -2;
        m_CPU_BrushStrokeShapesOffset_Buffer[2] = -3;
        m_CPU_BrushStrokeShapesOffset_Buffer[3] = -4;
        m_CPU_BrushStrokeShapesOffset_Buffer[4] = -5;


        m_CPU_BrushStrokeSizesArrayLengths_Buffer = new uint[m_DrawingPencilController.Brush.NumberOfSizes];
        for (var i = 0; i < m_CPU_BrushStrokeSizesArrayLengths_Buffer.Length; i++)
        {
            m_CPU_BrushStrokeSizesArrayLengths_Buffer[i] = (uint)m_DrawingPencilController.Brush.BrushSizes[i].Length;
            m_CPU_BrushStrokeShapesWidths_Buffer[i] = (uint)m_DrawingPencilController.Brush.WidthOfBrushSize[i];

        }

        GPU_BrushStrokeShapeSize0_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize0_Buffer.Length, sizeof(float));
        GPU_BrushStrokeShapeSize1_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize1_Buffer.Length, sizeof(float));
        GPU_BrushStrokeShapeSize2_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize2_Buffer.Length, sizeof(float));
        GPU_BrushStrokeShapeSize3_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize3_Buffer.Length, sizeof(float));
        GPU_BrushStrokeShapeSize4_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapeSize4_Buffer.Length, sizeof(float));

        GPU_BrushStrokeSizesArrayLengths_Buffer = new ComputeBuffer(m_CPU_BrushStrokeSizesArrayLengths_Buffer.Length, sizeof(uint));

        GPU_BrushStrokeShapesWidths_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapesWidths_Buffer.Length, sizeof(uint));
        GPU_BrushStrokeShapesOffset_Buffer = new ComputeBuffer(m_CPU_BrushStrokeShapesOffset_Buffer.Length, sizeof(int));

        GPU_BrushStrokeShapeSize0_Buffer.SetData(m_CPU_BrushStrokeShapeSize0_Buffer);
        GPU_BrushStrokeShapeSize1_Buffer.SetData(m_CPU_BrushStrokeShapeSize1_Buffer);
        GPU_BrushStrokeShapeSize2_Buffer.SetData(m_CPU_BrushStrokeShapeSize2_Buffer);
        GPU_BrushStrokeShapeSize3_Buffer.SetData(m_CPU_BrushStrokeShapeSize3_Buffer);
        GPU_BrushStrokeShapeSize4_Buffer.SetData(m_CPU_BrushStrokeShapeSize4_Buffer);

        GPU_BrushStrokeSizesArrayLengths_Buffer.SetData(m_CPU_BrushStrokeSizesArrayLengths_Buffer);

        GPU_BrushStrokeShapesWidths_Buffer.SetData(m_CPU_BrushStrokeShapesWidths_Buffer);
        GPU_BrushStrokeShapesOffset_Buffer.SetData(m_CPU_BrushStrokeShapesOffset_Buffer);

        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize0_Buffer", GPU_BrushStrokeShapeSize0_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize1_Buffer", GPU_BrushStrokeShapeSize1_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize2_Buffer", GPU_BrushStrokeShapeSize2_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize3_Buffer", GPU_BrushStrokeShapeSize3_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize4_Buffer", GPU_BrushStrokeShapeSize4_Buffer);

        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeSizesArrayLengths_Buffer", GPU_BrushStrokeSizesArrayLengths_Buffer);

        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapesWidths_Buffer", GPU_BrushStrokeShapesWidths_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapesOffset_Buffer", GPU_BrushStrokeShapesOffset_Buffer);
    }

    
#endregion Start Stop strokes




#region Drawing Methods
            // ------------------------------------------------------------------ //
    // ---------------------- DRAWING METHODS -------------------------------------------- //


    /// <summary>
    /// Calculated line can start from either point - dependent on what point (this frame or last) has largest brush size.
    /// </summary>
    /// <returns>Pixel Coordinates and brush widths in pixel sizes.</returns>
    (Pixel[], uint[]) CalculatePointsOnLine(Vector2 pointPreviousFrame, Vector2 pointThisFrame, int previousFrameBrushSize, int thisFrameBrushSize){ 
        Debug.Log("Point prev frame : " + pointPreviousFrame);
        Debug.Log("Point this frame : " + pointThisFrame);
        
        BiggestBrushSize biggestBrushSize = default;
        if(previousFrameBrushSize == thisFrameBrushSize) { biggestBrushSize = BiggestBrushSize.Idem; }
        else if(previousFrameBrushSize > thisFrameBrushSize) { biggestBrushSize = BiggestBrushSize.PreviousFrame; }
        else if(previousFrameBrushSize < thisFrameBrushSize) { biggestBrushSize = BiggestBrushSize.ThisFrame; }

        float distanceBetweenBrushHits = Vector2.Distance(pointThisFrame, pointPreviousFrame); // magnitude of delta vector
        Debug.Log($"Distance between hits : {distanceBetweenBrushHits}");
        List<uint> sizeOfBrushPerPoint = new List<uint>(); // from biggest brush stroke to smallest
        List<Pixel> pixelCoordinates = new List<Pixel>(); // from biggest brush stroke to smallest

        // Adding first points to list, before starting the iteration. Calculate deltavector
        // flip vector direction if brush size is reversed! Iteration will also be reversed, iterate from big to small brush size
        // TODO: Nice to have : center the pixel - avoid rounding error. Subtract .5 of pixel width and length for correct position!
        Vector2 deltaVector = Vector2.zero;
        Vector2Int firstPixelPosition;
        Pixel firstNewPixel;
        Debug.Log("Brush size previous frame : " + previousFrameBrushSize);
        Debug.Log("Brush size this frame : " + thisFrameBrushSize);
        switch(biggestBrushSize){
            case BiggestBrushSize.Idem: // normal, iterate starting from lastFrameStroke to thisFrameStroke
            case BiggestBrushSize.PreviousFrame:
                deltaVector = pointThisFrame - pointPreviousFrame;
                firstPixelPosition = CalculatePixelCoordinates(pointPreviousFrame);
                firstNewPixel = new Pixel(firstPixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(firstNewPixel);
                sizeOfBrushPerPoint.Add((uint)previousFrameBrushSize);
                Debug.Log($"Biggest brush size : previous frame or idem \n First pixel in array : {firstPixelPosition} \n Added coords to list..");
                break;
            case BiggestBrushSize.ThisFrame:
                deltaVector = pointPreviousFrame - pointThisFrame;
                firstPixelPosition = CalculatePixelCoordinates(pointThisFrame);
                firstNewPixel = new Pixel(firstPixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(firstNewPixel);
                sizeOfBrushPerPoint.Add((uint)thisFrameBrushSize);
                Debug.Log($"Biggest brush size : this frame \n First pixel in array : {firstPixelPosition} Added coords to list..");
                break;
            default:
                Debug.LogError("Error - no biggest brush size!!!");
                break;
        }
        if(deltaVector.x == 0) { deltaVector.x = 0.000001f; }
        if(deltaVector.y == 0) { deltaVector.y = 0.000001f; }

        // -- Preparing Iteration
        // Calculate radiuses. Brush size is in pixel width (diameter)
        float radiusPreviousStroke = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.Brush.WidthOfBrushSize[previousFrameBrushSize]) /2f;
        Debug.Log("radiusLastStroke : " + radiusPreviousStroke);
        float radiusThisStroke = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.Brush.WidthOfBrushSize[thisFrameBrushSize]) /2f;
        Debug.Log("RadiusThisStroke : " + radiusThisStroke);
        // Calculate stepSize based on smallest brush size radius
        float stepSize = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.Brush.WidthOfBrushSize[0]) /2f; // stepSize is the radius of the smallest brush
        Debug.Log("stepsize : " + stepSize);
        // scaling normalized deltaVector by stepSize
        Vector2 normalizedDeltaVector = (deltaVector/distanceBetweenBrushHits);
        Debug.Log("normalized delta vector : " + normalizedDeltaVector.ToString("F6"));
        Vector2 stepSizedDeltaVector =  normalizedDeltaVector * stepSize;
        Debug.Log("stepsized deltavector : " + stepSizedDeltaVector.ToString("F6")); 
        // calculate line on which to iterate - 
        Vector2 iterationLineStart = Vector2.zero; // delta vector added to this
        Vector2 iterationLineEnd = Vector2.zero;  
        switch(biggestBrushSize){ // Making sure that in both cases the line starts with the biggest brush size!
            case BiggestBrushSize.PreviousFrame:
            case BiggestBrushSize.Idem:
                // P0 : start from last frame -- direction of deltavector in direction from previous stroke point to this frame stroke point
                iterationLineStart = pointPreviousFrame + (normalizedDeltaVector * radiusPreviousStroke);
                iterationLineEnd = pointThisFrame - (normalizedDeltaVector * radiusThisStroke);
                break;
            case BiggestBrushSize.ThisFrame:
                // P0 : start from this frame
                iterationLineStart = pointThisFrame + (normalizedDeltaVector * radiusThisStroke);
                iterationLineEnd = pointPreviousFrame - (normalizedDeltaVector * radiusPreviousStroke);
                break;
            default:
                Debug.LogError("Error - no biggest brush size!!!");
                break;
        }
        Debug.Log("Iteration line start : " + iterationLineStart);
        Debug.Log("Iteration line end : " + iterationLineEnd);
        float lengthOfIterationLine = Vector2.Distance(iterationLineEnd, iterationLineStart);
        Debug.Log("length of iteration line " + lengthOfIterationLine);

        bool shouldIterate = true; // stop iteration when overlaping last point + radius of that point's brush size. 
        float currentPos1D = 0f;
        Vector2 currentPos = iterationLineStart;
        Vector2 lastAddedBrushStrokePlusRadiusPos = iterationLineStart; // 
        float lastAddedBrushStrokePlusRadiusPos1D = 0; // add brush radius and see if will fit! 

        // Iteration
        while(shouldIterate){
            // calculate next step - (start of line is on circumference of first brushstroke)
            currentPos += stepSizedDeltaVector;
            currentPos1D += stepSize;
            // end ? :
            if(currentPos1D >= lengthOfIterationLine){
                Debug.Log("added steps : " + currentPos1D);
                Debug.Log("current position " + currentPos);
                shouldIterate = false;
                break;
            }
       
            // Calculate brush size
            // % of line from start to end
            float percentageOfLine = currentPos1D/lengthOfIterationLine;
            //Debug.Log("Percentage or line : ".Colorize(Color.blue) + percentageOfLine);
            (int, float) brushSizeAndRadius = (-1, -1f);

            switch(biggestBrushSize){
                case BiggestBrushSize.Idem:
                case BiggestBrushSize.PreviousFrame:
                    brushSizeAndRadius = GetCurrentBrushSizeAndRadius(percentageOfLine, biggestBrushSize, thisFrameBrushSize, previousFrameBrushSize); // get current brush size by calculation - interpolation
                    break;
                case BiggestBrushSize.ThisFrame:
                    brushSizeAndRadius = GetCurrentBrushSizeAndRadius(percentageOfLine, biggestBrushSize, previousFrameBrushSize, thisFrameBrushSize); // get current brush size by calculation - interpolation
                    break;
                default:
                    Debug.LogError("Error - no biggest brush size!!!");
                    break;
            }
            float currentBrushSizeRadius = brushSizeAndRadius.Item2;
            //Debug.Log("Current brush size radius on line " + currentBrushSizeRadius);
            //Debug.Log("current brush size on line : " +brushSizeAndRadius.Item1);
            
            bool strokeCanFit = false;
            // check if should add to list, if stroke can fit 
            if( currentPos1D - lastAddedBrushStrokePlusRadiusPos1D > currentBrushSizeRadius &&
                lengthOfIterationLine - currentPos1D > currentBrushSizeRadius)
            {
                strokeCanFit = true;
            }else{ //Debug.Log("Stroke can't fit!".Colorize(Color.magenta)); 
            }
            // Add stroke and update 
            if( strokeCanFit ){
                sizeOfBrushPerPoint.Add((uint)brushSizeAndRadius.Item1);
                Vector2Int pixelPosition = CalculatePixelCoordinates(currentPos);
                Pixel newPixel = new Pixel(pixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(newPixel);
                // Update
                currentPos1D += brushSizeAndRadius.Item2; // move pos to end of radius
                lastAddedBrushStrokePlusRadiusPos1D = currentPos1D; // caching 1D position
                Vector2 deltaVectorRadiusOfCurrentPositionBrushStroke = normalizedDeltaVector * currentBrushSizeRadius;
                currentPos += deltaVectorRadiusOfCurrentPositionBrushStroke;
            }
        } // end While - iteration


        // Adding the last points to the lists
        Debug.Log("Adding two last points to list");
        Vector2Int lastPixelPosition;
        Pixel lastNewPixel;
        switch(biggestBrushSize){ // end iteration list with smallest brush size
            case BiggestBrushSize.ThisFrame:
                lastPixelPosition = CalculatePixelCoordinates(pointPreviousFrame);
                lastNewPixel = new Pixel(lastPixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(lastNewPixel);
                sizeOfBrushPerPoint.Add((uint)previousFrameBrushSize);
                break;
            case BiggestBrushSize.Idem:
            case BiggestBrushSize.PreviousFrame:
                lastPixelPosition = CalculatePixelCoordinates(pointThisFrame);
                lastNewPixel = new Pixel(lastPixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(lastNewPixel);
                sizeOfBrushPerPoint.Add((uint)thisFrameBrushSize);
                break;
        }
        Debug.Log($"pixel coord array size {pixelCoordinates.Count}");
        Debug.Log($"brush sizes on iteration line - array size {sizeOfBrushPerPoint.Count}");
        return (pixelCoordinates.ToArray(), sizeOfBrushPerPoint.ToArray());
    } // End CalculatePointsOnLine(...)

    /// <summary>
    /// From big brush size to small lerp.
    /// </summary>
    /// <returns>Returns current brush size diameter in pixel width (rounded, int) 
    /// and brush size radius in percentage of image width (float) </returns>
    (int, float) GetCurrentBrushSizeAndRadius(float percentageOfLine, BiggestBrushSize biggestBrushSize, int smallestBrush, int biggestBrush){
        if(percentageOfLine > 1f || percentageOfLine <0f) { Debug.LogError("percentageOfLine must be between 0 and 1f");}
        
        if(biggestBrushSize == BiggestBrushSize.Idem){ 
            // return smallestBrush, skip lerp.
            float brushWidth = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.Brush.WidthOfBrushSize[smallestBrush]);
            float brushRadius = brushWidth/2f;
            return (smallestBrush, brushRadius);
        }else {
            // interpolate - big to small!
            // add % of difference to small brush - round to int
            float difference = biggestBrush - smallestBrush;
            int brushSizeIndex = smallestBrush + Mathf.RoundToInt(difference * percentageOfLine);
            int brushSizePixelWidth = m_DrawingPencilController.Brush.WidthOfBrushSize[brushSizeIndex];
            float brushWidth = ConvertPixelWidthToPercentageOfImageWidth(brushSizePixelWidth);
            float brushRadius = brushWidth/2f;
            return(brushSizeIndex, brushRadius);
        }
    }

    float ConvertPixelWidthToPercentageOfImageWidth(int pixelWidth){
        return (float)pixelWidth/(float)m_ImageWidth;
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
        float resistance = Mathf.Clamp( (m_DepthPositionTransform.localPosition.z + .5f), 0f, 1f );
        // if(resistance > .8f) Debug.Log("Resistance : ".Colorize(Color.white) + resistance);
        m_DrawingPencilController.HandleResistance(resistance);
    }

    void UpdateStrokeAndTargetAndDepth(){
        m_TargetPositionTransform.position = m_OtherObject.position;
        m_DepthPositionTransform.position = m_OtherObject.position;
        m_TargetPositionTransform.localPosition = new Vector3(m_TargetPositionTransform.localPosition.x, 
                                                            m_TargetPositionTransform.localPosition.y, 0f);
        m_StrokePositionTransform.localPosition = Vector2.MoveTowards(m_StrokePositionTransform.localPosition, 
                                                                    m_TargetPositionTransform.localPosition, 
                                                                    m_drawSpeed * Time.deltaTime);
    }

    void UpdateDrawingStickOffset(){
        Vector3 offset = m_TargetPositionTransform.position - m_DepthPositionTransform.position;
        m_DrawingPencilController.OffsetMainMesh(offset);
    }

    Vector2 CalculateCanvasCoordinatesRaw(){ // from scaled gameobject, y diretion stretched by 1.25
        return new Vector2(Mathf.Clamp((m_StrokePositionTransform.localPosition.x + .5f), 0, 1), 
                           Mathf.Clamp((m_StrokePositionTransform.localPosition.y + .5f), 0, 1));
    }

    /// <summary>
    /// Input unscaled canvas coordinates (same length in x and y direction).
    /// </summary>
    /// <returns>Pixel coordinates</returns>
    Vector2Int CalculatePixelCoordinates(Vector2 canvasCoordinatesUnscaled){ // 4x5 textures - so 1.25 in y direction and 1 in x, to keep unscaled vectors
        float scaledY = canvasCoordinatesUnscaled.y/1.25f;
        return new Vector2Int( 
                            Mathf.RoundToInt(canvasCoordinatesUnscaled.x * m_ImageWidth), 
                            Mathf.RoundToInt(scaledY * m_ImageHeight) );
    }


    // ---------------------
    // Update textures.. 
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
            yield return new WaitForSeconds(m_RenderTextureMipsRefreshRate);
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
