using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using System;

namespace Core.SceneManagement
{
/// <summary>
/// Controls Global Scene States. <br/>
/// </summary>
public class SceneManager : MonoBehaviour
{
    // TODO: maybe replace with this : https://erdiizgi.com/avoiding-singletons-in-unity/
    private static SceneManager m_SceneManager;
    public static SceneManager Instance {
        get { return m_SceneManager; }
    }
    public delegate void HandHoldingPencilChanged(ControllerHand controllerHand);
    public static event HandHoldingPencilChanged handHoldingPencilChanged;

    void Awake(){
        if (m_SceneManager != null && m_SceneManager != this) 
        { 
            Destroy(this.gameObject);
            return;
        }
        m_SceneManager = this;
    }

    public void ChangeHandHoldingPencil(ControllerHand controllerHand){
        SceneStates.PencilHand = controllerHand;
        handHoldingPencilChanged?.Invoke(controllerHand);
    }
}

}