using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.XR;

namespace Core.Controls{

/// <summary>
/// To setup custom controls. 
/// <br/>Since this class builds on Unity's new Input system and still in production XR toolkit eventually it will be replaced <br/>
/// (as soon as Unity creates something more useful than their current mess).
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
    UnityEngine.XR.InputDevice rightXRController;
    

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

    private List<IContinousTrigger> continousLeftTriggerRegistrants;
    private List<IContinousTrigger> continousRightTriggerRegistrants;
    private List<IJoystickPress> joystickPressLeftRegistrants;
    private List<IJoystickPress> joystickPressRightRegistrants;
    private List<IPrimaryButtonDown> primaryButtonDownRightRegistrants;
    private List<IPrimaryButtonDown> primaryButtonDownLeftRegistrants;

    private ControllerHand m_MainController = ControllerHand.Right;
    private enum ControllerState { Drawing, NotDrawing }
    private ControllerState m_ControllerState = ControllerState.NotDrawing;


    #region Unity Methods
    private void Awake() {
        InitializeButtonSubscriberLists();
        InitializeMovementProviders();
        RegisterMethodsToActions();
    }
    private void Start() {
        InitializeLeftHandController();
        InitializeRightHandController();
    }
    void Update() {
        HandleXRInput();
        Debug_ToggleMainController();
    }
    #endregion Unity Methods

    #region Initializers
    void InitializeButtonSubscriberLists(){
        continousLeftTriggerRegistrants = new List<IContinousTrigger>();
        continousRightTriggerRegistrants = new List<IContinousTrigger>();
        joystickPressLeftRegistrants = new List<IJoystickPress>();
        joystickPressRightRegistrants = new List<IJoystickPress>();
        primaryButtonDownRightRegistrants = new List<IPrimaryButtonDown>();
        primaryButtonDownLeftRegistrants = new List<IPrimaryButtonDown>();
    }
    void InitializeMovementProviders(){
        customMoveProvider = FindObjectOfType<CustomMoveProvider>();
        customSnapTurnProvider = FindObjectOfType<CustomSnapTurnProvider>();
    }
    void RegisterMethodsToActions(){
        XR_leftTriggerPress.action.performed += SendLeftContinousTriggerToRegistrants;
        XR_rightTriggerPress.action.performed += SendRightContinousTriggerToRegistrants;
        XR_leftJoystickPress.action.performed += SendLeftJoystickPressToRegistrants;
    }
    void InitializeLeftHandController(){
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
    void InitializeRightHandController(){
        var rightHandDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevicesAtXRNode(UnityEngine.XR.XRNode.RightHand, rightHandDevices);
        if(rightHandDevices.Count == 0){
            Debug.LogError("Right XRController not initialized on start!");
        }
        else if(rightHandDevices.Count == 1)
        {
            rightXRController = rightHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", rightXRController.name, rightXRController.characteristics));
        }
        else if(rightHandDevices.Count > 1)
        {
            Debug.Log("Found more than one right hand!");
        }
    }
    #endregion Initializers

    #region XR Input    
    void HandleXRInput(){
        bool triggerValue;
        if (leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out triggerValue) && triggerValue)
        {
            Debug.Log("Joy button is pressed");
        }
        HandlePrimaryButtonDownLeft();
        HandlePrimaryButtonDownRight();
    }
    void HandlePrimaryButtonDownLeft(){
        leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryButtonDownLeft);
        if(primaryButtonDownLeft){
            SendPrimaryButtonDownLeftToRegistrants();
        }
    }
    void SendPrimaryButtonDownLeftToRegistrants(){
        foreach(IPrimaryButtonDown registrant in primaryButtonDownLeftRegistrants){
            registrant.ProcessPrimaryButtonDown();
        }
    }
    void HandlePrimaryButtonDownRight(){
        rightXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool primaryButtonDownRight);
        if(primaryButtonDownRight){
            Debug.Log("Primary btn right down");
            SendPrimaryButtonDownRightToRegistrants();
        }
    }
    void SendPrimaryButtonDownRightToRegistrants(){
        foreach(IPrimaryButtonDown registrant in primaryButtonDownRightRegistrants){
            registrant.ProcessPrimaryButtonDown();
        }
    }
    #endregion XR Input    


    // Debug toggle TODO: make into Jason inspector button call!
    void Debug_ToggleMainController(){
        // https://youtu.be/9udeBeQiZSc?t=334
        if(oldState != rightIsActive){
            if(rightIsActive){
                SetMainControllerToRightController();
            }else{
                SetMainControllerToLeftController();
            }
        }
        oldState = rightIsActive;
    }

    
    #region Control Setup

    void SetMainControllerToRightController(){
        m_MainController = ControllerHand.Right;
        Debug.Log("right controller is main controller"); // this is default
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
    }
    void SetMainControllerToLeftController(){
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
                M.SpecifyControllerHand();
                break;
        }
    }

    public void RegisterContinousTrigger(IContinousTrigger registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                continousLeftTriggerRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                continousRightTriggerRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }

    public void UnregisterContinousTrigger(IContinousTrigger registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = continousLeftTriggerRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = continousRightTriggerRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterPrimaryButtonDown(IPrimaryButtonDown registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                primaryButtonDownLeftRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                primaryButtonDownRightRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterPrimaryButtonDown(IPrimaryButtonDown registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = primaryButtonDownLeftRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = primaryButtonDownRightRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterJoystickPress(IJoystickPress registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                joystickPressLeftRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                joystickPressRightRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterJoystickPress(IJoystickPress registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = joystickPressLeftRegistrants.Remove(registrant);
                if(!successfullyRemoved) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = joystickPressRightRegistrants.Remove(registrant);
                if(!successfullyRemoved) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    #endregion Control Setup

    #region Control Processing
    private void SendLeftContinousTriggerToRegistrants(InputAction.CallbackContext obj){
        foreach(IContinousTrigger registrant in continousLeftTriggerRegistrants){
            registrant.ProcessTriggerInput(obj.ReadValue<float>());
        }
    }
    private void SendRightContinousTriggerToRegistrants(InputAction.CallbackContext obj){
        foreach(IContinousTrigger registrant in continousRightTriggerRegistrants){
            registrant.ProcessTriggerInput(obj.ReadValue<float>());
        }
    }
    private void SendLeftJoystickPressToRegistrants(InputAction.CallbackContext obj){
        float result = obj.ReadValue<float>();
        foreach(IJoystickPress registrant in joystickPressLeftRegistrants){
            registrant.ProcessJoystickPress(result);
        }
    }
    #endregion Control Processing

    #region Scene Callbacks
    // TODO: cleanup - are they needed ???
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