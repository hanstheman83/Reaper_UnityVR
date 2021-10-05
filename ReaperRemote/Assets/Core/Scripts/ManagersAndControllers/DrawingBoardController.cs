using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Core.Controls;

[RequireComponent(typeof(XRGrabInteractable))]
public class DrawingBoardController : MonoBehaviour
{
    ControllerHand controlledBy = ControllerHand.None;

    public void OnSelectEntered(SelectEnterEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        controlledBy = customDirectInteractor.ControllerHand;
        customDirectInteractor.attachTransform.position = GetComponent<XRGrabInteractable>().attachTransform.position;
        customDirectInteractor.attachTransform.rotation = GetComponent<XRGrabInteractable>().attachTransform.rotation;
    }

    public void OnSelectExited(SelectExitEventArgs args){
        CustomDirectInteractor customDirectInteractor = (CustomDirectInteractor)args.interactor;
        customDirectInteractor.attachTransform.localPosition = Vector3.zero;
        controlledBy = ControllerHand.None;
    }
}
