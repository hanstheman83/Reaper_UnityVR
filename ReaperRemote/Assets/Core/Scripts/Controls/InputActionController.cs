using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Core.Controls{

/// <summary>
/// To setup custom controls in-game. Store in playerprefs
/// </summary>
public class InputActionController : MonoBehaviour
{
    // all XR custom components
    private CustomMoveProvider customMoveProvider;

    // dictionary of xr control to action
    // problem : some controls use multiple controls ?


    // set of inputactions stored in scriptable objects 
    // perhaps use these as a compilation/set of controls

    // control components should start disactivated
    
    private void Awake() {
        // search for components ? or hard-link
        customMoveProvider = FindObjectOfType<CustomMoveProvider>();
    }


    void Start()
    {
        // read playerprefs - overriding SOs
        // link actions from SOs
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    #region UI callbacks
    // change control config and store in playerprefs
    public void StopMovement(){
        customMoveProvider.DeactivateMovement();
    }




    #endregion UI callbacks
}

}