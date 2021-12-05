using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls
{

[RequireComponent(typeof(RawButtonInput))]
/// <summary>
/// Reads values frow raw XR inputs and sends these to registrered classes.
/// </summary>
public class ButtonsProcessor : MonoBehaviour
{
    RawButtonInput m_RawButtonInput;
    private List<IContinousTrigger> m_TriggerLeftContinousRegistrants;
    private List<IContinousTrigger> m_TriggerRightContinousRegistrants;
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

    private List<IJoystickPress> m_JoystickPressLeftRegistrants;
    private List<IJoystickPress> m_JoystickPressRightRegistrants;

    private void Awake() {
        InitMembers();
        InitButtonRegistrantLists();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleButtonInput();
    }

    private void InitMembers(){
        m_RawButtonInput = GetComponent<RawButtonInput>();
    }
    void InitButtonRegistrantLists(){
        m_TriggerLeftContinousRegistrants = new List<IContinousTrigger>();
        m_TriggerRightContinousRegistrants = new List<IContinousTrigger>();
        m_JoystickPressLeftRegistrants = new List<IJoystickPress>();
        m_JoystickPressRightRegistrants = new List<IJoystickPress>();
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

    void HandleButtonInput(){
        HandlePrimaryButtonLeft();
        HandlePrimaryButtonRight();
        HandleSecondaryButtonLeft();
        HandleSecondaryButtonRight();
        HandleJoystickPressLeft();
    }

    void HandleJoystickPressLeft(){
        // TODO:  implement
        // bool triggerValue;
        // if (leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxisClick, out triggerValue) && triggerValue)
        // {
        //     Debug.Log("Joy button is pressed");
        // }
    }

    #region Button Handlers
    void HandlePrimaryButtonLeft(){
        SendPrimaryButtonLeftContinous(m_RawButtonInput.PrimaryButtonLeft);
        if(m_RawButtonInput.PrimaryButtonLeftDown){
            Debug.Log("Primary btn left down");
            SendPrimaryButtonLeftDown();
        }else if(m_RawButtonInput.PrimaryButtonLeftUp){
            Debug.Log("Primary btn left up");
            SendPrimaryButtonLeftUp();
        }
    }
    void HandlePrimaryButtonRight(){
        SendPrimaryButtonRightContinous(m_RawButtonInput.PrimaryButtonRight);
        if(m_RawButtonInput.PrimaryButtonRightDown){
            SendPrimaryButtonRightDown();
            Debug.Log("Primary btn right down");
        }else if(m_RawButtonInput.PrimaryButtonRightUp){
            Debug.Log("Primary btn right up");
            SendPrimaryButtonRightUp();
        }
    }
    void HandleSecondaryButtonLeft(){
        SendSecondaryButtonLeftContinous(m_RawButtonInput.SecondaryButtonLeft);
        if(m_RawButtonInput.SecondaryButtonLeftDown){
            Debug.Log("Secondary btn left down");
            SendSecondaryButtonLeftDown();
        }else if(m_RawButtonInput.SecondaryButtonLeftUp){
            Debug.Log("Secondary btn left up");
            SendSecondaryButtonLeftUp();
        }
    }
    void HandleSecondaryButtonRight(){
        SendSecondaryButtonRightContinous(m_RawButtonInput.SecondaryButtonRight);
        if(m_RawButtonInput.SecondaryButtonRightDown){
            SendSecondaryButtonRightDown();
            Debug.Log("Secondary btn right down");
        }else if(m_RawButtonInput.SecondaryButtonRightUp){
            Debug.Log("Secondary btn right up");
            SendSecondaryButtonRightUp();
        }
    }

    #endregion Button Handlers


    #region Buttons Setup
    public void RegisterContinousTrigger(IContinousTrigger registrant, ControllerHand controllerHand){
        switch(controllerHand){
            case ControllerHand.Left:
                m_TriggerLeftContinousRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_TriggerRightContinousRegistrants.Add(registrant);
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
                successfullyRemoved = m_TriggerLeftContinousRegistrants.Remove(registrant);
                if(successfullyRemoved is false) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_TriggerRightContinousRegistrants.Remove(registrant);
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
                m_JoystickPressLeftRegistrants.Add(registrant);
                break;
            case ControllerHand.Right:
                m_JoystickPressRightRegistrants.Add(registrant);
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
                successfullyRemoved = m_JoystickPressLeftRegistrants.Remove(registrant);
                if(!successfullyRemoved) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.Right:
                successfullyRemoved = m_JoystickPressRightRegistrants.Remove(registrant);
                if(!successfullyRemoved) {M.NoItemRemoved();}
                else {M.ItemRemoved();}
                break;
            case ControllerHand.None:
                M.SpecifyControllerHand();
                break;
        }
    }
    #endregion Buttons Setup

    
    #region Buttons Processing
    private void SendPrimaryButtonLeftDown(){
        foreach(IPrimaryButtonDown registrant in m_PrimaryButtonLeftDownRegistrants){
            registrant.ProcessPrimaryButtonDown();
        }
    }
    private void SendPrimaryButtonLeftUp(){
        foreach(IPrimaryButtonUp registrant in m_PrimaryButtonLeftUpRegistrants){
            registrant.ProcessPrimaryButtonUp();
        }
    }
    private void SendPrimaryButtonLeftContinous(bool value){
        foreach(IPrimaryButtonContinous registrant in m_PrimaryButtonLeftContinousRegistrants){
            registrant.ProcessPrimaryButtonContinous(value);
        }
    }
    private void SendPrimaryButtonRightDown(){
        foreach(IPrimaryButtonDown registrant in m_PrimaryButtonRightDownRegistrants){
            registrant.ProcessPrimaryButtonDown();
        }
    }
    private void SendPrimaryButtonRightUp(){
        foreach(IPrimaryButtonUp registrant in m_PrimaryButtonRightUpRegistrants){
            registrant.ProcessPrimaryButtonUp();
        }
    }
    private void SendPrimaryButtonRightContinous(bool value){
        foreach(IPrimaryButtonContinous registrant in m_PrimaryButtonRightContinousRegistrants){
            registrant.ProcessPrimaryButtonContinous(value);
        }
    }
    private void SendSecondaryButtonLeftDown(){
        foreach(ISecondaryButtonDown registrant in m_SecondaryButtonLeftDownRegistrants){
            registrant.ProcessSecondaryButtonDown();
        }
    }
    private void SendSecondaryButtonLeftUp(){
        foreach(ISecondaryButtonUp registrant in m_SecondaryButtonLeftUpRegistrants){
            registrant.ProcessSecondaryButtonUp();
        }
    }
    private void SendSecondaryButtonLeftContinous(bool value){
        foreach(ISecondaryButtonContinous registrant in m_SecondaryButtonLeftContinousRegistrants){
            registrant.ProcessSecondaryButtonContinous(value);
        }
    }
    private void SendSecondaryButtonRightDown(){
        foreach(ISecondaryButtonDown registrant in m_SecondaryButtonRightDownRegistrants){
            registrant.ProcessSecondaryButtonDown();
        }
    }
    private void SendSecondaryButtonRightUp(){
        foreach(ISecondaryButtonUp registrant in m_SecondaryButtonRightUpRegistrants){
            registrant.ProcessSecondaryButtonUp();
        }
    }
    private void SendSecondaryButtonRightContinous(bool value){
        foreach(ISecondaryButtonContinous registrant in m_SecondaryButtonRightContinousRegistrants){
            registrant.ProcessSecondaryButtonContinous(value);
        }
    }

    // TODO: get from raw input instead... 
    private void SendTriggerLeftContinous(float value){
        foreach(IContinousTrigger registrant in m_TriggerLeftContinousRegistrants){
            registrant.ProcessTriggerInput(value);
        }
    }
    private void SendTriggerRightContinous(float value){
        foreach(IContinousTrigger registrant in m_TriggerRightContinousRegistrants){
            registrant.ProcessTriggerInput(value);
        }
    }
    private void SendJoystickPressLeftContinousToRegistrants(bool value){
        foreach(IJoystickPress registrant in m_JoystickPressLeftRegistrants){
            registrant.ProcessJoystickPress(value);
        }
    }
    #endregion Buttons Processing
}
 
}