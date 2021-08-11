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
    public bool isDisabled = false;
    private bool oldState = false;
    [SerializeField]private ControlScheme_01 controlScheme_01;

    /// <summary>
    /// Pass in and activate controls for continous movement
    /// </summary>
    /// <param name="moveAction"></param>
    public void ActivateMovement(InputActionProperty moveAction){ 
        // enum for right/left hand
    }
    public void DeactivateMovement(){
        // enum only
    }

    protected new void OnEnable(){ // should prevent base OnEnable from being called..
        leftHandMoveAction = controlScheme_01.leftHandMoveAction;
        // base.Awake();
        // base.OnEnable();
    }

    

    new void Update() {
        base.Update();
        // toggle
        if(oldState != isDisabled){
            if(isDisabled) {
                leftHandMoveAction = default;
                base.OnDisable();
                Debug.Log("disabling input");
                }
            else {
                leftHandMoveAction = controlScheme_01.leftHandMoveAction;
                base.OnEnable();

                Debug.Log("enabling input");
            }
        }
        oldState = isDisabled;
    }

    public void DisableAllControls(){

    }
    
}
}
