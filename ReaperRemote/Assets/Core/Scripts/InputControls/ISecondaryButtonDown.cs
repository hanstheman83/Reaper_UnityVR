using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{

public interface ISecondaryButtonDown
{
    public ControllerHand ControlledBy { get; }

    public void ProcessSecondaryButtonDown();
}

}