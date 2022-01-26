using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;
using UnityEngine.XR.Interaction.Toolkit;

namespace Core.UI{

public class SetSwatches_UI : MonoBehaviour
{
    private CustomDirectInteractor m_LeftHandDirectInteractor;
    private CustomDirectInteractor m_RightHandDirectInteractor;
    [SerializeField] ColorSwatches_UI m_ColorSwatches_UI;

    private void Start() {
        CustomDirectInteractor[] interactors = GameObject.FindObjectsOfType<CustomDirectInteractor>(); //TODO: better architecture - static class with direct references??
        foreach (var interactor in interactors)
        {
            if(interactor.ControllerHand == ControllerHand.Left){
                m_LeftHandDirectInteractor = interactor;
            }else if(interactor.ControllerHand == ControllerHand.Right){
                m_RightHandDirectInteractor = interactor;
            }
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        if(customDirectInteractor.ControllerHand == ControllerHand.Left){
            m_ColorSwatches_UI.gameObject.SetActive(true);
            m_ColorSwatches_UI.transform.SetParent(m_RightHandDirectInteractor.transform);
            m_ColorSwatches_UI.transform.localPosition = Vector3.zero;
            m_ColorSwatches_UI.transform.localRotation = Quaternion.identity;
        }else if(customDirectInteractor.ControllerHand == ControllerHand.Right){
            m_ColorSwatches_UI.gameObject.SetActive(true);
            m_ColorSwatches_UI.transform.SetParent(m_LeftHandDirectInteractor.transform);
            m_ColorSwatches_UI.transform.localPosition = Vector3.zero;
            m_ColorSwatches_UI.transform.localRotation = Quaternion.identity;
        }
    }

    public void OnSelectExited(SelectExitEventArgs args){
        m_ColorSwatches_UI.gameObject.SetActive(false);
    }
}

}