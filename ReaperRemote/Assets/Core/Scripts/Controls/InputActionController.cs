using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



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

    [SerializeField]private InputActionProperty leftMove;
    [SerializeField]private InputActionProperty rightMove;
    [SerializeField]private InputActionProperty leftTurn;
    [SerializeField]private InputActionProperty rightTurn;


    // set of inputactions stored in scriptable objects 
    // perhaps use these as a compilation/set of controls

    // control components should start disactivated
    
    private void Awake() {
        // search for components ? or hard-link
        customMoveProvider = FindObjectOfType<CustomMoveProvider>();
        customSnapTurnProvider = FindObjectOfType<CustomSnapTurnProvider>();
    }

    private void Start() {
        // init
        
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







    #region UI callbacks
    // change control config and store in playerprefs
    public void StopMovement(){
        customMoveProvider.DeactivateControl(ControllerHand.Left);
    }




    #endregion UI callbacks
}

}