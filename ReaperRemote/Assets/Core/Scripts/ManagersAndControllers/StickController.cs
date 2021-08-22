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
    public DataHandler TriggerDataFlow {get => triggerDataFlow; set => triggerDataFlow = value;}
    private int hitVelocity;
    public int HitVelocity { get => hitVelocity; }
    
    private float lerp = 0f;
    private InputActionController inputActionController;


    private void Awake() {
        controlledBy = ControllerHand.None;
        hitVelocity = 0;
    }
    private void Start() {
        inputActionController = FindObjectOfType<InputActionController>();
    }

    private void OnDisable() {
        // 
    }

    // two controls can process input eg. trigger and grip
    public void ProcessInput(float val){
        lerp = val;
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
        inputActionController.RegisterTriggerControl(this, controlledBy);

    }
    public void OnDeselected(SelectExitEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        if( customDirectInteractor.ControllerHand != controlledBy){
            Debug.LogError("Name mismatch!");
        }
        // can the other controller steal the stick, prevent stealing from changing interaction layer ? 
        inputActionController.RemoveTriggerControl(this, controlledBy);
        controlledBy = ControllerHand.None;
        rendererOfStickHead.material = materialFlat;
        Debug.Log($"OnDeselected. Controlled by : {controlledBy}".Colorize(Color.magenta));      
        lerp = 0;
        
    }


}
