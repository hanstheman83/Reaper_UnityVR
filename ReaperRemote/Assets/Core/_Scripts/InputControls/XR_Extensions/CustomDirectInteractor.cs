using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Core.Controls{

public class CustomDirectInteractor : XRDirectInteractor
{
    [SerializeField] private ControllerHand controllerHand;
    public ControllerHand ControllerHand {get => controllerHand;}
}

}