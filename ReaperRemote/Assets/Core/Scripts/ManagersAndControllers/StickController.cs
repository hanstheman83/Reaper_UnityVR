using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction;
using UnityEngine.InputSystem;
using Core.Controls;
using System;
using Core;
using UnityEngine.XR.Interaction.Toolkit;

public class StickController : MonoBehaviour, IContinousTrigger
{
    [SerializeField] private string nameOfTriggerController;
    [SerializeField] private Renderer rendererOfStickHead;
    public string NameOfTriggerController {get => nameOfTriggerController;} // name accessible from list of all controllers implementing IContinousTrigger
    private DataHandler triggerDataFlow;
    // private string interactorName;
    private ControllerHand controlledBy;
    public ControllerHand ControlledBy {get => controlledBy;}
    public DataHandler TriggerDataFlow {get => triggerDataFlow;}
    [SerializeField] private XRGrabInteractable grabInteractable;


    private void Awake() {
        controlledBy = ControllerHand.None;
    }


    // need both controllers registered - will filter on hand attachment!
    public void RegisterTriggerControl(InputActionController inputActionController, ControllerHand hand, DataHandler dataFlow)
    {
        switch(hand){
            case ControllerHand.Left:
                inputActionController.leftTriggerPressed += ProcessInput;
                break;
            case ControllerHand.Right:
                inputActionController.rightTriggerPressed += ProcessInput;
                break;
        }

        triggerDataFlow = dataFlow;
    }

    // two controls can process input eg. trigger and grip
    private void ProcessInput(float val, ControllerHand hand){
        switch(hand){
            case ControllerHand.Left:
                if(controlledBy == ControllerHand.Left){
                    Debug.Log("Left Trigger processing".Colorize(Color.green) + (" val : "+val).Colorize(Color.white) );
                }
                // check lefthand is holding stick or ignore
                //grabInteractable.
                // https://forum.unity.com/threads/how-to-get-which-hand-is-grabbing-an-xr-grab-interactable-object.946045/
                // process input
                break;
            case ControllerHand.Right:
                if(controlledBy == ControllerHand.Right){
                    Debug.Log("Right Trigger processing".Colorize(Color.green) + (" val : "+val).Colorize(Color.white) );
                }
                break;
            case ControllerHand.None:
                break;
        }

        // Switch based on Hand holding it..
        // Get the velocity from this component!

        // rendering https://answers.unity.com/questions/1685162/materialcolor-only-changing-one-instance-of-object.html 

        // process based on triggerDataFlow
        // Debug.Log($"val : {val}".Colorize(Color.black));
    }

    // TODO implement shared interface for direct and ray interactor    
    public void OnSelected(SelectEnterEventArgs args){
        // set hand holding it
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        controlledBy = customDirectInteractor.ControllerHand;        
    }

    public void OnDeselected(SelectExitEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        if( customDirectInteractor.ControllerHand != controlledBy){
            Debug.LogError("Name mismatch!");
        }
        // can the other controller steal the stick, prevent stealing from changing interaction layer ? 
        controlledBy = ControllerHand.None;
        
    }


}
