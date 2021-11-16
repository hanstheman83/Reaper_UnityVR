using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{
/// <summary>
/// Should register the opposite primary and secondary buttons of hand holding pencil.
/// </summary>
public class DrawingInputController : MonoBehaviour, IPrimaryButtonDown
{
    private ControllerHand m_ControlledBy = ControllerHand.None;
    public ControllerHand ControlledBy { get => m_ControlledBy; }


    // TODO: take input, button interface(s)
    // TODO: calls drawing on texture

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessButtonDown()
    {
        Debug.Log($"Primary button down on {m_ControlledBy}");
    }
}

}