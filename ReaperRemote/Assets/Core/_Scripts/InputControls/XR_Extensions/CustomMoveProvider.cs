using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Core.Controls{

/// <summary>
/// Custom version of ActionBasedContinuousMoveProvider from Unity XR
/// </summary>
public class CustomMoveProvider : ActionBasedContinuousMoveProvider, IXR_CustomControls
{
    // public bool isDisabled = false;
    // private bool oldState = false;
    // [SerializeField]private ControlScheme_01 controlScheme_01;

    /// <summary>
    /// Pass in and activate controls for continous movement
    /// </summary>
    public void ActivateControl(InputActionProperty moveAction, ControllerHand controllerHand){ 
        switch(controllerHand){
            case ControllerHand.Left:
                leftHandMoveAction = moveAction;
                break;
            case ControllerHand.Right:
                rightHandMoveAction = moveAction;
                break;
        }
    }
    public void DeactivateControl(ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                leftHandMoveAction = default;
                break;
            case ControllerHand.Right:
                rightHandMoveAction = default;
                break;
        }
    }

    protected new void OnEnable(){ // should prevent base OnEnable from being called..
        //leftHandMoveAction = controlScheme_01.leftHandMoveAction;
    }
    protected new void OnDisable(){

    }

    

    // new void Update() {
    //     base.Update();
    //     // toggle
    //     if(oldState != isDisabled){
    //         if(isDisabled) {
    //             leftHandMoveAction = default;
    //             base.OnDisable();
    //             Debug.Log("disabling input");
    //             }
    //         else {
    //             leftHandMoveAction = controlScheme_01.leftHandMoveAction;
    //             base.OnEnable();

    //             Debug.Log("enabling input");
    //         }
    //     }
    //     oldState = isDisabled;
    // }

    public void DisableAllControls(){

    }

    public void DeactivateComponent()
    {
        this.enabled = false;
    }

    public void ActivateComponent()
    {
        this.enabled = true;
    }
}
}
