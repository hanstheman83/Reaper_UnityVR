using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{
public interface IPrimaryButtonDown
{
    public ControllerHand ControlledBy { get; }
    public void ProcessPrimaryButtonDown();
}


}