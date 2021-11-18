using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using System;

namespace Core.SceneManagement
{
/// <summary>
/// Controls Global Scene States. <br/> Singleton Pattern. 
/// </summary>
public class SceneManager : MonoBehaviour
{
    public static SceneManager instance 
                    { get 
                        {   if(m_SceneManager is null){
                                m_SceneManager = new SceneManager();
                            } 
                            return m_SceneManager;
                        }
                    }
    static SceneManager m_SceneManager; 
    public delegate void HandHoldingPencilChanged(ControllerHand controllerHand);
    public static event HandHoldingPencilChanged handHoldingPencilChanged;

    public void ChangeHandHoldingPencil(ControllerHand controllerHand){
        SceneStates.PencilHand = controllerHand;
        handHoldingPencilChanged?.Invoke(controllerHand);
    }
}

}