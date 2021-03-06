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
    public delegate void MainControllerHandChanged(ControllerHand controllerHand);
    public static event MainControllerHandChanged mainControllerHandChanged;

    void Awake(){
        InitializeAsSingleton();
    }
    void InitializeAsSingleton(){
        bool instanceExists = m_SceneManager != null && m_SceneManager != this;
        if (instanceExists){ 
            Debug.LogError("SceneManager already exists - only 1 per scene!");
            Destroy(this.gameObject);
        }else{
            m_SceneManager = this;
        }
    }

    public void ChangeHandHoldingPencil(ControllerHand controllerHand){
        SceneStates.PencilHand = controllerHand;
        handHoldingPencilChanged?.Invoke(controllerHand);
        // log
        // change state -- 

    }
    public void ChangeMainControllerHand(ControllerHand controllerHand){
        SceneStates.MainControllerHand = controllerHand;
        mainControllerHandChanged?.Invoke(controllerHand);
    }
}

}