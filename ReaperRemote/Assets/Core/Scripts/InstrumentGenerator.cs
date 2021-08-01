using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.IO;
using Core.Data;


public class InstrumentGenerator : MonoBehaviour
{
    [SerializeField] private GameObject instrumentsLocation;
    [SerializeField] private Transform positionTransform;
    [SerializeField] private Harp harp;
    private Vector3 position;


    // Start is called before the first frame update
    void Start()
    {
        Data.Init();

        GenerateInstrument();

        void GenerateInstrument(){
            // from harp.firstNote;
            // to harp.lastNote;
            // harp.distanceBetweenStrings;
            
            // create 

            // # harp strings
            int numStrings = harp.numStrings;
            // A0: midi note 21, C3 : Midi note 48, 
            // int currentString = harp.firstNote;
            int currentString = 10;

            // pattern for scale.. 
            
            GameObject newHarp = new GameObject("new harp");
            newHarp.transform.position = positionTransform.position;
            newHarp.transform.SetParent(instrumentsLocation.transform);
            

            for(int i = 0; i < numStrings; i++){
                GameObject newString = new GameObject();
                //newString = Instantiate(newString, Vector3.zero, Quaternion.identity);
                newString.transform.SetParent(newHarp.transform);
                newString.transform.localPosition = Vector3.zero;
                // displace from zero at newHarp
                float stringOffset = harp.stringLength/2;
                newString.transform.localPosition = new Vector3(i * harp.distanceBetweenStrings, stringOffset, 0);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(newString.transform);
                cube.transform.localPosition = Vector3.zero;
                // cylinder is 2m height by default
                cube.transform.localScale = new Vector3(harp.stringWidth, harp.stringLength, harp.stringWidth);
                // 
                cube.GetComponent<Collider>().isTrigger = true;
                HitInteractor hitInteractor = newString.AddComponent<HitInteractor>();
                hitInteractor.MidiNote = currentString;
                hitInteractor.Transmitter = FindObjectOfType<MTransmitter>();
                cube.AddComponent<ChildTrigger>();
                cube.layer = LayerMask.NameToLayer("MusicInteraction");

                currentString++;
            }


        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
