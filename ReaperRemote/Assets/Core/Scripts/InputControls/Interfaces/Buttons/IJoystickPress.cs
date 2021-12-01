using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{

public interface IJoystickPress
{
    public ControllerHand ControlledBy {get;}

    public void ProcessJoystickPress(float val);
}

}