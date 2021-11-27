using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{

public interface ISecondaryButtonUp
{
    public ControllerHand ControlledBy { get; } 
    public void ProcessSecondaryButtonUp();  
}


}