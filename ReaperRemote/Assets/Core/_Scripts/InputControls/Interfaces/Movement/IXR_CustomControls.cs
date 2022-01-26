using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Controls{

    

public interface IXR_CustomControls
{
    public void DisableAllControls();
    public void ActivateControl(InputActionProperty moveAction, ControllerHand controllerHand);
    public void DeactivateControl(ControllerHand controllerHand);
    public void DeactivateComponent();
    public void ActivateComponent();
     
}

}