using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System;


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

    // InputActionProperties for XR components
    [SerializeField] private InputActionProperty leftMove;
    [SerializeField] private InputActionProperty rightMove;
    [SerializeField] private InputActionProperty leftTurn;
    [SerializeField] private InputActionProperty rightTurn;
    [SerializeField] private InputActionProperty leftTrigger;
    [SerializeField] private InputActionProperty rightTrigger;

    // InputActionReferences for own Input manipulations
    // https://docs.unity3d.com/Manual/xr_input.html
    [SerializeField] private InputActionReference XR_leftTriggerPress;
    [SerializeField] private InputActionReference XR_rightTriggerPress;

    private List<IContinousTrigger> continousLeftTriggers;
    private List<IContinousTrigger> continousRightTriggers;
 
    private void Awake() {
        continousLeftTriggers = new List<IContinousTrigger>();
        continousRightTriggers = new List<IContinousTrigger>();

        customMoveProvider = FindObjectOfType<CustomMoveProvider>();
        customSnapTurnProvider = FindObjectOfType<CustomSnapTurnProvider>();
        XR_leftTriggerPress.action.performed += ProcessLeftTrigger;
        XR_rightTriggerPress.action.performed += ProcessRightTrigger;
        
        // All prefabs implementing interface! - so can assign different controls per gameobject!
        // problem : filtering in UI, can filter by type.
        // continousTriggers = new List<IContinousTrigger>(); // list will have different component types implementing the interface
        // var ss = FindObjectsOfType<MonoBehaviour>().OfType<IContinousTrigger>();
        // foreach (IContinousTrigger t in ss) {
        //     continousTriggers.Add (t);
        // }
        // Debug.Log($"Number of IContinousTrigger { ss.Count()} "); 
    }

    private void Start() {
        // continousTriggers[0].RegisterTriggerControl(this, ControllerHand.Left, DataHandler.Reversed);
    // continousTriggers[0].RegisterTriggerControl(this, ControllerHand.Right, DataHandler.Reversed);
    // continousTriggers[1].RegisterTriggerControl(this, ControllerHand.Left, DataHandler.Reversed);
    // continousTriggers[1].RegisterTriggerControl(this, ControllerHand.Right, DataHandler.Reversed);
    }

    public void RegisterTriggerControl(IContinousTrigger trigger, ControllerHand c){
        switch(c){
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
    public void RemoveTriggerControl(IContinousTrigger trigger, ControllerHand c){
        bool success;
        switch(c){
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

    private void ProcessLeftTrigger(InputAction.CallbackContext obj){
        // leftTriggerPressed(obj.ReadValue<float>(), ControllerHand.Left);
        //leftTriggerPressed?.Invoke(obj.ReadValue<float>(), ControllerHand.Left);
        foreach(IContinousTrigger t in continousLeftTriggers){
            t.ProcessInput(obj.ReadValue<float>());
        }
    }
    private void ProcessRightTrigger(InputAction.CallbackContext obj){
        // rightTriggerPressed(obj.ReadValue<float>(), ControllerHand.Right);
        //rightTriggerPressed?.Invoke(obj.ReadValue<float>(), ControllerHand.Right);
        foreach(IContinousTrigger t in continousRightTriggers){
            t.ProcessInput(obj.ReadValue<float>());
        }
    }

    void Test(InputAction.CallbackContext obj){
        Debug.Log("Test!".Colorize(Color.black));
        Debug.Log("action type :" + obj.valueType);
        // Debug.Log("action type :" + obj.v);
        var v = obj.ReadValue<float>();
        Debug.Log($"value {v}".Colorize(Color.green));
    }

    void Update() {
        // toggle
        if(oldState != rightIsActive){
            if(rightIsActive) {
                Debug.Log("right controller active"); // this is default
                // right controller is main controller :
                // 
                rightUI_Controller.SetActive(false);
                rightTeleportController.SetActive(true);
                rightActionBasedControllerManager.enabled = true;
                rightUI_InteractionController.enabled = false;

                leftActionBasedControllerManager.enabled = false;
                leftUI_InteractionController.enabled = true;
                leftUI_Controller.SetActive(true);
                leftTeleportController.SetActive(false);

                //customMoveProvider.ActivateComponent();
                customMoveProvider.ActivateControl(leftMove, ControllerHand.Left);
                //customSnapTurnProvider.ActivateComponent();
                customSnapTurnProvider.ActivateControl(rightTurn, ControllerHand.Right);

                customMoveProvider.DeactivateControl(ControllerHand.Right);
                //customMoveProvider.DeactivateComponent();
                customSnapTurnProvider.DeactivateControl(ControllerHand.Left);
                //customSnapTurnProvider.DeactivateComponent();
                
                }
            else {
                // leftHandMoveAction = controlScheme_01.leftHandMoveAction;
                Debug.Log("left controller active");

                leftUI_Controller.SetActive(false);
                leftTeleportController.SetActive(true);
                
                leftActionBasedControllerManager.enabled = true; // TODO add this to right controller if script starts disabled...
                leftUI_InteractionController.enabled = false;
                
                rightActionBasedControllerManager.enabled = false;
                rightUI_InteractionController.enabled = true;
                rightUI_Controller.SetActive(true);
                rightTeleportController.SetActive(false);
                
                //customMoveProvider.ActivateComponent();
                customMoveProvider.ActivateControl(rightMove, ControllerHand.Right);
                //customSnapTurnProvider.ActivateComponent();
                customSnapTurnProvider.ActivateControl(leftTurn, ControllerHand.Left);
                
                customMoveProvider.DeactivateControl(ControllerHand.Left);
                //customMoveProvider.DeactivateComponent();
                customSnapTurnProvider.DeactivateControl(ControllerHand.Right);
                //customSnapTurnProvider.DeactivateComponent();

            }
        }
        oldState = rightIsActive;
    }

    private void OnDestroy() {
        //RemoveListenersRightTrigger();
        // rightTriggerPressed = null;
        // leftTriggerPressed = null;
    }
    // private void RemoveListenersRightTrigger(){
    //     if(rightTriggerPressed == null) return;
    //     foreach(Delegate d in rightTriggerPressed.GetInvocationList())
    //     {
    //         rightTriggerPressed -= (RightTriggerPressed)d;
    //         Debug.Log("Removing Delegates right trigger".Colorize(Color.red));
    //     }
    // }
    

    #region UI callbacks
    // change control config and store in playerprefs
    public void StopMovement(){
        customMoveProvider.DeactivateControl(ControllerHand.Left);
    }




    #endregion UI callbacks
}

}