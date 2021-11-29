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
    // Singleton
    private static InputActionController m_InputActionController;
    public static InputActionController Instance { get => m_InputActionController; }
    
    // Debug
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

    private List<IContinousTrigger> m_LeftTriggerContinousRegistrants;
    private List<IContinousTrigger> m_RightTriggerContinousRegistrants;
    private List<IPrimaryButtonDown> m_PrimaryButtonRightDownRegistrants;
    private List<IPrimaryButtonDown> m_PrimaryButtonLeftDownRegistrants;
    private List<IPrimaryButtonUp> m_PrimaryButtonLeftUpRegistrants;
    private List<IPrimaryButtonUp> m_PrimaryButtonRightUpRegistrants;
    private List<IPrimaryButtonContinous> m_PrimaryButtonLeftContinousRegistrants;
    private List<IPrimaryButtonContinous> m_PrimaryButtonRightContinousRegistrants;
    private List<ISecondaryButtonDown> m_SecondaryButtonLeftDownRegistrants;
    private List<ISecondaryButtonDown> m_SecondaryButtonRightDownRegistrants;
    private List<ISecondaryButtonUp> m_SecondaryButtonLeftUpRegistrants;
    private List<ISecondaryButtonUp> m_SecondaryButtonRightUpRegistrants;
    private List<ISecondaryButtonContinous> m_SecondaryButtonLeftContinousRegistrants;
    private List<ISecondaryButtonContinous> m_SecondaryButtonRightContinousRegistrants;

    private List<IJoystickPress> joystickPressLeftRegistrants;
    private List<IJoystickPress> joystickPressRightRegistrants;
    private ControllerHand m_MainController = ControllerHand.Right;
    private enum ControllerState { Drawing, NotDrawing }
    private ControllerState m_ControllerState = ControllerState.NotDrawing;
    bool m_PrimaryButtonLeftPressedPrevFrame = false;
    bool m_PrimaryButtonRightPressedPrevFrame = false;
    bool m_SecondaryButtonLeftPressedPrevFrame = false;
    bool m_SecondaryButtonRightPressedPrevFrame = false;
    


    #region Unity Methods
    private void Awake() {
        InitializeAsSingleton();
        InitializeButtonRegistrantLists();
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
    void InitializeAsSingleton(){
        bool instanceExists = m_InputActionController != null && m_InputActionController != this;
        if(instanceExists){
            Debug.LogError("Only 1 InputActionController per scene!");
            Destroy(this.gameObject);
        }else{
            m_InputActionController = this;
        }
    }
    void InitializeButtonRegistrantLists(){
        m_LeftTriggerContinousRegistrants = new List<IContinousTrigger>();
        m_RightTriggerContinousRegistrants = new List<IContinousTrigger>();
        joystickPressLeftRegistrants = new List<IJoystickPress>();
        joystickPressRightRegistrants = new List<IJoystickPress>();
        m_PrimaryButtonLeftDownRegistrants = new List<IPrimaryButtonDown>();
        m_PrimaryButtonRightDownRegistrants = new List<IPrimaryButtonDown>();
        m_PrimaryButtonLeftUpRegistrants = new List<IPrimaryButtonUp>();
        m_PrimaryButtonRightUpRegistrants = new List<IPrimaryButtonUp>();
        m_PrimaryButtonLeftContinousRegistrants = new List<IPrimaryButtonContinous>();
        m_PrimaryButtonRightContinousRegistrants = new List<IPrimaryButtonContinous>();
        m_SecondaryButtonLeftDownRegistrants = new List<ISecondaryButtonDown>();
        m_SecondaryButtonRightDownRegistrants = new List<ISecondaryButtonDown>();
        m_SecondaryButtonLeftUpRegistrants = new List<ISecondaryButtonUp>();
        m_SecondaryButtonRightUpRegistrants = new List<ISecondaryButtonUp>();
        m_SecondaryButtonLeftContinousRegistrants = new List<ISecondaryButtonContinous>();
        m_SecondaryButtonRightContinousRegistrants = new List<ISecondaryButtonContinous>();
    }
    void InitializeMovementProviders(){
        customMoveProvider = FindObjectOfType<CustomMoveProvider>();
        customSnapTurnProvider = FindObjectOfType<CustomSnapTurnProvider>();
    }
    void RegisterMethodsToActions(){
        XR_leftTriggerPress.action.performed += SendLeftContinousTrigger;
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
        HandlePrimaryButtonLeft();
        HandlePrimaryButtonRight();
        HandleSecondaryButtonLeft();
        HandleSecondaryButtonRight();

    }
    void HandleJoystickButtonLeft(){
        // bool triggerValue;
        // if (leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out triggerValue) && triggerValue)
        // {
        //     Debug.Log("Joy button is pressed");
        // }
    }
    void HandlePrimaryButtonLeft(){
        bool primaryButtonLeftPressed = GetPrimaryButtonLeft(); 
        bool primaryButtonLeftDown = !m_PrimaryButtonLeftPressedPrevFrame && primaryButtonLeftPressed;
        bool primaryButtonLeftUp = m_PrimaryButtonLeftPressedPrevFrame && !primaryButtonLeftPressed;        
        SendPrimaryButtonLeftContinous(primaryButtonLeftPressed);
        if(primaryButtonLeftDown){
            SendPrimaryButtonLeftDown();
        }else if(primaryButtonLeftUp){
            SendPrimaryButtonLeftUp();
        }
        m_PrimaryButtonLeftPressedPrevFrame = primaryButtonLeftPressed;
    }
    void HandlePrimaryButtonRight(){
        bool primaryButtonRightPressed = GetPrimaryButtonRight();
        bool primaryButtonRightDown = !m_PrimaryButtonRightPressedPrevFrame && primaryButtonRightPressed;
        bool primaryButtonRightUp = m_PrimaryButtonRightPressedPrevFrame && !primaryButtonRightPressed;
        SendPrimaryButtonRightContinous(primaryButtonRightPressed);
        if(primaryButtonRightDown){ 
            SendPrimaryButtonRightDown();
        }else if(primaryButtonRightUp){
            SendPrimaryButtonRightUp();
        }
        m_PrimaryButtonRightPressedPrevFrame = primaryButtonRightPressed;
    }
    void HandleSecondaryButtonLeft(){
        bool secondaryButtonLeftPressed = GetSecondaryButtonLeft();
        bool secondaryButtonLeftDown = !m_SecondaryButtonLeftPressedPrevFrame && secondaryButtonLeftPressed;
        bool secondaryButtonLeftUp = m_SecondaryButtonLeftPressedPrevFrame && !secondaryButtonLeftPressed;
        SendSecondaryButtonLeftContinous(secondaryButtonLeftPressed);
        if(secondaryButtonLeftDown){
            SendSecondaryButtonLeftDown();
        }else if(secondaryButtonLeftUp){
            SendSecondaryButtonLeftUp();
        }
        m_SecondaryButtonLeftPressedPrevFrame = secondaryButtonLeftPressed;
    }
    void HandleSecondaryButtonRight(){
        bool secondaryButtonRightPressed = GetSecondaryButtonRight();
        bool secondaryButtonRightDown = !m_SecondaryButtonRightPressedPrevFrame && secondaryButtonRightPressed;
        bool secondaryButtonRightUp = m_SecondaryButtonRightPressedPrevFrame && !secondaryButtonRightPressed;
        SendSecondaryButtonRightContinous(secondaryButtonRightPressed);
        if(secondaryButtonRightDown){
            SendSecondaryButtonRightDown();
        }else if(secondaryButtonRightUp){
            SendSecondaryButtonRightUp();
        }
        m_SecondaryButtonRightPressedPrevFrame = secondaryButtonRightPressed;
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
    void DisableTeleportLeftController(){
        
    }
    void DisableTeleportRightController(){
        
    }
    void EnableTeleportLeftController(){
        
    }
    public void EnableTeleportRightController(){

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
                m_LeftTriggerContinousRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_RightTriggerContinousRegistrants.Add(registrant);
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
                successfullyRemoved = m_LeftTriggerContinousRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_RightTriggerContinousRegistrants.Remove(registrant);
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
                m_PrimaryButtonLeftDownRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_PrimaryButtonRightDownRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterPrimaryButtonUp(IPrimaryButtonUp registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_PrimaryButtonLeftUpRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_PrimaryButtonRightUpRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterPrimaryButtonContinous(IPrimaryButtonContinous registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_PrimaryButtonLeftContinousRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_PrimaryButtonRightContinousRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterSecondaryButtonDown(ISecondaryButtonDown registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_SecondaryButtonLeftDownRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_SecondaryButtonRightDownRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterSecondaryButtonUp(ISecondaryButtonUp registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_SecondaryButtonLeftUpRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_SecondaryButtonRightUpRegistrants.Add(registrant);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void RegisterSecondaryButtonContinous(ISecondaryButtonContinous registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_SecondaryButtonLeftContinousRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_SecondaryButtonRightContinousRegistrants.Add(registrant);
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
                successfullyRemoved = m_PrimaryButtonLeftDownRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_PrimaryButtonRightDownRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterPrimaryButtonUp(IPrimaryButtonUp registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = m_PrimaryButtonLeftUpRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_PrimaryButtonRightUpRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterPrimaryButtonContinous(IPrimaryButtonContinous registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = m_PrimaryButtonLeftContinousRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_PrimaryButtonRightContinousRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterSecondaryButtonDown(ISecondaryButtonDown registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = m_SecondaryButtonLeftDownRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_SecondaryButtonRightDownRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterSecondaryButtonUp(ISecondaryButtonUp registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = m_SecondaryButtonLeftUpRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_SecondaryButtonRightUpRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void UnregisterSecondaryButtonContinous(ISecondaryButtonContinous registrant, ControllerHand controllerHand){
        bool successfullyRemoved;
        switch(controllerHand){
            case ControllerHand.Left:
                successfullyRemoved = m_SecondaryButtonLeftContinousRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_SecondaryButtonRightContinousRegistrants.Remove(registrant);
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
    void SendPrimaryButtonLeftDown(){
        foreach(IPrimaryButtonDown registrant in m_PrimaryButtonLeftDownRegistrants){
            registrant.ProcessPrimaryButtonDown();
        }
    }
    void SendPrimaryButtonLeftUp(){
        foreach(IPrimaryButtonUp registrant in m_PrimaryButtonLeftUpRegistrants){
            registrant.ProcessPrimaryButtonUp();
        }
    }
    void SendPrimaryButtonLeftContinous(bool value){
        foreach(IPrimaryButtonContinous registrant in m_PrimaryButtonLeftContinousRegistrants){
            registrant.ProcessPrimaryButtonContinous(value);
        }
    }
    void SendPrimaryButtonRightDown(){
        foreach(IPrimaryButtonDown registrant in m_PrimaryButtonRightDownRegistrants){
            registrant.ProcessPrimaryButtonDown();
        }
    }
    void SendPrimaryButtonRightUp(){
        foreach(IPrimaryButtonUp registrant in m_PrimaryButtonRightUpRegistrants){
            registrant.ProcessPrimaryButtonUp();
        }
    }
    void SendPrimaryButtonRightContinous(bool value){
        foreach(IPrimaryButtonContinous registrant in m_PrimaryButtonRightContinousRegistrants){
            registrant.ProcessPrimaryButtonContinous(value);
        }
    }
    void SendSecondaryButtonLeftDown(){
        foreach(ISecondaryButtonDown registrant in m_SecondaryButtonLeftDownRegistrants){
            registrant.ProcessSecondaryButtonDown();
        }
    }
    void SendSecondaryButtonLeftUp(){
        foreach(ISecondaryButtonUp registrant in m_SecondaryButtonLeftUpRegistrants){
            registrant.ProcessSecondaryButtonUp();
        }
    }
    void SendSecondaryButtonLeftContinous(bool value){
        foreach(ISecondaryButtonContinous registrant in m_SecondaryButtonLeftContinousRegistrants){
            registrant.ProcessSecondaryButtonContinous(value);
        }
    }
    void SendSecondaryButtonRightDown(){
        foreach(ISecondaryButtonDown registrant in m_SecondaryButtonRightDownRegistrants){
            registrant.ProcessSecondaryButtonDown();
        }
    }
    void SendSecondaryButtonRightUp(){
        foreach(ISecondaryButtonUp registrant in m_SecondaryButtonRightUpRegistrants){
            registrant.ProcessSecondaryButtonUp();
        }
    }
    void SendSecondaryButtonRightContinous(bool value){
        foreach(ISecondaryButtonContinous registrant in m_SecondaryButtonRightContinousRegistrants){
            registrant.ProcessSecondaryButtonContinous(value);
        }
    }
    private void SendLeftContinousTrigger(InputAction.CallbackContext obj){
        foreach(IContinousTrigger registrant in m_LeftTriggerContinousRegistrants){
            registrant.ProcessTriggerInput(obj.ReadValue<float>());
        }
    }
    private void SendRightContinousTriggerToRegistrants(InputAction.CallbackContext obj){
        foreach(IContinousTrigger registrant in m_RightTriggerContinousRegistrants){
            registrant.ProcessTriggerInput(obj.ReadValue<float>());
        }
    }
    private void SendLeftJoystickPressToRegistrants(InputAction.CallbackContext obj){
        float result = obj.ReadValue<float>();
        foreach(IJoystickPress registrant in joystickPressLeftRegistrants){
            registrant.ProcessJoystickPress(result);
        }
    }
    bool GetPrimaryButtonLeft(){ // TODO: move to own class
        bool primaryButtonLeftPressed;
        leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButtonLeftPressed);
        return primaryButtonLeftPressed;
    }
    bool GetPrimaryButtonRight(){ // TODO: move to own class
        bool primaryButtonRightPressed;
        rightXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out primaryButtonRightPressed);
        return primaryButtonRightPressed;
    }
    bool GetSecondaryButtonLeft(){
        bool secondaryButtonLeftPressed;
        leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButtonLeftPressed);
        return secondaryButtonLeftPressed;
    }
    bool GetSecondaryButtonRight(){
        bool secondaryButtonRightPressed;
        rightXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out secondaryButtonRightPressed);
        return secondaryButtonRightPressed;
    }
    #endregion Control Processing

    #region Scene Callbacks
    public void SetControllerStateToDrawing(){ // TODO: set these callbacks from code ?! - now they are called from the XR on the Pencil!!
        m_ControllerState = ControllerState.Drawing;
        switch(m_MainController){
            case ControllerHand.Left:
                SetControllerUI_State(ControllerHand.Right, false);
                break;  
            case ControllerHand.Right:
                SetControllerUI_State(ControllerHand.Left, false); // TODO: split into two methods.
                DisableTeleportRightController();
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    public void SetControllerStateToNotDrawing(){
        m_ControllerState = ControllerState.NotDrawing;
        switch(m_MainController){
            case ControllerHand.Left:
                SetControllerUI_State(ControllerHand.Right, true);
                break;  
            case ControllerHand.Right:
                SetControllerUI_State(ControllerHand.Left, true);
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
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