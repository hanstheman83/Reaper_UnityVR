using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataTransfer = Core.IO;
using Core.Interactions;

namespace Core.Instruments{

public class InstrumentGenerator : MonoBehaviour
{
    [SerializeField] private GameObject instrumentsLocation;
    [SerializeField] private Transform positionTransform;
    [SerializeField] private Harp harp;
    [SerializeField] private GameObject stringObject;
    private Vector3 position;
    private List<GameObject> listStrings;
    
    

    // Start is called before the first frame update
    void Start()
    {
        listStrings = new List<GameObject>();


        Data.Init();

        GenerateInstrument4();

        void GenerateInstrument4(){ // with string prefab
            Debug.Log("Generating instrument".Colorize(Color.cyan));
            GameObject newInstrument = new GameObject("new harp");
            newInstrument.transform.position = positionTransform.position;
            newInstrument.transform.SetParent(instrumentsLocation.transform); 
            List<int> allMidiNotesInScale = Data.GetMidiNotesInScale(harp.scale, harp.rootNote, harp.firstOctave, harp.numberOfStrings);
            for(int i = 0; i < allMidiNotesInScale.Count; i++){
                GameObject newString = Instantiate(stringObject, newInstrument.transform);
                Debug.Log("newString instantiated..".Colorize(Color.blue));
                newString.name = "String MidiNote " + allMidiNotesInScale[i]; // new GameObject same as Instantiate (creates in scene..)
                listStrings.Add(newString);
                // newString.transform.SetParent(newInstrument.transform);
                newString.transform.localPosition = Vector3.zero;
                // change size etc.. child cube is scaled.
                // displace from zero at newHarp
                float stringOffset = harp.stringLength/2;
                newString.transform.localPosition = new Vector3(i * harp.distanceBetweenStrings, stringOffset, 0);
                GameObject cube = newString.transform.Find("Cube").gameObject;
                Debug.Log("cube found ? ".Colorize(Color.blue)+ (cube ? "found" : "not found").Colorize(Color.green));
                cube.transform.localScale = new Vector3(harp.stringWidth, harp.stringLength, harp.stringWidth);
                HitInteractor hitInteractor = newString.GetComponent<HitInteractor>();
                hitInteractor.MidiNote = allMidiNotesInScale[i];
                hitInteractor.Transmitter = FindObjectOfType<DataTransfer::MTransmitter>();
                // set transmitter
            }

        }

        void GenerateInstrument3(){ // Generate without prefab

            GameObject newInstrument = new GameObject("new harp");
            newInstrument.transform.position = positionTransform.position;
            newInstrument.transform.SetParent(instrumentsLocation.transform); 
            List<int> allMidiNotesInScale = Data.GetMidiNotesInScale(harp.scale, harp.rootNote, harp.firstOctave, harp.numberOfStrings);

            for(int i = 0; i < allMidiNotesInScale.Count; i++){
                GameObject newString = new GameObject("String MidiNote " + allMidiNotesInScale[i]); // new GameObject same as Instantiate (creates in scene..)
                listStrings.Add(newString);
                newString.transform.SetParent(newInstrument.transform);
                newString.transform.localPosition = Vector3.zero;
                // displace from zero at newHarp
                float stringOffset = harp.stringLength/2;
                newString.transform.localPosition = new Vector3(i * harp.distanceBetweenStrings, stringOffset, 0);
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.SetParent(newString.transform);
                cube.transform.localPosition = Vector3.zero;
                // cylinder is 2m height by default
                cube.transform.localScale = new Vector3(harp.stringWidth, harp.stringLength, harp.stringWidth);
                cube.GetComponent<Collider>().isTrigger = true;
                ChildTrigger childTrigger = cube.AddComponent<ChildTrigger>();
                HitInteractor hitInteractor = newString.AddComponent<HitInteractor>();

                hitInteractor.MidiNote = allMidiNotesInScale[i];
                hitInteractor.Transmitter = FindObjectOfType<DataTransfer::MTransmitter>();
                cube.layer = LayerMask.NameToLayer("MusicInteraction");
            }
        }


        #region old code
        void GenerateInstrument2()
        {
            GameObject newInstrument = new GameObject("new harp");
            newInstrument.transform.position = positionTransform.position;
            newInstrument.transform.SetParent(instrumentsLocation.transform); 
            if(!Data.RootNotes.TryGetValue(harp.rootNote, out var innerDic)){ Debug.LogError("No key found!"); }
            if(!innerDic.TryGetValue(harp.firstOctave, out int firstMidiNote)){ Debug.LogError("No key found!"); } // first midiNote is the root note in specified octave
            int midiNote = firstMidiNote;
            Debug.Log("First midi note " + midiNote);
            if(!Data.Scales.TryGetValue(harp.scale, out int[] scale)) { Debug.LogError("No key found!"); }
            int j = 0; int k = 0;

            for(int i = 0; i < harp.numberOfStrings; i++){
                if(midiNote >= 127) { break; } // range is 0 - 126
                // name GO as midiNote!
                GameObject newString = new GameObject("String MidiNote " + midiNote); // new GameObject same as Instantiate (creates in scene..)

                newString.transform.SetParent(newInstrument.transform);
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
                hitInteractor.MidiNote = midiNote;
                hitInteractor.Transmitter = FindObjectOfType<DataTransfer::MTransmitter>();
                cube.AddComponent<ChildTrigger>();
                cube.layer = LayerMask.NameToLayer("MusicInteraction");
                // Calc next midiNote
                // from root note ? octave ?
                if(j < scale.Length -1) { j++; } else { j = 0; k++; }
                Debug.Log($"j : {j}, k : {k}");
                //if(j == 0) { midiNote += scale[j]; } else { midiNote += scale[j] - scale[j-1]; } 
                midiNote = (firstMidiNote + (k * 12)) + scale[j] - 1;
                
                
                // create first string plus scales[j] 
                // iterate j : next in scales.. reset if scales.length.
                // calc new midiNote : plus j in scale.. 
                // iterate scale : add (result -1) for correct distance
                
            }
            
            
        }

        void GenerateInstrument(){
            int numStrings = harp.numberOfStrings;
            // A0: midi note 21, C3 : Midi note 48, 
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
                hitInteractor.Transmitter = FindObjectOfType<DataTransfer::MTransmitter>();
                cube.AddComponent<ChildTrigger>();
                cube.layer = LayerMask.NameToLayer("MusicInteraction");

                currentString++;
            }

        #endregion old code

        }
    } // end Start()

    #region UI Callbacks
    public void ChangeScale(RootNote root, Scale scale, int firstOctave){
        // loop through list and change midinotes
        List<int> allMidiNotesInScale = Data.GetMidiNotesInScale(scale, root, firstOctave, listStrings.Count);
        for(int i = 0; i < allMidiNotesInScale.Count; i++){
            GameObject someString = listStrings[i];
            someString.GetComponent<HitInteractor>().MidiNote = allMidiNotesInScale[i];
            someString.name = "String MidiNote " + allMidiNotesInScale[i];
        }
    }

    
    
    
    
    
    
    #endregion UI Callbacks





} // end Class 
}