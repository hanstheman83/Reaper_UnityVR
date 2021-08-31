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
        if(haptics != null) {
            StopCoroutine(haptics);
            haptics = null;
        }
    }

    public void ProcessInput(float val)
    {
        //Debug.Log("val : " +val);
        if(val > .5 && haptics == null){
            Debug.Log("starting haptics");
            haptics = StartCoroutine(StartPeriodicHaptics());
        }else if (val < .5 && haptics != null){
            StopCoroutine(haptics);
            haptics = null;
        }
    }

    IEnumerator StartPeriodicHaptics()
    {
        // Trigger haptics every second
        var delay = new WaitForSeconds(1f);
 
        while (true)
        {
            yield return delay;
            SendHaptics();
        }
    }
 
    void SendHaptics()
    {   if(baseController == null) Debug.Log("controller is null");
        baseController?.SendHapticImpulse(0.7f, 0.1f);
    }
}
