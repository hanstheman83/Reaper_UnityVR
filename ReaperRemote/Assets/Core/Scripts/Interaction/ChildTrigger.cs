using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Interactions{

public class ChildTrigger : MonoBehaviour
{
    public delegate void childTriggeredEnter(Collider c);
    public delegate void childTriggeredExit(Collider c);
    public event childTriggeredEnter childTriggeredEnterEvent;
    public event childTriggeredExit childTriggeredExitEvent;
 

    void OnTriggerEnter(Collider c){
        childTriggeredEnterEvent(c);
    }

    void OnTriggerExit(Collider c){
        childTriggeredExitEvent(c);
    }
 
}


}