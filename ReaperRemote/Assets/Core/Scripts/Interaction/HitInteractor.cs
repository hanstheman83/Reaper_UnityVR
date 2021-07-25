using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC.Examples;
using Core.IO;

public class HitInteractor : MonoBehaviour
{
    [SerializeField] private MTransmitter transmitter;
    [SerializeField][Range(1,127)] private int midiNote;

    public int MidiNote {get => midiNote; set => midiNote = value;}
    public MTransmitter Transmitter {set => transmitter = value;}
    
    

    private void OnCollisionEnter(Collision collision){
        Debug.Log("Collision");

    }

    // private void OnTriggerEnter(Collider other){
    //     Debug.Log("Triggered!");
    //     //GetComponent<Renderer>().material.color = Color.red;
    //     // start note
    //     transmitter.TransmitMidi(true, midiNote);

    // }

    public void PullTrigger(Collider other){
        Debug.Log("Triggered!");
        //GetComponent<Renderer>().material.color = Color.red;
        // start note
        transmitter.TransmitMidi(true, midiNote, 80);

    }




    // private void OnTriggerExit(Collider other){
    //     Debug.Log("Triggered Exiting!");
    //     //GetComponent<Renderer>().material.color = Color.blue;
    //     transmitter.TransmitMidi(false, midiNote);
    // }
    public void ResetTrigger(Collider other){
        Debug.Log("Triggered Exiting!");
        //GetComponent<Renderer>().material.color = Color.blue;
        transmitter.TransmitMidi(false, midiNote, 0);
    }

    
}
