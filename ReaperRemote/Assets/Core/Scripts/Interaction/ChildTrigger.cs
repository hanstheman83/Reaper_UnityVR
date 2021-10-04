using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildTrigger : MonoBehaviour
{
    public delegate void childTriggeredEnter(Collider c);
    public delegate void childTriggeredExit(Collider c);
    public event childTriggeredEnter childTriggeredEnterEvent;
    public event childTriggeredExit childTriggeredExitEvent;
 

    void OnTriggerEnter(Collider c){
        childTriggeredEnterEvent?.Invoke(c);
    }

    void OnTriggerExit(Collider c){
        childTriggeredExitEvent?.Invoke(c);
    }

}