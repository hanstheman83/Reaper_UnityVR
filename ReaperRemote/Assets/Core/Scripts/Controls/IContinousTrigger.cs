using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Core.Controls{

public interface IContinousTrigger
{
    public string NameOfTriggerController {get;}
    public DataHandler TriggerDataFlow {get;}

    /// <summary>
    /// Data for Trigger : 0 to 1, use Data.Process(val, dataHandler) to return processed data (custom data type triggerData ??)
    /// </summary>
    public void RegisterTriggerControl(InputActionController inputActionController, ControllerHand hand, DataHandler dataFlow);
    
}

}