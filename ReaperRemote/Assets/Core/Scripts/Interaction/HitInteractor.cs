using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC.Examples;
using Core.IO;

namespace Core.Interactions{

public class HitInteractor : MonoBehaviour
{
    [SerializeField] private MTransmitter transmitter;
    [SerializeField][Range(0,126)] private int midiNote;

    public int MidiNote {get => midiNote; set => midiNote = value;}
    public MTransmitter Transmitter {set => transmitter = value;}
    private ChildTrigger childTrigger;
    
    private void Awake() {
        childTrigger = GetComponentInChildren<ChildTrigger>();
        if(!childTrigger) Debug.Log("childtrigger was null".Colorize(Color.red));
    }
    private void OnEnable() {
        if(!childTrigger) {childTrigger = GetComponentInChildren<ChildTrigger>();}
        childTrigger.childTriggeredEnterEvent += PullTrigger;
        childTrigger.childTriggeredExitEvent += ResetTrigger;
    }
    private void OnDisable() {
        childTrigger.childTriggeredEnterEvent -= PullTrigger;
        childTrigger.childTriggeredExitEvent -= ResetTrigger;
    }

    public void PullTrigger(Collider other){
        Debug.Log("Triggered!");
        //GetComponent<Renderer>().material.color = Color.red;
        // start note
        transmitter.TransmitMidi(true, midiNote, 80);

    }

    public void ResetTrigger(Collider other){
        Debug.Log("Triggered Exiting!");
        //GetComponent<Renderer>().material.color = Color.blue;
        transmitter.TransmitMidi(false, midiNote, 0); // 0 is velocity 0, also note off event
    }

    
}

}