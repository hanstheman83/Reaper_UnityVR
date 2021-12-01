using UnityEngine;

namespace Core.Controls
{
    /// <summary>
    /// Helper Class for InputActionController. <br/>
    /// Handles Scene and UI callbacks.
    /// </summary>
    [RequireComponent(typeof(InputActionController))]
    public class InputStateController : MonoBehaviour, IMovementControlStates
    {
        InputActionController m_InputActionController;

        private void Awake() {
            m_InputActionController = GetComponent<InputActionController>();
        }
        public void DeactivateLeftHandControls()
        {
            throw new System.NotImplementedException();
        }
        public void DeactivateRightHandControls()
        {
            throw new System.NotImplementedException();
        }

    }



}