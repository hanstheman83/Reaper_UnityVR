using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Drawing;

namespace Core.Controls{
/// <summary>
/// Should register the opposite primary and secondary buttons of hand holding pencil.
/// </summary>
public class Drawing_ButtonInputController : MonoBehaviour, IPrimaryButtonDown, ISecondaryButtonDown
{
    [SerializeField] DrawingOnTexture_GPU m_DrawingOnTexture;
    private ControllerHand m_ControlledBy = ControllerHand.None;
    public ControllerHand ControlledBy { get => m_ControlledBy; }


    // TODO: take input, button interface(s)
    // TODO: calls drawing on texture, undo redo
    // TODO: Deactivate when let go of pencil - only active when pencil is in hand.

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessPrimaryButtonDown()
    {
        Debug.Log($"Primary button down on {m_ControlledBy}");
    }

    public void ProcessSecondaryButtonDown()
    {
        Debug.Log($"Secondary button down on {m_ControlledBy}");
    }
}

}