using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using UnityEngine.XR.Interaction.Toolkit;


public class DrawingStickController : MonoBehaviour, IContinousTrigger
{
    // Get info from drawingboard - z coord on stroke
    // send haptic

    // Test : on grab cache controller -- 
    // haptic = trigger

    ControllerHand controlledBy = ControllerHand.None;
    [SerializeField] private string nameOfTriggerController;
    public string NameOfTriggerController {get => nameOfTriggerController;} // name accessible from list of all controllers implementing IContinousTrigger

    public DataHandler TriggerDataFlow { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public ControllerHand ControlledBy {get => controlledBy;}

    private InputActionController inputActionController;
    private XRBaseController baseController;
    private Coroutine haptics = null;

    private enum ResistanceLevel{
        None, Lowest, Low, Medium, High, Highest 
    }
    private ResistanceLevel resistanceLevel = ResistanceLevel.None;


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
        baseController = customDirectInteractor.gameObject.GetComponent<XRBaseController>(); 
        inputActionController.RegisterTriggerControl(this, controlledBy);
    }
    public void OnSelectExited(SelectExitEventArgs args){
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
        //Debug.Log("val : " +val);
        // if(val > .5 && haptics == null){
        //     Debug.Log("starting haptics");
        //     haptics = StartCoroutine(StartPeriodicHaptics());
        // }else if (val < .5 && haptics != null){
        //     StopCoroutine(haptics);
        //     haptics = null;
        // }
    }

    /// <summary>
    /// Normalized 0 to 1
    /// </summary>
    public void HandleResistance(float resistance){ // normalized to 0-1
        // keep rotouine, change delay and amp
        if(resistance < 0f && resistance > 1f) Debug.LogError("Need value between 0 and 1");
        
        if(resistance == 0f) {
            resistanceLevel = ResistanceLevel.None;
            if(haptics != null) {
                StopCoroutine(haptics);
                haptics = null;
            }
        }else if(resistance < .2f){
            if(resistanceLevel != ResistanceLevel.Lowest){
                resistanceLevel = ResistanceLevel.Lowest;
                if(haptics != null) StopCoroutine(haptics);
                haptics = StartCoroutine(StartPeriodicHaptics(.2f, .2f));
            }
        }else if(resistance < .4f){
            if(resistanceLevel != ResistanceLevel.Low){
                resistanceLevel = ResistanceLevel.Low;
                if(haptics != null) StopCoroutine(haptics);
                haptics = StartCoroutine(StartPeriodicHaptics(.2f, .4f));
            }
        }else if(resistance < .6f){
            if(resistanceLevel != ResistanceLevel.Medium){
                resistanceLevel = ResistanceLevel.Medium;
                if(haptics != null) StopCoroutine(haptics);
                haptics = StartCoroutine(StartPeriodicHaptics(.2f, .6f));
            }
        }else if(resistance < .8f){
            if(resistanceLevel != ResistanceLevel.High){
                resistanceLevel = ResistanceLevel.High;
                if(haptics != null) StopCoroutine(haptics);
                haptics = StartCoroutine(StartPeriodicHaptics(.2f, .8f));
            }
        }else if(resistance <= 1f){
            if(resistanceLevel != ResistanceLevel.Highest){
                resistanceLevel = ResistanceLevel.Highest;
                if(haptics != null) StopCoroutine(haptics);
                haptics = StartCoroutine(StartPeriodicHaptics(.2f, 1f));
            }
        }
    }



    IEnumerator StartPeriodicHaptics(float delay, float amplitude)
    {
        // Trigger haptics every second
        var wait = new WaitForSeconds(delay);
 
        while (true)
        {
            yield return wait;
            SendHaptics(amplitude);
        }
    }
 
    void SendHaptics(float amplitude)
    {   if(baseController == null) Debug.Log("controller is null");
        baseController?.SendHapticImpulse(amplitude, 0.05f);
    }
}
