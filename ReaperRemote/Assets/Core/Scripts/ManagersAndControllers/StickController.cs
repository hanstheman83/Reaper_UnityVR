using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction;
using UnityEngine.InputSystem;
using Core.Controls;
using System;
using Core;
using UnityEngine.XR.Interaction.Toolkit;

public class StickController : MonoBehaviour, IContinousTrigger, IHitVelocity
{    
    [SerializeField] private string nameOfTriggerController;
    public string NameOfTriggerController {get => nameOfTriggerController;} // name accessible from list of all controllers implementing IContinousTrigger
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Renderer rendererOfStickHead;
    
    [Header("Materials Lerp")]
    [SerializeField] private Material materialFlat;
    [SerializeField] private Material materialGlowing;
    private ControllerHand controlledBy;
    public ControllerHand ControlledBy {get => controlledBy;}
    private DataHandler triggerDataFlow;
    public DataHandler TriggerDataFlow {get => triggerDataFlow;}
    private int hitVelocity;
    public int HitVelocity { get => hitVelocity; }
    
    private float lerp = 0f;


    private void Awake() {
        controlledBy = ControllerHand.None;
        hitVelocity = 0;
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
                    lerp = val;
                }
                // check lefthand is holding stick or ignore
                //grabInteractable.
                // https://forum.unity.com/threads/how-to-get-which-hand-is-grabbing-an-xr-grab-interactable-object.946045/
                // process input
                break;
            case ControllerHand.Right:
                if(controlledBy == ControllerHand.Right){
                    Debug.Log("Right Trigger processing".Colorize(Color.green) + (" val : "+val).Colorize(Color.white) );
                    lerp = val;
                }
                break;
            case ControllerHand.None:
                // flat material
                lerp = 0;
                break;
        }
        rendererOfStickHead.material.Lerp(materialFlat, materialGlowing, lerp);
        hitVelocity = (int)(Mathf.Round(lerp * 126f));
    

        // changing several materials
        // rendering https://answers.unity.com/questions/1685162/materialcolor-only-changing-one-instance-of-object.html 
    }

    // TODO implement shared interface for direct and ray interactor    
    public void OnSelected(SelectEnterEventArgs args){
        // set hand holding it
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        controlledBy = customDirectInteractor.ControllerHand;  
        Debug.Log($"OnSelected. Controlled by : {controlledBy}".Colorize(Color.magenta));      
    }

    public void OnDeselected(SelectExitEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        if( customDirectInteractor.ControllerHand != controlledBy){
            Debug.LogError("Name mismatch!");
        }
        // can the other controller steal the stick, prevent stealing from changing interaction layer ? 
        controlledBy = ControllerHand.None;
        rendererOfStickHead.material = materialFlat;
        Debug.Log($"OnDeselected. Controlled by : {controlledBy}".Colorize(Color.magenta));      
        lerp = 0;
        
    }


}
