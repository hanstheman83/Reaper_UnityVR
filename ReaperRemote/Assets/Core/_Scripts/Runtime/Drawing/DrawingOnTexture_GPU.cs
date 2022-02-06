using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;  
using Core.Interactions;
using Unity.Mathematics;
using UnityEngine.Events;

namespace Core.Drawing{

public class DrawingOnTexture_GPU : MonoBehaviour
{
    
    [Header("Image Settings")]
    [SerializeField][Tooltip("Multiple of 2 -512 1024, 2048, 4096, ...")] int m_RenderTextureWidth = 1024;
    [SerializeField][Tooltip("Multiple of 2 -512 1024, 2048, 4096, ...")] int m_RenderTextureHeight = 1024;
    [SerializeField][Range(0.02f, .2f)] float m_RenderTextureMipsRefreshRate = 0.03f;
    [Header("Drawing Settings")]
    [SerializeField][Range(0.02f, 2f)][Tooltip("How fast stroke moves towards brush (slow value = delayed brush stroke)")] float m_drawSpeed = 0.02f;
    [Header("References")]
    [SerializeField] GameObject m_StrokePosition;
    [SerializeField] GameObject m_TargetPosition;
    [SerializeField] GameObject m_DepthPosition;
    [SerializeField] Transform m_ReleasePosition;
    public delegate void FinishedStroke(int[] listOfMarkedTextures); // 1 for marked texture. Index is texture # 
    public event FinishedStroke finishedStroke;
    private int[] m_CPU_MarkedTextures;
    private ComputeBuffer GPU_MarkedTextures;
    [SerializeField] ComputeShader m_DrawOnTexture_Compute;
    [Header("Exposed Fields")]
    [SerializeField] Color drawingColor;
    [Header("Render Textures")]
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
    
    // Not visible in Editor
    Transform m_StrokePositionTransform, m_TargetPositionTransform, m_DepthPositionTransform;
    DrawingStickController m_DrawingPencilController = null;
    Coroutine refreshRenderTextureMips;
    ChildTrigger childTrigger; // for XR interaction
    Collider childCollider; // XR interaction
    Collider m_PencilCollider;
    private int m_ImageWidth;
    private int m_ImageHeight;
    ComputeBuffer GPU_ActiveLayer_Buffer; // 1D array of _Pixel structs
    ComputeBuffer GPU_BrushStrokeShapeSize0;
    ComputeBuffer GPU_BrushStrokeShapeSize1; 
    ComputeBuffer GPU_BrushStrokeShapeSize2; 
    ComputeBuffer GPU_BrushStrokeShapeSize3; 
    ComputeBuffer GPU_BrushStrokeShapeSize4; 
    ComputeBuffer GPU_BrushStrokeSizesArrayLengths;
    ComputeBuffer GPU_BrushStrokeShapesWidths;
    ComputeBuffer GPU_BrushStrokeShapesOffset;
    ComputeBuffer GPU_BrushStrokePositionsOnLine_Buffer; // 1D array of _Pixel structs (positions are from 0,0)
    ComputeBuffer GPU_BrushStrokeSizesOnLine_Buffer;
    ComputeBuffer GPU_JobDone_Buffer; // dummy data - to block update loop waiting for data from compute shader
    // ---- CPU buffers : arrays for compute buffers 
    uint[] m_CPU_BrushStrokeSizesOnLine_Buffer;
    private Pixel[] m_CPU_BrushStrokePositionsOnLine_Buffer;
    private int[] m_CPU_JobDone_Buffer;
    uint[] m_CPU_BrushStrokeShapesWidths;
    int[] m_CPU_BrushStrokeShapesOffset;
    private float[] m_CPU_BrushStrokeShapeSize0;
    private float[] m_CPU_BrushStrokeShapeSize1;
    private float[] m_CPU_BrushStrokeShapeSize2;
    private float[] m_CPU_BrushStrokeShapeSize3;
    private float[] m_CPU_BrushStrokeShapeSize4;
    private uint[] m_CPU_BrushStrokeSizesArrayLengths_Buffer;
    // ---- RENDER TEXTURES, shared with GPU compute buffer
    [HideInInspector] public RenderTexture renderTexture_00;
    [HideInInspector] public RenderTexture renderTexture_01;
    [HideInInspector] public RenderTexture renderTexture_02;
    [HideInInspector] public RenderTexture renderTexture_03;
#if !UNITY_EDITOR // DirectX11 doesn't support many textures in a compute shader!
    public RenderTexture renderTexture_04;
    public RenderTexture renderTexture_05;
    public RenderTexture renderTexture_06;
    public RenderTexture renderTexture_07;
    public RenderTexture renderTexture_08;
    public RenderTexture renderTexture_09;
    public RenderTexture renderTexture_10;
    public RenderTexture renderTexture_11;
    public RenderTexture renderTexture_12;
    public RenderTexture renderTexture_13;
    public RenderTexture renderTexture_14;
    public RenderTexture renderTexture_15;
    public RenderTexture renderTexture_16;
    public RenderTexture renderTexture_17;
    public RenderTexture renderTexture_18;
    public RenderTexture renderTexture_19;
#endif
    // RW textures 
    private float4[] m_CPU_Texture00_Buffer;
    private float4[] m_CPU_Texture01_Buffer;
    private float4[] m_CPU_Texture02_Buffer;
    private float4[] m_CPU_Texture03_Buffer;
#if !UNITY_EDITOR
    private float4[] texture_04;
    private float4[] texture_05;
    private float4[] texture_06;
    private float4[] texture_07;
    private float4[] texture_08;
    private float4[] texture_09;
    private float4[] texture_10;
    private float4[] texture_11;
    private float4[] texture_12;
    private float4[] texture_13;
    private float4[] texture_14;
    private float4[] texture_15;
    private float4[] texture_16;
    private float4[] texture_17;
    private float4[] texture_18;
    private float4[] texture_19;
#endif
    //
    private ComputeBuffer GPU_Texture00_Buffer;
    private ComputeBuffer GPU_Texture01_Buffer;
    private ComputeBuffer GPU_Texture02_Buffer;
    private ComputeBuffer GPU_Texture03_Buffer;


    private enum BiggestBrushSize {ThisFrame, PreviousFrame, Idem}
    public bool IsDrawing { get => m_IsDrawing; }
    bool m_IsDrawing = false;
    bool m_ComputeShaderDispatched = false;
    Transform m_OtherObject;
    Vector2 m_PreviousStroke = new Vector2(-1f, -1f); // init
    int m_LastActiveBrushSize = 0;
    (Pixel[], uint[]) pointsOnLineTuple; // pixel positions, brush width [in pixels] per point
    

#region Unity Methods
    void Awake() {
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        childTrigger.childTriggeredEnterEvent += HandleTriggerEnter;
        childTrigger.childTriggeredExitEvent += HandleTriggerExit;
    }

    void Start()
    {
        InitImageSettings();
        InitAllRenderTextures();
        // TODO: 1. init native array for buffer - read async and copy to undo list per stroke
        // a/b x/y - on non pencil controller
        InitComputeShaderInts();
        InitTransforms();
    }
    // TODO: release texture when they are not needed- free resources [also on loading another scene!]:
    // RenderTexture.Release()  Releases the RenderTexture.
    // This function releases the hardware resources used by the render texture. The texture itself is not destroyed, and will be automatically created again when being used.
    // As with other "native engine object" types, it is important to pay attention to the lifetime of any render textures and release them when you are finished using them, as they will not be garbage collected like normal managed types.
    
    private void OnDestroy() { // TODO: shouldn't it be -= the added methods ????
        childTrigger.childTriggeredEnterEvent -= HandleTriggerEnter;
        childTrigger.childTriggeredExitEvent -= HandleTriggerExit;
    }

    // Update is called once per frame
    void Update(){
        CatchShaderDispatch();
        bool pencilReleasedDuringDrawing = m_IsDrawing == true && !m_DrawingPencilController.DrawingModeActive;
        if(!m_IsDrawing) {
            // do nothing
        }else if(pencilReleasedDuringDrawing){
            StopHandlingDrawingInput(); // TODO: push pencil back
        }else{
            UpdateStrokeAndTargetAndDepth();
            UpdateDrawingStickOffset();
            UpdateResistance();
            HandleDrawing();
        }
    }
#endregion Unity Methods



    // -------- Helper methods for Start() ---------- //
    void InitImageSettings(){
        m_ImageWidth = m_RenderTextureWidth * 4;
        m_ImageHeight = m_RenderTextureHeight * 5;
    }
    void InitTransforms(){
        m_StrokePositionTransform = m_StrokePosition.transform;
        m_TargetPositionTransform = m_TargetPosition.transform;
        m_DepthPositionTransform = m_DepthPosition.transform;
    }
    void InitComputeShaderInts(){
        m_DrawOnTexture_Compute.SetInt("_TextureWidth", m_RenderTextureWidth);
        m_DrawOnTexture_Compute.SetInt("_TextureHeight", m_RenderTextureHeight);
        m_DrawOnTexture_Compute.SetInt("_ImageWidth", m_ImageWidth);
        m_DrawOnTexture_Compute.SetInt("_ImageHeight", m_ImageHeight);
    }
    
    void InitAllRenderTextures(){
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
    }
    void InitRenderTexture(Renderer renderer, ref RenderTexture renderTexture, string name){
        int kernel = m_DrawOnTexture_Compute.FindKernel("CSMain");
        renderTexture = new RenderTexture(m_RenderTextureWidth, m_RenderTextureHeight, 0, RenderTextureFormat.ARGB32);
        renderTexture.antiAliasing = 1;
        renderTexture.anisoLevel = 0;
        renderTexture.useMipMap = true;
        renderTexture.autoGenerateMips = false;
        renderTexture.enableRandomWrite = true;
        Debug.Log("Rendertexture sRGB :" + renderTexture.sRGB); 
        renderTexture.filterMode = FilterMode.Trilinear;
        renderTexture.Create();
        renderer.material.mainTexture = renderTexture;
        m_DrawOnTexture_Compute.SetTexture(kernel, name, renderTexture);
    }

   
    // -------- Helper methods for Update() ---------- //
    bool HandleDrawing(){
        Vector2 currentStroke = CalculateCanvasCoordinatesRaw(); // scaled/stretched in y dimension
        // Conversion based on RenderTextures , 4x5 - thus scale y dimension from 1 to 1.25, basically unscalling stretch in canvas dimensions. 
        currentStroke = new Vector2(currentStroke.x, currentStroke.y * 1.25f );
        bool isFirstFrameInStroke = m_PreviousStroke.x < 0;
        if(isFirstFrameInStroke){ 
            //Debug.Log("Skipping first frame in new brush stroke..."); 
            m_PreviousStroke = currentStroke;
            m_LastActiveBrushSize = m_DrawingPencilController.ActiveBrushSize;
            return false;
        }
        bool hasPointsInLine = InitPointsOnLineCPU_Buffers(currentStroke);
        if(hasPointsInLine){
            DispatchShader(currentStroke);
            CachingDrawingData(currentStroke);
        }
        return true;
    }

    /// <summary>
    /// Initializes points for CPU Buffers.
    /// </summary>
    /// <returns>Bool : true if there are points in currently generated point arrays.</returns>
    bool InitPointsOnLineCPU_Buffers(Vector2 currentStroke){
        pointsOnLineTuple = CalculatePointsOnLine(m_PreviousStroke, currentStroke,
                                    m_LastActiveBrushSize, 
                                    m_DrawingPencilController.ActiveBrushSize); // if lastStroke = -1 calculate only 1 point
        m_CPU_BrushStrokePositionsOnLine_Buffer = pointsOnLineTuple.Item1;
        m_CPU_BrushStrokeSizesOnLine_Buffer = pointsOnLineTuple.Item2;
        if(m_CPU_BrushStrokePositionsOnLine_Buffer.Length != m_CPU_BrushStrokeSizesOnLine_Buffer.Length){
            Debug.LogError("The two arrays have uneven lengths?!");
        }
        if(m_CPU_BrushStrokePositionsOnLine_Buffer.Length <= 0){ // should never happen!
            m_PreviousStroke = currentStroke;
            return false;
        }
        return true;
    }

    /// <summary>
    /// Releases buffers when the Compute Shader is done if it was dispatched in previous frame. <br/>
    /// Effectively blocks the Update loop. 
    /// </summary>
    void CatchShaderDispatch(){
        if(m_ComputeShaderDispatched){
            ReleaseBuffers();
            m_ComputeShaderDispatched = false;
        }
    }

    void DispatchShader(Vector2 currentStroke){
        int kernel = m_DrawOnTexture_Compute.FindKernel("CSMain");
        m_DrawOnTexture_Compute.SetInt("_NumberOfBrushStrokesOnLine", m_CPU_BrushStrokePositionsOnLine_Buffer.Length);
        var structSize = sizeof(float)*4 + sizeof(uint)*2; 
        GPU_BrushStrokePositionsOnLine_Buffer = new ComputeBuffer(m_CPU_BrushStrokePositionsOnLine_Buffer.Length, structSize);
        GPU_BrushStrokePositionsOnLine_Buffer.SetData(m_CPU_BrushStrokePositionsOnLine_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokePositionsOnLine_Buffer", GPU_BrushStrokePositionsOnLine_Buffer);
        GPU_BrushStrokeSizesOnLine_Buffer = new ComputeBuffer(m_CPU_BrushStrokeSizesOnLine_Buffer.Length, sizeof(uint));
        GPU_BrushStrokeSizesOnLine_Buffer.SetData(m_CPU_BrushStrokeSizesOnLine_Buffer);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeSizesOnLine_Buffer", GPU_BrushStrokeSizesOnLine_Buffer);
        int numberOfRuns = CalculateNumberOfKernelRuns();
        SetDummyData(kernel, numberOfRuns);
        m_DrawOnTexture_Compute.Dispatch(kernel, numberOfRuns, 1, 1);
        m_ComputeShaderDispatched = true;
    }

    int CalculateNumberOfKernelRuns(){
        int numberOfRuns = 0;
        for (var i = 0; i < m_CPU_BrushStrokePositionsOnLine_Buffer.Length; i++)
        {
            int brushStrokeSize = (int)m_CPU_BrushStrokeSizesOnLine_Buffer[i];
            numberOfRuns += (   m_DrawingPencilController.m_Brush.WidthOfBrushSize[brushStrokeSize] * 
                                m_DrawingPencilController.m_Brush.WidthOfBrushSize[brushStrokeSize] );
        }
        return numberOfRuns;
    }

    /// <summary>
    /// Setting Dummy Data for the Compute Shader - to know when it is done.
    /// </summary>
    void SetDummyData(int kernel, int numberOfRuns){
        m_CPU_JobDone_Buffer = new int[numberOfRuns];
        GPU_JobDone_Buffer = new ComputeBuffer(m_CPU_JobDone_Buffer.Length, sizeof(int));
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_JobDone_Buffer", GPU_JobDone_Buffer);
    }

    /// <summary>
    /// Dummy call to Compute Shader <br/> Stops update loop until kernel is done - to delay .Release() calls.
    /// </summary>
    void ReleaseBuffers(){
        GPU_JobDone_Buffer.GetData(m_CPU_JobDone_Buffer);
        GPU_JobDone_Buffer.Release();
        GPU_BrushStrokePositionsOnLine_Buffer.Release();
        GPU_BrushStrokeSizesOnLine_Buffer.Release();
    }

    void CachingDrawingData(Vector2 currentStroke){
        m_PreviousStroke = currentStroke;
        m_LastActiveBrushSize = m_DrawingPencilController.ActiveBrushSize;
    }


            // -------------------------------------------------------------------- //
    // ------------------------------- Handling Drawing --------------------------------- //
#region Handling drawing
    void HandleTriggerEnter(Collider other) // collision - may not start a stroke!! 
    {
        m_PencilCollider = other;
        m_DrawingPencilController = other.GetComponentInParent<DrawingStickController>();
        bool isPencilHeld = !(m_DrawingPencilController.ControlledBy == Core.Controls.ControllerHand.None);
        if(isPencilHeld){ // collision is user moving board hitting pencil = ignore
            StartHandlingDrawingInput(other);
        }else{
            m_IsDrawing = false; 
            m_DrawingPencilController = null;
        }
    }
    void HandleTriggerExit(Collider other){
        bool pencilGoneThroughPaper = m_DepthPositionTransform.localPosition.z > 0f && !(m_DrawingPencilController is null);
        bool pencilNotInHand = m_DrawingPencilController is null;
        // Debug.Log("Drawing pencil controller not in hand : " + pencilNotInHand);
        if(pencilGoneThroughPaper){ // TODO: handle case : releasing pencil while drawing - null!
            m_ReleasePosition.localPosition = CalculateReleasePosition();
            m_DrawingPencilController.ReleasePencil(m_ReleasePosition.position);
            StopHandlingDrawingInput();
            CatchShaderDispatch();
            // Debug.Log("Pencil gone through paper - has stopped handling drawing input!");
        }else if(pencilNotInHand){ 
            // Debug.Log("Pencil not in hand!");
            // do nothing
        }else{
            StopHandlingDrawingInput();
            CatchShaderDispatch();
        }
        HandleFinishedStrokeEvent();
    }
    Vector3 CalculateReleasePosition(){
        return new Vector3( m_TargetPosition.transform.localPosition.x, 
                            m_TargetPosition.transform.localPosition.y,
                            m_ReleasePosition.localPosition.z);
    }
    /// <summary>
    /// A Stroke is continous drawing-lines during pencil contact with the texture (if the user is holding the pencil).
    /// </summary>
    void StartHandlingDrawingInput(Collider other){
        m_IsDrawing = true;
        m_DrawingPencilController.StartDrawingMode();
        InitDrawingData(other);
    }
    void StopHandlingDrawingInput(){
        m_IsDrawing = false;
        m_OtherObject = null;
        StopMipRefresh();
        LastMipRefresh();
        ReleaseDrawingPencil();
        ReleaseBrushStrokeShapeBuffers();
    }
    void LastMipRefresh(){
        StartCoroutine(UpdateTexturesOnce()); // need to wait
    }
    void StopMipRefresh(){
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
    }
    void ReleaseDrawingPencil(){
        m_DrawingPencilController.StopResistance();
        m_DrawingPencilController.OffsetMainMesh(Vector3.zero);
        m_DrawingPencilController.StopDrawingMode();
        m_DrawingPencilController = null;
    }
    void ReleaseBrushStrokeShapeBuffers(){
        GPU_BrushStrokeShapeSize0.Release();
        GPU_BrushStrokeShapeSize1.Release();
        GPU_BrushStrokeShapeSize2.Release();
        GPU_BrushStrokeShapeSize3.Release();
        GPU_BrushStrokeShapeSize4.Release();
        GPU_BrushStrokeSizesArrayLengths.Release();
        GPU_BrushStrokeShapesWidths.Release();
        GPU_BrushStrokeShapesOffset.Release();
    }

    private void HandleFinishedStrokeEvent(){
        GPU_MarkedTextures.GetData(m_CPU_MarkedTextures);
        finishedStroke?.Invoke(m_CPU_MarkedTextures); // For saving state
        GPU_MarkedTextures.Release();

    }

    /// <summary>
    /// Initialize drawing data - start of a stroke.
    /// </summary>
    private void InitDrawingData(Collider other)
    {
        m_OtherObject = other.transform.Find("DrawPoint");
        m_PreviousStroke = new Vector2(-1f, -1f); // skip first frame, no line length!
        drawingColor = m_DrawingPencilController.DrawingColor;
        m_DrawingPencilController.StartResistance();
        m_StrokePositionTransform.position = m_OtherObject.position;
        m_DepthPositionTransform.position = m_OtherObject.position;
        m_StrokePositionTransform.localPosition = new Vector3(m_StrokePositionTransform.localPosition.x,
                                                            m_StrokePositionTransform.localPosition.y, 0f);
        if (refreshRenderTextureMips == null) {
            refreshRenderTextureMips = StartCoroutine(UpdateRendureTextureMips());
            Debug.Log("starting coroutine mips refresh");
        }
        m_TargetPositionTransform.position = m_OtherObject.position;
        m_TargetPositionTransform.localPosition = new Vector3(m_TargetPositionTransform.localPosition.x,
                                                            m_TargetPositionTransform.localPosition.y, 0f);
        // For compute shader - GPU
        int kernel = m_DrawOnTexture_Compute.FindKernel("CSMain");
        m_CPU_BrushStrokeShapeSize0 = m_DrawingPencilController.m_Brush.BrushSizes[0];
        m_CPU_BrushStrokeShapeSize1 = m_DrawingPencilController.m_Brush.BrushSizes[1];
        m_CPU_BrushStrokeShapeSize2 = m_DrawingPencilController.m_Brush.BrushSizes[2];
        m_CPU_BrushStrokeShapeSize3 = m_DrawingPencilController.m_Brush.BrushSizes[3];
        m_CPU_BrushStrokeShapeSize4 = m_DrawingPencilController.m_Brush.BrushSizes[4];
        m_CPU_BrushStrokeShapesWidths = new uint[m_DrawingPencilController.m_Brush.NumberOfSizes];
        m_CPU_BrushStrokeShapesOffset = new int[m_DrawingPencilController.m_Brush.NumberOfSizes];
        m_CPU_BrushStrokeShapesOffset[0] = -1;
        m_CPU_BrushStrokeShapesOffset[1] = -2;
        m_CPU_BrushStrokeShapesOffset[2] = -3;
        m_CPU_BrushStrokeShapesOffset[3] = -4;
        m_CPU_BrushStrokeShapesOffset[4] = -5;
        m_CPU_BrushStrokeSizesArrayLengths_Buffer = new uint[m_DrawingPencilController.m_Brush.NumberOfSizes];

        for (var i = 0; i < m_CPU_BrushStrokeSizesArrayLengths_Buffer.Length; i++)
        {
            m_CPU_BrushStrokeSizesArrayLengths_Buffer[i] = (uint)m_DrawingPencilController.m_Brush.BrushSizes[i].Length;
            m_CPU_BrushStrokeShapesWidths[i] = (uint)m_DrawingPencilController.m_Brush.WidthOfBrushSize[i];
        }

        m_CPU_MarkedTextures = new int[20];

        GPU_MarkedTextures = new ComputeBuffer(m_CPU_MarkedTextures.Length, sizeof(int));

        GPU_BrushStrokeShapeSize0 = new ComputeBuffer(m_CPU_BrushStrokeShapeSize0.Length, sizeof(float));
        GPU_BrushStrokeShapeSize1 = new ComputeBuffer(m_CPU_BrushStrokeShapeSize1.Length, sizeof(float));
        GPU_BrushStrokeShapeSize2 = new ComputeBuffer(m_CPU_BrushStrokeShapeSize2.Length, sizeof(float));
        GPU_BrushStrokeShapeSize3 = new ComputeBuffer(m_CPU_BrushStrokeShapeSize3.Length, sizeof(float));
        GPU_BrushStrokeShapeSize4 = new ComputeBuffer(m_CPU_BrushStrokeShapeSize4.Length, sizeof(float));
        GPU_BrushStrokeSizesArrayLengths = new ComputeBuffer(m_CPU_BrushStrokeSizesArrayLengths_Buffer.Length, sizeof(uint));
        GPU_BrushStrokeShapesWidths = new ComputeBuffer(m_CPU_BrushStrokeShapesWidths.Length, sizeof(uint));
        GPU_BrushStrokeShapesOffset = new ComputeBuffer(m_CPU_BrushStrokeShapesOffset.Length, sizeof(int));

        GPU_MarkedTextures.SetData(m_CPU_MarkedTextures);

        GPU_BrushStrokeShapeSize0.SetData(m_CPU_BrushStrokeShapeSize0);
        GPU_BrushStrokeShapeSize1.SetData(m_CPU_BrushStrokeShapeSize1);
        GPU_BrushStrokeShapeSize2.SetData(m_CPU_BrushStrokeShapeSize2);
        GPU_BrushStrokeShapeSize3.SetData(m_CPU_BrushStrokeShapeSize3);
        GPU_BrushStrokeShapeSize4.SetData(m_CPU_BrushStrokeShapeSize4);
        GPU_BrushStrokeSizesArrayLengths.SetData(m_CPU_BrushStrokeSizesArrayLengths_Buffer);
        GPU_BrushStrokeShapesWidths.SetData(m_CPU_BrushStrokeShapesWidths);
        GPU_BrushStrokeShapesOffset.SetData(m_CPU_BrushStrokeShapesOffset);

        m_DrawOnTexture_Compute.SetBuffer(kernel,"_MarkedTextures_Buffer", GPU_MarkedTextures);

        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize0_Buffer", GPU_BrushStrokeShapeSize0);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize1_Buffer", GPU_BrushStrokeShapeSize1);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize2_Buffer", GPU_BrushStrokeShapeSize2);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize3_Buffer", GPU_BrushStrokeShapeSize3);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapeSize4_Buffer", GPU_BrushStrokeShapeSize4);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeSizesArrayLengths_Buffer", GPU_BrushStrokeSizesArrayLengths);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapesWidths_Buffer", GPU_BrushStrokeShapesWidths);
        m_DrawOnTexture_Compute.SetBuffer(kernel, "_BrushStrokeShapesOffset_Buffer", GPU_BrushStrokeShapesOffset);
    }
#endregion Handling drawing



#region Drawing Methods
    /// <summary>
    /// Calculated line can start from either point - dependent on what point (this frame or last) has largest brush size.
    /// </summary>
    /// <returns>Pixel Coordinates and brush widths in pixel sizes.</returns>
    (Pixel[], uint[]) CalculatePointsOnLine(Vector2 pointPreviousFrame, Vector2 pointThisFrame, int previousFrameBrushSize, int thisFrameBrushSize){ 
        BiggestBrushSize biggestBrushSize = CalculateFrameWithBiggestBrushSize(previousFrameBrushSize, thisFrameBrushSize);
        float distanceBetweenBrushHits = Vector2.Distance(pointThisFrame, pointPreviousFrame); // magnitude of delta vector
        List<uint> sizeOfBrushPerPoint = new List<uint>(); // from biggest brush stroke to smallest
        List<Pixel> pixelCoordinates = new List<Pixel>(); // from biggest brush stroke to smallest
        // Adding first points to list, before starting the iteration. Calculate deltavector
        // flip vector direction if brush size is reversed! Iteration will also be reversed, iterate from big to small brush size
        // TODO: Nice to have : center the pixel - avoid rounding error. Subtract .5 of pixel width and length for correct position!
        Vector2 deltaVector = Vector2.zero;
        Vector2Int firstPixelPosition;
        Pixel firstNewPixel;
        switch(biggestBrushSize){
            case BiggestBrushSize.Idem: // normal, iterate starting from lastFrameStroke to thisFrameStroke
            case BiggestBrushSize.PreviousFrame:
                deltaVector = pointThisFrame - pointPreviousFrame;
                firstPixelPosition = CalculatePixelCoordinates(pointPreviousFrame);
                firstNewPixel = new Pixel(firstPixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(firstNewPixel);
                sizeOfBrushPerPoint.Add((uint)previousFrameBrushSize);
                break;
            case BiggestBrushSize.ThisFrame:
                deltaVector = pointPreviousFrame - pointThisFrame;
                firstPixelPosition = CalculatePixelCoordinates(pointThisFrame);
                firstNewPixel = new Pixel(firstPixelPosition, m_DrawingPencilController.DrawingColor);
                pixelCoordinates.Add(firstNewPixel);
                sizeOfBrushPerPoint.Add((uint)thisFrameBrushSize);
                break;
            default:
                break;
        }
        if(deltaVector.x == 0) { deltaVector.x = 0.000001f; }
        if(deltaVector.y == 0) { deltaVector.y = 0.000001f; }
        // -- Preparing Iteration
        // Calculate radiuses. Brush size is in pixel width (diameter)
        float radiusPreviousStroke = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.m_Brush.WidthOfBrushSize[previousFrameBrushSize]) /2f;
        float radiusThisStroke = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.m_Brush.WidthOfBrushSize[thisFrameBrushSize]) /2f;
        // Calculate stepSize based on smallest brush size radius
        float stepSize = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.m_Brush.WidthOfBrushSize[0]) /2f; // stepSize is the radius of the smallest brush
        // scaling normalized deltaVector by stepSize
        Vector2 normalizedDeltaVector = (deltaVector/distanceBetweenBrushHits);
        Vector2 stepSizedDeltaVector =  normalizedDeltaVector * stepSize;
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
        float lengthOfIterationLine = Vector2.Distance(iterationLineEnd, iterationLineStart);
        bool shouldIterate = true; // stop iteration when overlaping last point + radius of that point's brush size. 
        float currentPos1D = 0f;
        Vector2 currentPos = iterationLineStart;
        Vector2 lastAddedBrushStrokePlusRadiusPos = iterationLineStart; // 
        float lastAddedBrushStrokePlusRadiusPos1D = 0; // add brush radius and see if will fit! 

        // Iteration
        while(shouldIterate){  // calculate next step - (start of line is on circumference of first brushstroke)
            currentPos += stepSizedDeltaVector;
            currentPos1D += stepSize;
            if(currentPos1D >= lengthOfIterationLine){
                shouldIterate = false;
                break;
            }
            float percentageOfLine = currentPos1D/lengthOfIterationLine;  // Calculate brush size, % of line from start to end
            (int, float) brushSizeAndRadius = (-1, -1f);  // dummy data, tuple needs ini-vars
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
            bool strokeCanFit = false;  // check if should add to list, if stroke can fit 
            if( currentPos1D - lastAddedBrushStrokePlusRadiusPos1D > currentBrushSizeRadius &&
                lengthOfIterationLine - currentPos1D > currentBrushSizeRadius)
            {
                strokeCanFit = true;
            }else{ 
            }
            if( strokeCanFit ){ // Add stroke and update 
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

        Vector2Int lastPixelPosition;  // Adding the last points to the lists
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

        return (pixelCoordinates.ToArray(), sizeOfBrushPerPoint.ToArray());
    }  // End of crazy long algo I don't know how to clean :(

    BiggestBrushSize CalculateFrameWithBiggestBrushSize(int previousFrameBrushSize, int thisFrameBrushSize){
        if(previousFrameBrushSize == thisFrameBrushSize) { return BiggestBrushSize.Idem; }
        else if(previousFrameBrushSize > thisFrameBrushSize) { return BiggestBrushSize.PreviousFrame; }
        else if(previousFrameBrushSize < thisFrameBrushSize) { return BiggestBrushSize.ThisFrame; } 
        else{ 
            Debug.LogError("Faulty biggest brush size calculation");
            return default; 
            }
    }
    /// <summary>
    /// From big brush size to small lerp.
    /// </summary>
    /// <returns>Returns current brush size diameter in pixel width (rounded, int) 
    /// and brush size radius in percentage of image width (float) </returns>
    (int, float) GetCurrentBrushSizeAndRadius(float percentageOfLine, BiggestBrushSize biggestBrushSize, int smallestBrush, int biggestBrush){
        if(percentageOfLine > 1f || percentageOfLine <0f) { Debug.LogError("percentageOfLine must be between 0 and 1f");}
        
        if(biggestBrushSize == BiggestBrushSize.Idem){ 
            // return smallestBrush, skip lerp.
            float brushWidth = ConvertPixelWidthToPercentageOfImageWidth(m_DrawingPencilController.m_Brush.WidthOfBrushSize[smallestBrush]);
            float brushRadius = brushWidth/2f;
            return (smallestBrush, brushRadius);
        }else {
            // interpolate - big to small!
            // add % of difference to small brush - round to int
            float difference = biggestBrush - smallestBrush;
            int brushSizeIndex = smallestBrush + Mathf.RoundToInt(difference * percentageOfLine);
            int brushSizePixelWidth = m_DrawingPencilController.m_Brush.WidthOfBrushSize[brushSizeIndex];
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
            RefreshAllRenderTexturesMips();
            yield return new WaitForSeconds(m_RenderTextureMipsRefreshRate);
        }
    }
    IEnumerator UpdateTexturesOnce(){
        yield return new WaitForEndOfFrame();
        RefreshAllRenderTexturesMips();
    }
    void RefreshAllRenderTexturesMips(){
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

}