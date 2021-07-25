using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.IO;


public class InstrumentGenerator : MonoBehaviour
{
    [SerializeField] private GameObject instrumentsLocation;
    [SerializeField] private Transform positionTransform;
    [SerializeField] private Harp harp;
    private Vector3 position;


    // Start is called before the first frame update
    void Start()
    {
        GenerateInstrument();

        void GenerateInstrument(){
            // from harp.firstNote;
            // to harp.lastNote;
            // harp.distanceBetweenStrings;
            
            // create 

            // # harp strings
            int numStrings = harp.lastNote - harp.firstNote;
            int currentString = harp.firstNote;
            
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
                GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cylinder.transform.SetParent(newString.transform);
                cylinder.transform.localPosition = Vector3.zero;
                // cylinder is 2m height by default
                cylinder.transform.localScale = new Vector3(harp.stringWidth, harp.stringLength/2, harp.stringWidth);
                // 
                cylinder.GetComponent<Collider>().isTrigger = true;
                HitInteractor hitInteractor = newString.AddComponent<HitInteractor>();
                hitInteractor.MidiNote = currentString;
                hitInteractor.Transmitter = FindObjectOfType<MTransmitter>();
                cylinder.AddComponent<ChildTrigger>();

                currentString++;
            }


        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
