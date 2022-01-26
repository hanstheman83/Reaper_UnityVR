using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.XR;

namespace Core.Controls{

/// <summary>
/// To setup input actions. 
/// <br/>Since this class builds on Unity's new Input system and still in production XR toolkit eventually it will be replaced <br/>
/// (as soon as Unity creates something more useful than their current mess).
/// </summary>
public class InputActionController : MonoBehaviour
{
    // [SerializeField] private ActionBasedControllerManager leftActionBasedControllerManager;
    // [SerializeField] private ActionBasedControllerManager rightActionBasedControllerManager;
    
    // dictionary of xr control to action
    // problem : some controls use multiple controls ?

    // InputActionProperties for XR components
    // [SerializeField] private InputActionProperty leftMove;
    // [SerializeField] private InputActionProperty rightMove;
    // [SerializeField] private InputActionProperty leftTurn;
    // [SerializeField] private InputActionProperty rightTurn;

    // InputActionReferences for own Input manipulations
    // https://docs.unity3d.com/Manual/xr_input.html
    // [SerializeField] private InputActionReference XR_leftTriggerPress;
    // [SerializeField] private InputActionReference XR_rightTriggerPress;
    // [SerializeField] private InputActionReference XR_leftJoystickPress;
    // [SerializeField] private InputActionReference XR_rightJoystickPress;
    // UI pie menu for joy press + move axis

    
    
   
    #region Unity Methods
    private void Awake() {
        
        //RegisterMethodsToActions();
    }
    private void Start() {
    }
    void Update() {
    }
    #endregion Unity Methods

    #region Initializers
    
    
    
    void RegisterMethodsToActions(){
        // XR_leftTriggerPress.action.performed += SendLeftContinousTrigger;
        // XR_rightTriggerPress.action.performed += SendRightContinousTriggerToRegistrants;
        // XR_leftJoystickPress.action.performed += SendLeftJoystickPressToRegistrants;
    }
    
    #endregion Initializers

    #region XR Input    
    
    
    #endregion XR Input    




    
}

}