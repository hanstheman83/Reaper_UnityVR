using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

[CreateAssetMenu(fileName = "Control Scheme", menuName = "Controls/Control scheme 1")]
public class ControlScheme_01 : ScriptableObject
{
    // all base XR actions
    [SerializeField]
        [Tooltip("The Input System Action that will be used to read Move data from the left hand controller. Must be a Value Vector2 Control.")]
        public InputActionProperty leftHandMoveAction;
        
        


        
}
