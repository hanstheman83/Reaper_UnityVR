using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.XR;

namespace Core.Controls{

/// <summary>
/// To setup custom controls in-game. Store in playerprefs
/// </summary>
public class InputActionController : MonoBehaviour
{
    public bool rightIsActive = true;
    private bool oldState = false;

    [SerializeField] private ActionBasedControllerManager leftActionBasedControllerManager;
    [SerializeField] private ActionBasedControllerManager rightActionBasedControllerManager;
    [SerializeField] private UI_InteractionController leftUI_InteractionController;
    [SerializeField] private UI_InteractionController rightUI_InteractionController;
    [SerializeField] private GameObject rightUI_Controller;
    [SerializeField] private GameObject leftUI_Controller;
    [SerializeField] private GameObject rightTeleportController;
    [SerializeField] private GameObject leftTeleportController;

    // all XR custom components
    private CustomMoveProvider customMoveProvider;
    private CustomSnapTurnProvider customSnapTurnProvider;

    // dictionary of xr control to action
    // problem : some controls use multiple controls ?

    // Directly accessed XR devices
    // IN EDITOR : TOUCH CONTROLLERS MUST BE ACTIVE BEFORE STARTING PLAYMODE!!!
    // otherwise they will never be initialized!
    UnityEngine.XR.InputDevice leftXRController;
    

    // InputActionProperties for XR components
    [SerializeField] private InputActionProperty leftMove;
    [SerializeField] private InputActionProperty rightMove;
    [SerializeField] private InputActionProperty leftTurn;
    [SerializeField] private InputActionProperty rightTurn;

    // InputActionReferences for own Input manipulations
    // https://docs.unity3d.com/Manual/xr_input.html
    [SerializeField] private InputActionReference XR_leftTriggerPress;
    [SerializeField] private InputActionReference XR_rightTriggerPress;
    [SerializeField] private InputActionReference XR_leftJoystickPress;
    [SerializeField] private InputActionReference XR_rightJoystickPress;
    // UI pie menu for joy press + move axis

    private List<IContinousTrigger> continousLeftTriggers;
    private List<IContinousTrigger> continousRightTriggers;
    private List<IJoystickPress> joystickPressLeft;
    private List<IJoystickPress> joystickPressRight;

    private ControllerHand m_MainController = ControllerHand.Right;
    private enum ControllerState { Drawing, NotDrawing }
    private ControllerState m_ControllerState = ControllerState.NotDrawing;


    
 
    #region Unity Methods
    private void Awake() {
        continousLeftTriggers = new List<IContinousTrigger>();
        continousRightTriggers = new List<IContinousTrigger>();
        joystickPressLeft = new List<IJoystickPress>();
        joystickPressRight = new List<IJoystickPress>();
        customMoveProvider = FindObjectOfType<CustomMoveProvider>();
        customSnapTurnProvider = FindObjectOfType<CustomSnapTurnProvider>();
        XR_leftTriggerPress.action.performed += ProcessLeftTrigger;
        XR_rightTriggerPress.action.performed += ProcessRightTrigger;
        XR_leftJoystickPress.action.performed += ProcessLeftJoystickPress;


    }

    private void Start() {
        // https://docs.unity.cn/2019.2/Documentation/Manual/xr_input.html
        var leftHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.LeftHand, leftHandDevices);
        if(leftHandDevices.Count == 0){
            Debug.LogError("Left XRController not initialized on start!");
        }
        else if(leftHandDevices.Count == 1)
        {
            leftXRController = leftHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", leftXRController.name, leftXRController.characteristics));
        }
        else if(leftHandDevices.Count > 1)
        {
            Debug.Log("Found more than one left hand!");
        }
    }

    void Update() {
        bool triggerValue;
        if (leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out triggerValue) && triggerValue)
        {
            Debug.Log("Joy button is pressed");
        }

        // Debug toggle
        if(oldState != rightIsActive){
            if(rightIsActive){
                SetMainController(ControllerHand.Right);
            }else{
                SetMainController(ControllerHand.Left);
            }
        }
        oldState = rightIsActive;
    }// end Update()

    #endregion Unity Methods
    

    // ----------- 
    #region Control Setup

    void SetMainController(ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_MainController = ControllerHand.Left;
                Debug.Log("left controller active");
                leftTeleportController.SetActive(true);
                leftActionBasedControllerManager.enabled = true; // TODO add this to right controller if script starts disabled...
                rightActionBasedControllerManager.enabled = false;

                SetControllerUI_State(ControllerHand.Left, false);
                SetControllerUI_State(ControllerHand.Right, true);
                
                rightTeleportController.SetActive(false);
                customMoveProvider.ActivateControl(rightMove, ControllerHand.Right);
                customSnapTurnProvider.ActivateControl(leftTurn, ControllerHand.Left);
                customMoveProvider.DeactivateControl(ControllerHand.Left);
                customSnapTurnProvider.DeactivateControl(ControllerHand.Right);
                break;
            case ControllerHand.Right:
                Debug.Log("right controller is main controller"); // this is default
                m_MainController = ControllerHand.Right;
                rightTeleportController.SetActive(true);
                rightActionBasedControllerManager.enabled = true;
                leftActionBasedControllerManager.enabled = false;

                SetControllerUI_State(ControllerHand.Left, true);
                SetControllerUI_State(ControllerHand.Right, false);

                leftTeleportController.SetActive(false);
                customMoveProvider.ActivateControl(leftMove, ControllerHand.Left);
                customSnapTurnProvider.ActivateControl(rightTurn, ControllerHand.Right);
                customMoveProvider.DeactivateControl(ControllerHand.Right);
                customSnapTurnProvider.DeactivateControl(ControllerHand.Left);
                break;
            case ControllerHand.None:
            // TODO: turn off all controls!
                Debug.LogError("Input a controller hand for main controls!");
                break;
        }
    }

    void SetControllerUI_State(ControllerHand controllerHand, bool state){ // Unity World UI
        switch(controllerHand){
            case ControllerHand.Left:
                if(state == true){
                    leftUI_InteractionController.enabled = true;
                    leftUI_Controller.SetActive(true);
                }else {
                    leftUI_Controller.SetActive(false);
                    leftUI_InteractionController.enabled = false;
                }
                break;
            case ControllerHand.Right:
                if(state == true){
                    rightUI_InteractionController.enabled = true;
                    rightUI_Controller.SetActive(true);
                }else {
                    rightUI_InteractionController.enabled = false;
                    rightUI_Controller.SetActive(false);
                }
                break;
            case ControllerHand.None:
                Debug.LogError("Please Input a correct controller hand");
                break;
            
        }
    }

    public void RegisterTriggerControl(IContinousTrigger trigger, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                continousLeftTriggers.Add(trigger);
                break;
            case ControllerHand.Right:
                continousRightTriggers.Add(trigger);
                break;
            case ControllerHand.None:
                Debug.LogError("Need to specify a controlled by hand!");
                break;
        }
    }
    public void RemoveTriggerControl(IContinousTrigger trigger, ControllerHand controllerHand){
        bool success;
        switch(controllerHand){
            case ControllerHand.Left:
                success = continousLeftTriggers.Remove(trigger);
                if(!success) {Debug.LogError("No item removed!");}
                else {Debug.Log("Item removed");}
                break;
            case ControllerHand.Right:
                success = continousRightTriggers.Remove(trigger);
                if(!success) {Debug.LogError("No item removed!");}
                else {Debug.Log("Item removed");}
                break;
            case ControllerHand.None:
                Debug.LogError("Need to specify a controlled by hand!");
                break;
        }
    }

    public void RegisterJoystickPress(IJoystickPress joystickPress, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                joystickPressLeft.Add(joystickPress);
                break;
            case ControllerHand.Right:
                joystickPressRight.Add(joystickPress);
                break;
            case ControllerHand.None:
                Debug.LogError("Need to specify a controlled by hand!");
                break;
        }
    }

    public void RemoveJoystickPress(IJoystickPress joystickPress, ControllerHand controllerHand){
        bool success;
        switch(controllerHand){
            case ControllerHand.Left:
                success = joystickPressLeft.Remove(joystickPress);
                if(!success) {Debug.LogError("No item removed!");}
                else {Debug.Log("Item removed");}
                break;
            case ControllerHand.Right:
                success = joystickPressRight.Remove(joystickPress);
                if(!success) {Debug.LogError("No item removed!");}
                else {Debug.Log("Item removed");}
                break;
            case ControllerHand.None:
                Debug.LogError("Need to specify a controlled by hand!");
                break;
        }
    }

    private void ProcessLeftTrigger(InputAction.CallbackContext obj){
        // leftTriggerPressed(obj.ReadValue<float>(), ControllerHand.Left);
        //leftTriggerPressed?.Invoke(obj.ReadValue<float>(), ControllerHand.Left);
        foreach(IContinousTrigger t in continousLeftTriggers){
            t.ProcessTriggerInput(obj.ReadValue<float>());
        }
    }
    private void ProcessRightTrigger(InputAction.CallbackContext obj){
        // rightTriggerPressed(obj.ReadValue<float>(), ControllerHand.Right);
        //rightTriggerPressed?.Invoke(obj.ReadValue<float>(), ControllerHand.Right);
        foreach(IContinousTrigger t in continousRightTriggers){
            t.ProcessTriggerInput(obj.ReadValue<float>());
        }
    }

    private void ProcessLeftJoystickPress(InputAction.CallbackContext obj){
        float result = obj.ReadValue<float>();
        
        Debug.Log($"Joy left : {result}");

        foreach(IJoystickPress j in joystickPressLeft){
            j.ProcessJoystickPress(result);
        }
    }
    #endregion Control Setup

    void Test(InputAction.CallbackContext obj){
        Debug.Log("Test!".Colorize(Color.black));
        Debug.Log("action type :" + obj.valueType);
        // Debug.Log("action type :" + obj.v);
        var v = obj.ReadValue<float>();
        Debug.Log($"value {v}".Colorize(Color.green));
    }

    #region Scene Callbacks
    public void SetControllerStateToDrawing(){
        m_ControllerState = ControllerState.Drawing;
        if(m_MainController == ControllerHand.Left){
            SetControllerUI_State(ControllerHand.Right, false);
        }else if(m_MainController == ControllerHand.Right){
            SetControllerUI_State(ControllerHand.Left, false);
        }

    }
    public void SetControllerStateToNotDrawing(){
        m_ControllerState = ControllerState.NotDrawing;
        if(m_MainController == ControllerHand.Left){
            SetControllerUI_State(ControllerHand.Right, true);
        }else if(m_MainController == ControllerHand.Right){
            SetControllerUI_State(ControllerHand.Left, true);
        }
    }

    #endregion Scene Callbacks

    #region UI callbacks
    // change control config and store in playerprefs
    public void StopMovement(){
        customMoveProvider.DeactivateControl(ControllerHand.Left);
    }

    #endregion UI callbacks
}

}