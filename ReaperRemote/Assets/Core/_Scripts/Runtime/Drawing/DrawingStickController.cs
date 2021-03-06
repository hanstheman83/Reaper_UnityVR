using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using Core.SceneManagement;

namespace Core.Drawing{


[RequireComponent(typeof(XRGrabInteractable))]
public class DrawingStickController : MonoBehaviour, IContinousTrigger
{
    [SerializeField] Transform m_ColorPickingDrawPoint;
    public Transform ColorPickingDrawPoint {get => m_ColorPickingDrawPoint;}

    public ControllerHand ControlledBy {get => m_ControlledBy;}
    ControllerHand m_ControlledBy = ControllerHand.None;
    [SerializeField] private string nameOfTriggerController;
    [SerializeField] GameObject m_PencilMesh;

    [SerializeField]Renderer m_StickRenderer;
    public Renderer StickRenderer { get => m_StickRenderer; set => m_StickRenderer = value; }
    public string NameOfTriggerController {get => nameOfTriggerController;} // name accessible from list of all controllers implementing IContinousTrigger

    public DataHandler TriggerDataFlow { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    private ButtonsProcessor m_ButtonsProcessor;
    private XRBaseController m_BaseController;
    private Coroutine haptics = null;
    public Color DrawingColor;
    public Brush m_Brush;
    int m_ActiveBrushSize = 0;
    public int ActiveBrushSize { get => m_ActiveBrushSize;}
    public bool DrawingModeActive { get => m_DrawingModeActive;}
    bool m_DrawingModeActive = false;

    private enum ResistanceLevel{
        None, Lowest, Low, Medium, High, Highest 
    }
    private ResistanceLevel resistanceLevel = ResistanceLevel.None;
    private float amplitude = 0f;
    private float delay = 1f;
    SceneManager m_SceneManager;    


    // Start is called before the first frame update
    void Start()
    {
        m_SceneManager = SceneManager.Instance;
        m_ButtonsProcessor = FindObjectOfType<ButtonsProcessor>();
        m_Brush = new Brush(5);
    }

    #region XR Callbacks
    public void OnSelectEntered(SelectEnterEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        m_ControlledBy = customDirectInteractor.ControllerHand;
        m_SceneManager.ChangeHandHoldingPencil(m_ControlledBy);
        customDirectInteractor.attachTransform.position = GetComponent<XRGrabInteractable>().attachTransform.position;
        customDirectInteractor.attachTransform.rotation = GetComponent<XRGrabInteractable>().attachTransform.rotation;
        m_BaseController = customDirectInteractor.gameObject.GetComponent<XRBaseController>(); 
        m_ButtonsProcessor.RegisterContinousTrigger(this, m_ControlledBy);
    }
    public void OnSelectExited(SelectExitEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        customDirectInteractor.attachTransform.localPosition = Vector3.zero;
        m_ButtonsProcessor.UnregisterContinousTrigger(this, m_ControlledBy);
        m_ControlledBy = ControllerHand.None;
        m_SceneManager.ChangeHandHoldingPencil(m_ControlledBy);
        m_BaseController = null;
        Debug.Log("Deselected controller.. Held by : "+ m_ControlledBy);
        m_DrawingModeActive = false;
    }
    #endregion XR Callbacks

    #region Button Callbacks
    public void ProcessTriggerInput(float val)
    {
        //Debug.Log("Processing trigger input");
        m_ActiveBrushSize = Mathf.Clamp( 
                                (Mathf.RoundToInt( val * (m_Brush.NumberOfSizes - 1) )), 0, 
                                (m_Brush.NumberOfSizes - 1)
                                );
        //Debug.Log("active brush size : "+ m_ActiveBrushSize);
    }
    #endregion Button Callbacks

    /// <summary>
    /// Offset mesh during drawing a stroke - continously updated.
    /// </summary>
    public void OffsetMainMesh(Vector3 offset){
        // offsets relative to angle of drawing board!
        m_PencilMesh.transform.localPosition = Vector3.zero;
        m_PencilMesh.transform.position += offset;
    }
    public void ReleasePencil(Vector3 newWorldPosition){
        m_BaseController.GetComponent<CustomDirectInteractor>().ForceDeselect();
        transform.position = newWorldPosition;
        Debug.Log("Forcing deselect!");
        m_ActiveBrushSize = 0;
        m_DrawingModeActive = false;
    }
    public void StartDrawingMode(){
        m_DrawingModeActive = true;
    }
    public void StopDrawingMode(){
        m_DrawingModeActive = false;
    }
    /// <summary>
    /// Start Haptic Feedback
    /// </summary>
    public void StartResistance(){ // 
        if(haptics == null) haptics = StartCoroutine(StartHaptics());
        else Debug.LogError("Haptics was already started!!");
    }
    public void StopResistance(){
        if(haptics == null) Debug.LogError("Can't stop haptics - already null!!");
        else {
            StopCoroutine(haptics);
            haptics = null;
        }
    }

    // ------------------ Haptics -------------------

    /// <summary>
    /// Incr Decr haptic feedback <br/> Input should be normalized 0 to 1
    /// </summary>
    public void HandleResistance(float resistance){ // normalized to 0-1
        // keep rotouine, change delay and amp
        if(resistance < 0f && resistance > 1f) Debug.LogError("Need value between 0 and 1");
            
        if(resistance < .2f){
            if(resistanceLevel != ResistanceLevel.Lowest){
                resistanceLevel = ResistanceLevel.Lowest;
                amplitude = .1f;
                delay = .3f;
            }
        }else if(resistance < .4f){
            if(resistanceLevel != ResistanceLevel.Low){
                resistanceLevel = ResistanceLevel.Low;
                amplitude = .4f;
                delay = .2f;
            }
        }else if(resistance < .6f){
            if(resistanceLevel != ResistanceLevel.Medium){
                resistanceLevel = ResistanceLevel.Medium;
                amplitude = .7f;
                delay = .1f;
            }
        }else if(resistance < .8f){
            if(resistanceLevel != ResistanceLevel.High){
                resistanceLevel = ResistanceLevel.High;
                amplitude = .9f;
                delay = .07f;
            }
        }else if(resistance <= 1f){
            if(resistanceLevel != ResistanceLevel.Highest){
                resistanceLevel = ResistanceLevel.Highest;
                amplitude = 1f;
                delay = .04f;
            }
        }
    }
    IEnumerator StartHaptics()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            SendHaptics();
        }
    }
    void SendHaptics()
    {   if(m_BaseController == null) Debug.Log("controller is null");
        m_BaseController?.SendHapticImpulse(amplitude, 0.03f);
    }
}

// ---------------------------------- 
// ---------------------------------- 
public struct Brush{
    // list of all brush sizes - 1D float arrays [0 to 1, alpha]
    public List<float[]> BrushSizes;
    public List<int> WidthOfBrushSize;
    public int NumberOfSizes;

    public Brush(int numberOfSizes){
        BrushSizes = new List<float[]>();
        WidthOfBrushSize = new List<int>();
        this.NumberOfSizes = numberOfSizes;
        //
        GenerateList();
    }

    void GenerateList(){ // TODO: add softness algo!!
        int brushWidth = 3;
        for (var i = 0; i < NumberOfSizes; i++)
        {
            int sizeOfNewArray = brushWidth * brushWidth; 
            float[] newBrushSizeArray = new float[sizeOfNewArray];
            for (var j = 0; j < sizeOfNewArray; j++)
            {
                newBrushSizeArray[j] = 1f;
            }
            WidthOfBrushSize.Add(brushWidth);
            BrushSizes.Add(newBrushSizeArray);
            brushWidth += 2;
        }
    }
}

}