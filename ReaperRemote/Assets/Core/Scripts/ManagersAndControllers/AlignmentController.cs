using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using Core;

public class AlignmentController : MonoBehaviour, IJoystickPress
{
    private ControllerHand controlledBy;
    public ControllerHand ControlledBy {get => controlledBy;}
    private InputActionController inputActionController;
    private ReferenceBoardController leftReferenceBoardController;

    public void ProcessJoystickPress(float val)
    {
        Debug.Log("ACTION".Colorize(Color.grey));
        // TODO: what if is held by other hand ??
        transform.rotation = leftReferenceBoardController.referenceBoard.rotation;
        transform.position = leftReferenceBoardController.referenceBoard.position;
    }

    // v2 
    // auto alignment : quest 2 only
    // from 3 transforms determine rotation
    // manually adjust height

    // try get name of xr device 
    // InputDevice.name

    private void Awake() {
        controlledBy = ControllerHand.Left; // TODO: set in UI
        // then update
    }

    // Start is called before the first frame update
    void Start()
    {
        leftReferenceBoardController = GameObject.FindGameObjectWithTag("Left Hand").GetComponent<ReferenceBoardController>();
        inputActionController = FindObjectOfType<InputActionController>();
        // TODO: dynamic setup in UI
        inputActionController.RegisterJoystickPress(this, controlledBy);
    }

    
}
