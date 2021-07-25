using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildTrigger : MonoBehaviour
{
    
 
    void OnTriggerEnter(Collider c){
    gameObject.GetComponentInParent<HitInteractor>().PullTrigger(c);
    }

    void OnTriggerExit(Collider c){
        gameObject.GetComponentInParent<HitInteractor>().ResetTrigger(c);
    }
 
}

