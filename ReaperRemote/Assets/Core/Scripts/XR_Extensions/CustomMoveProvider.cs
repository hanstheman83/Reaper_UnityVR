using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

namespace Core.Controls{

public class CustomMoveProvider : ActionBasedContinuousMoveProvider
{
    public bool isDisabled = false;
    private bool oldState = false;
    [SerializeField]private ControlScheme_01 controlScheme_01;

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
    
}
}
