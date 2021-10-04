using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using Core;

[RequireComponent(typeof(XRGrabInteractable))]
public class DrawingStickController : MonoBehaviour, IContinousTrigger
{
    // Get info from drawingboard - z coord on stroke
    // send haptic

    // Test : on grab cache controller -- 
    // haptic = trigger

    [SerializeField] Transform m_ColorPickingDrawPoint;
    public Transform ColorPickingDrawPoint {get => m_ColorPickingDrawPoint;}

    ControllerHand controlledBy = ControllerHand.None;
    [SerializeField] private string nameOfTriggerController;
    public Renderer stickRenderer;
    public string NameOfTriggerController {get => nameOfTriggerController;} // name accessible from list of all controllers implementing IContinousTrigger

    public DataHandler TriggerDataFlow { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public ControllerHand ControlledBy {get => controlledBy;}

    private InputActionController inputActionController;
    private XRBaseController baseController;
    private Coroutine haptics = null;
    public Color DrawingColor;

    private enum ResistanceLevel{
        None, Lowest, Low, Medium, High, Highest 
    }
    private ResistanceLevel resistanceLevel = ResistanceLevel.None;
    private float amplitude = 0f;
    private float delay = 1f;


    // Start is called before the first frame update
    void Start()
    {
        inputActionController = FindObjectOfType<InputActionController>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelectEntered(SelectEnterEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        controlledBy = customDirectInteractor.ControllerHand;
        customDirectInteractor.attachTransform.position = GetComponent<XRGrabInteractable>().attachTransform.position;
        customDirectInteractor.attachTransform.rotation = GetComponent<XRGrabInteractable>().attachTransform.rotation;
        baseController = customDirectInteractor.gameObject.GetComponent<XRBaseController>(); 
        inputActionController.RegisterTriggerControl(this, controlledBy);
    }
    public void OnSelectExited(SelectExitEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        customDirectInteractor.attachTransform.localPosition = Vector3.zero;
        inputActionController.RemoveTriggerControl(this, controlledBy);
        controlledBy = ControllerHand.None;
        baseController = null;
        // if(haptics != null) {
        //     StopCoroutine(haptics);
        //     haptics = null;
        // }
    }

    public void ProcessTriggerInput(float val) // called from inputActionController
    {
        
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
                // Debug.Log("Ristance level highest!".Colorize(Color.magenta));
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
    {   if(baseController == null) Debug.Log("controller is null");
        baseController?.SendHapticImpulse(amplitude, 0.03f);
    }
}
