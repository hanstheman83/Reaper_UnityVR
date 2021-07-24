using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC.Examples;
using Core.IO;

public class HitInteractor : MonoBehaviour
{
    [SerializeField] private MTransmitter transmitter;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision){
        Debug.Log("Collision");

    }

    private void OnTriggerEnter(Collider other){
        Debug.Log("Triggered!");
        GetComponent<Renderer>().material.color = Color.red;
        // start note
        transmitter.TransmitMidi(true, 80);

    }
    private void OnTriggerExit(Collider other){
        Debug.Log("Triggered Exiting!");
        GetComponent<Renderer>().material.color = Color.blue;
        transmitter.TransmitMidi(false, 80);
    }

    
}
