using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls
{
/// <summary>
/// Directly accessed XR devices - Left and Right hand Controllers. 
/// <br/>IN EDITOR : TOUCH CONTROLLERS MUST BE ACTIVE BEFORE STARTING PLAYMODE!!!
/// <br/>Otherwise they will never be initialized!
/// </summary>
public class RawButtonInput : MonoBehaviour
{
    UnityEngine.XR.InputDevice leftXRController;
    UnityEngine.XR.InputDevice rightXRController;
    bool m_PrimaryButtonLeftCached = false;
    bool m_PrimaryButtonRightCached = false;
    bool m_SecondaryButtonLeftCached = false;
    bool m_SecondaryButtonRightCached = false;
    bool m_PrimaryButtonLeft = false;
    public bool PrimaryButtonLeft { get => m_PrimaryButtonLeft; }
    bool m_PrimaryButtonRight = false;
    public bool PrimaryButtonRight { get => m_PrimaryButtonRight; }
    bool m_SecondaryButtonLeft = false;
    public bool SecondaryButtonLeft { get => m_SecondaryButtonLeft; }
    bool m_SecondaryButtonRight = false;
    public bool SecondaryButtonRight { get => m_SecondaryButtonRight; }
    bool m_PrimaryButtonLeftDown = false;
    public bool PrimaryButtonLeftDown { get => m_PrimaryButtonLeftDown; }
    bool m_PrimaryButtonRightDown = false;
    public bool PrimaryButtonRightDown { get => m_PrimaryButtonRightDown; }
    bool m_SecondaryButtonLeftDown = false;
    public bool SecondaryButtonLeftDown { get => m_SecondaryButtonLeftDown; }
    bool m_SecondaryButtonRightDown = false;
    public bool SecondaryButtonRightDown { get => m_SecondaryButtonRightDown; }
    bool m_PrimaryButtonLeftUp = false;
    public bool PrimaryButtonLeftUp { get => m_PrimaryButtonLeftUp; }
    bool m_PrimaryButtonRightUp = false;
    public bool PrimaryButtonRightUp { get => m_PrimaryButtonRightUp; }
    bool m_SecondaryButtonLeftUp = false;
    public bool SecondaryButtonLeftUp { get => m_SecondaryButtonLeftUp; }
    bool m_SecondaryButtonRightUp = false;
    public bool SecondaryButtonRightUp { get => m_SecondaryButtonRightUp; }

    private void Start() {
        InitializeLeftHandController();
        InitializeRightHandController();    
    }
    private void Update() {
        ProcessButtons();
    }
    void ProcessButtons(){
        ProcessPrimaryButtonLeft();
        ProcessPrimaryButtonRight();
        ProcessSecondaryButtonLeft();
        ProcessSecondaryButtonRight();
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
    void ProcessPrimaryButtonLeft(){
        leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out m_PrimaryButtonLeft);
        m_PrimaryButtonLeftDown = !m_PrimaryButtonLeftCached && m_PrimaryButtonLeft;
        m_PrimaryButtonLeftUp = m_PrimaryButtonLeftCached && !m_PrimaryButtonLeft;        
        m_PrimaryButtonLeftCached = m_PrimaryButtonLeft;
    }
    void ProcessPrimaryButtonRight(){
        rightXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out m_PrimaryButtonRight);
        m_PrimaryButtonRightDown = !m_PrimaryButtonRightCached && m_PrimaryButtonRight;
        m_PrimaryButtonRightUp = m_PrimaryButtonRightCached && !m_PrimaryButtonRight;        
        m_PrimaryButtonRightCached = m_PrimaryButtonRight;
    }
    void ProcessSecondaryButtonLeft(){
        leftXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out m_SecondaryButtonLeft);
        m_SecondaryButtonLeftDown = !m_SecondaryButtonLeftCached && m_SecondaryButtonLeft;
        m_SecondaryButtonLeftUp = m_SecondaryButtonLeftCached && !m_SecondaryButtonLeft;        
        m_SecondaryButtonLeftCached = m_SecondaryButtonLeft;
    }
    void ProcessSecondaryButtonRight(){
        rightXRController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out m_SecondaryButtonRight);
        m_SecondaryButtonRightDown = !m_SecondaryButtonRightCached && m_SecondaryButtonRight;
        m_SecondaryButtonRightUp = m_SecondaryButtonRightCached && !m_SecondaryButtonRight;        
        m_SecondaryButtonRightCached = m_SecondaryButtonRight;
    }

    // TODO: get trigger continous and joystick press
}


}