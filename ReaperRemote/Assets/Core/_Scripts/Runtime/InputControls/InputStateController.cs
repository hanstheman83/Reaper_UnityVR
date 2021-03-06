using UnityEngine;

namespace Core.Controls
{
/// <summary>
/// Helper Class for InputActionController. <br/>
/// Handles Scene and UI callbacks. <br/>
/// Since ActionBasedControllerManager turns off teleport and snap turn while holding object that is not needed! 
/// </summary>
[RequireComponent(typeof(ButtonsProcessor))]
[RequireComponent(typeof(XR_ComponentsController))]
public class InputStateController : MonoBehaviour, IMovementControlStates
{
    private ButtonsProcessor m_ButtonsProcessor;
    private XR_ComponentsController m_XR_ComponentsController;
    private ControllerHand m_MainController = ControllerHand.Right;
    private enum ControllerState { Drawing, NotDrawing }
    private ControllerState m_ControllerState = ControllerState.NotDrawing;
    // dictionary of controller states - linking to a controllerState object

    private void Awake() {
        m_ButtonsProcessor = GetComponent<ButtonsProcessor>();
        m_XR_ComponentsController = GetComponent<XR_ComponentsController>();
    }

    public void DeactivateLeftControllerButtons()
    {
        throw new System.NotImplementedException();
    }
    public void DeactivateRightControllerButtons()
    {
        throw new System.NotImplementedException();
    }
    // TODO: add interfaces
    //#region Scene Callbacks
// public void SetControllerStateToDrawing(){ // TODO: set these callbacks from code ?! - now they are called from the XR component on the Pencil!!
//     m_ControllerState = ControllerState.Drawing;
//     switch(m_MainController){
//         case ControllerHand.Left:
//             SetControllerUI_State(ControllerHand.Right, false);
//             break;  
//         case ControllerHand.Right:
//             SetControllerUI_State(ControllerHand.Left, false); // TODO: split into two methods.
//             DisableTeleportRightController();
//             break;
//         case ControllerHand.None:
//             M.SpecifyControllerHand();
//             break;
//     }
// }
// public void SetControllerStateToNotDrawing(){
//     m_ControllerState = ControllerState.NotDrawing;
//     switch(m_MainController){
//         case ControllerHand.Left:
//             SetControllerUI_State(ControllerHand.Right, true);
//             break;  
//         case ControllerHand.Right:
//             SetControllerUI_State(ControllerHand.Left, true);
//             break;
//         case ControllerHand.None:
//             M.SpecifyControllerHand();
//             break;
//     }
// }
// #endregion Scene Callbacks

// #region UI callbacks
// // change control config and store in playerprefs
// public void StopMovement(){
//     customMoveProvider.DeactivateControl(ControllerHand.Left);
// }

// #endregion UI callbacks

// #region Set Controller Roles
//     void SetMainControllerToRightController(){
//         m_MainController = ControllerHand.Right;
//         Debug.Log("right controller is main controller"); // this is default
//         rightTeleportController.SetActive(true);
//         rightActionBasedControllerManager.enabled = true;
//         leftActionBasedControllerManager.enabled = false;
//         SetControllerUI_State(ControllerHand.Left, true);
//         SetControllerUI_State(ControllerHand.Right, false);
//         leftTeleportController.SetActive(false);
//         customMoveProvider.ActivateControl(leftMove, ControllerHand.Left);
//         customSnapTurnProvider.ActivateControl(rightTurn, ControllerHand.Right);
//         customMoveProvider.DeactivateControl(ControllerHand.Right);
//         customSnapTurnProvider.DeactivateControl(ControllerHand.Left);
//     }
//     void SetMainControllerToLeftController(){
//         m_MainController = ControllerHand.Left;
//         Debug.Log("left controller active");
//         leftTeleportController.SetActive(true);
//         leftActionBasedControllerManager.enabled = true; // TODO add this to right controller if script starts disabled...
//         rightActionBasedControllerManager.enabled = false;
//         SetControllerUI_State(ControllerHand.Left, false);
//         SetControllerUI_State(ControllerHand.Right, true);
//         rightTeleportController.SetActive(false);
//         customMoveProvider.ActivateControl(rightMove, ControllerHand.Right);
//         customSnapTurnProvider.ActivateControl(leftTurn, ControllerHand.Left);
//         customMoveProvider.DeactivateControl(ControllerHand.Left);
//         customSnapTurnProvider.DeactivateControl(ControllerHand.Right);
//     }
// #endregion Set Controller Roles

}



}