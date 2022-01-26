using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

namespace Core.Controls{

public class CustomSnapTurnProvider : ActionBasedSnapTurnProvider, IXR_CustomControls
{
    public void DisableAllControls()
    {
        throw new System.NotImplementedException();
    }

    public void ActivateControl(InputActionProperty turnAction, ControllerHand controllerHand){ 
        switch(controllerHand){
            case ControllerHand.Left:
                leftHandSnapTurnAction = turnAction;
                break;
            case ControllerHand.Right:
                rightHandSnapTurnAction = turnAction;
                break;
        }
    }
    public void DeactivateControl(ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                leftHandSnapTurnAction = default;
                break;
            case ControllerHand.Right:
                rightHandSnapTurnAction = default;
                break;
        }
    }

   protected new void OnEnable(){ // should prevent base OnEnable from being called..
    }
    protected new void OnDisable(){

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