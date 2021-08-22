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
    public Material noteOnMaterial, noteOffMaterial;
    private bool isOn = false;
    // VR only pedal : strings remains ON until muted by hit at 0 velocity!
    
    private void Awake() {
        childTrigger = GetComponentInChildren<ChildTrigger>();
        if(!childTrigger) Debug.Log("childtrigger was null".Colorize(Color.red));
    }
    private void OnEnable() {
        if(!childTrigger) {childTrigger = GetComponentInChildren<ChildTrigger>();}
        childTrigger.childTriggeredEnterEvent += OnChildTriggerEntered;
        childTrigger.childTriggeredExitEvent += OnChildTriggerExited;
    }
    private void OnDisable() {
        childTrigger.childTriggeredEnterEvent -= OnChildTriggerEntered;
        childTrigger.childTriggeredExitEvent -= OnChildTriggerExited;
    }

    public void OnChildTriggerEntered(Collider other){
        Debug.Log("Triggered! ");
        //GetComponent<Renderer>().material.color = Color.red;
        // start note
        int hitVelocity = other.GetComponentInParent<IHitVelocity>().HitVelocity;
        if(hitVelocity == 0 && Data.VR_MutePedalState == VRMutePedalState.Off){
            transmitter.TransmitMidiNote(0, midiNote, 0); // manually muting a note    
            childTrigger.GetComponent<Renderer>().material = noteOffMaterial;
            return;
        }
        else if(hitVelocity == 0) return;
        transmitter.TransmitMidiNote(0, midiNote, hitVelocity);
        childTrigger.GetComponent<Renderer>().material = noteOnMaterial;
    }

    public void OnChildTriggerExited(Collider other){
        Debug.Log("Triggered Exiting!");
        //GetComponent<Renderer>().material.color = Color.blue;
        if(Data.VR_MutePedalState == VRMutePedalState.Off){
            Debug.Log("not auto muting...");
        }else {
            transmitter.TransmitMidiNote(0, midiNote, false);
            childTrigger.GetComponent<Renderer>().material = noteOffMaterial;

        }
    }

    
}

}