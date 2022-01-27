using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{


public class XR_ComponentsController : MonoBehaviour
{
    [SerializeField] private GameObject rightUI_Controller;
    [SerializeField] private GameObject leftUI_Controller;
    [SerializeField] private GameObject rightTeleportController;
    [SerializeField] private GameObject leftTeleportController;
    [SerializeField] private UI_InteractionController leftUI_InteractionController;
    [SerializeField] private UI_InteractionController rightUI_InteractionController;
    
    // all XR custom components
    [SerializeField] private CustomMoveProvider customMoveProvider;
    [SerializeField] private CustomSnapTurnProvider customSnapTurnProvider;

#region Unity Methods
    private void Awake() {
        InitializeMovementProviders();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
#endregion Unity Methods

    void InitializeMovementProviders(){
        
    }


    // 
    void LeftController_DisableUI(){

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




}

}