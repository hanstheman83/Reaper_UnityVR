using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Drawing;
using Core.SceneManagement;
using UnityEngine.Events;
namespace Core.Controls{

/// <summary>
/// Called from Drawing Stick - setting right controller btns
/// </summary>
public class UndoRedoButtons : MonoBehaviour, IPrimaryButtonDown, ISecondaryButtonDown
{
    [SerializeField]Undo undo;
    SceneManager sceneManager;
    private ButtonsProcessor m_ButtonsProcessor;
    private ControllerHand m_ControlledBy = ControllerHand.None; 
    public ControllerHand ControlledBy { get => m_ControlledBy; } // This script controlled by Opposite hand of the hand holding the pencil/drawing stick!!
    public UnityEvent UndoEvent;
    public void ProcessPrimaryButtonDown()
    {
        Debug.Log("Primary down on " + m_ControlledBy);
        // Undo
        UndoEvent?.Invoke();
        // drawing will store undos and redos
        // call drawing - change back to earlier state 
    }

    public void ProcessSecondaryButtonDown()
    {
        Debug.Log("Secondary down on " + m_ControlledBy);
        // Redo
    }

    // Start is called before the first frame update
    void Start()
    {
        m_ButtonsProcessor = FindObjectOfType<ButtonsProcessor>();
        sceneManager = SceneManager.Instance;
        sceneManager.handHoldingPencilChanged += SetupButtons;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetupButtons(ControllerHand controllerHand){ // controllerHand is controller hand holding pencil!
        // controllerHand is pencil hand - undo redo should be set to opposite!
        switch(controllerHand){
            case ControllerHand.None:
                if(m_ControlledBy == ControllerHand.None){ // might never be called..
                    // do nothing
                }else if(m_ControlledBy == ControllerHand.Left){
                    m_ButtonsProcessor.UnregisterPrimaryButtonDown(this, ControllerHand.Left);
                    m_ButtonsProcessor.UnregisterSecondaryButtonDown(this, ControllerHand.Left);
                }else if(m_ControlledBy == ControllerHand.Right){
                    m_ButtonsProcessor.UnregisterPrimaryButtonDown(this, ControllerHand.Right);
                    m_ButtonsProcessor.UnregisterSecondaryButtonDown(this, ControllerHand.Right);
                }
                m_ControlledBy = ControllerHand.None;
                break;
            case ControllerHand.Left:
                m_ButtonsProcessor.RegisterPrimaryButtonDown(this, ControllerHand.Right);
                m_ButtonsProcessor.RegisterSecondaryButtonDown(this, ControllerHand.Right);
                m_ControlledBy = ControllerHand.Right;
                break;
            case ControllerHand.Right:
                m_ButtonsProcessor.RegisterPrimaryButtonDown(this, ControllerHand.Left);
                m_ButtonsProcessor.RegisterSecondaryButtonDown(this, ControllerHand.Left);
                m_ControlledBy = ControllerHand.Left;
                break;

        }
    }

    private void OnEnable() {
        
    }
    private void OnDisable() {
        sceneManager.handHoldingPencilChanged -= SetupButtons;
    }

}

}