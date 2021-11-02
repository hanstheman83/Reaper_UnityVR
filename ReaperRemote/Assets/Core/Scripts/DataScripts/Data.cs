using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Core{

    #region Colors etc...
    
    /// <summary>
    /// Constructor : (Vector2Int position, Color color).
    /// </summary>
    public struct Pixel{
        public uint position_x, position_y;
        public float color_r, color_g, color_b, color_a;
        public Pixel(Vector2Int position, Color color){ // should auto unpack to compute shader!
            position_x = (uint)position.x;
            position_y = (uint)position.y;
            color_r = color.r;
            color_g = color.g;
            color_b = color.b;
            color_a = color.a;
        }
    } 

    #endregion Colors etc...


    /// <summary>
    /// All 12 notes in the western chromatic musical scale.
    /// Contains duplicate notes eg. C_Sharp and D_Flat
    /// </summary>
    public enum RootNote{
        C, C_Sharp, D_Flat, D, D_Sharp, E_Flat, E, F, F_Sharp, G_Flat, G, G_Sharp, A_Flat, A, A_Sharp, B_Flat, B 
    }
    /// <summary>
    /// Common scales.
    /// </summary>
    public enum Scale{
        Major, Minor7, MinorHarmonic 
    }
    /// <summary>
    /// When mute pedal is Off strings will keep being On until muted by a hit with 0 velocity. <br/>
    /// The mute pedal is VR only - not Midi.
    /// </summary>
    public enum VRMutePedalState{
        On, Off
    }

    
/// <summary>
/// Static Data class, holds all basic data.
/// </summary>
public static class Data{ // need init from GameObject
    private static bool init = false;
    public static VRMutePedalState VR_MutePedalState = VRMutePedalState.On;
    private static Dictionary<Scale, int[]> scales; // # in scale (all 12 tones in octave), root = 1, note before octave = 12
    /// <summary>
    /// Dictionary that holds all scales as integer arrays.
    /// </summary>
    public static Dictionary<Scale, int[]> Scales {
        get => scales;
    }
    private static Dictionary<RootNote, Dictionary<int, int>> rootNotes; // get all octaves and midiNotes for a root note. (inner dictionary : Octave, midiNote) 
    /// <summary>
    /// Dictionary that holds another dictionary with key:Octave, value:MidiNote 
    /// </summary>
    public static Dictionary<RootNote, Dictionary<int, int>> RootNotes {
        get => rootNotes;
    }

    // called from Unity Scene. You cannot use normal constructor since it is never called!
    static public void Init(){
        if(init) {Debug.LogError("Data class already initialized!");}
        init = true; 
        Debug.Log("Static \"constructor\" called!");

        InitScales();
        InitRootNotes();

        void InitScales(){
            scales = new Dictionary<Scale, int[]>(){
                {Scale.Major, new[] {1,3,5,6,8,10,12}},
                {Scale.Minor7, new[] {1,3,4,6,8,9,11}},
                {Scale.MinorHarmonic, new[] {1,3,4,6,8,9,12}},
            }; // #s in scale (all 12 tones in octave), root = 1, note before octave = 12
        }    

        void InitRootNotes(){
            // Start from lowest octave.. calculate the ones above. From C

            // need midiNote in lowest octave per rootnote
            // using the Roland system : http://www.midisolutions.com/chapter3.htm
            // c-1 : 0, c-0 : 12, up to c-8 : 120
            // a-0 : 21 is lowest note on standard piano

            // 
            rootNotes = new Dictionary<RootNote, Dictionary<int, int>>(){ // only init of lowest octave -1
                {RootNote.C, new Dictionary<int, int>(){{-1, 0}}},
                {RootNote.C_Sharp, new Dictionary<int, int>(){{-1, 1}}},
                {RootNote.D_Flat, new Dictionary<int, int>(){{-1, 1}}},
                {RootNote.D, new Dictionary<int, int>(){{-1, 2}}},
                {RootNote.D_Sharp, new Dictionary<int, int>(){{-1, 3}}},
                {RootNote.E_Flat, new Dictionary<int, int>(){{-1, 3}}},
                {RootNote.E, new Dictionary<int, int>(){{-1, 4}}},
                {RootNote.F, new Dictionary<int, int>(){{-1, 5}}},
                {RootNote.F_Sharp, new Dictionary<int, int>(){{-1, 6}}},
                {RootNote.G_Flat, new Dictionary<int, int>(){{-1, 6}}},
                {RootNote.G, new Dictionary<int, int>(){{-1, 7}}},
                {RootNote.G_Sharp, new Dictionary<int, int>(){{-1, 8}}},
                {RootNote.A_Flat, new Dictionary<int, int>(){{-1, 8}}},
                {RootNote.A, new Dictionary<int, int>(){{-1, 9}}},
                {RootNote.A_Sharp, new Dictionary<int, int>(){{-1, 10}}},
                {RootNote.B_Flat, new Dictionary<int, int>(){{-1, 10}}},
                {RootNote.B, new Dictionary<int, int>(){{-1, 11}}},
            };

            // to c-8
            for(int i = 0; i <= 8; i++){
                // get inner dictionary by giving outer key, give inner key (i-1 for octave before), get midinote and add +12 in new entry (i, midinote + 12) 
                //rootNotes;

                //
                foreach(KeyValuePair<RootNote, Dictionary<int, int>> entry in rootNotes)
                {
                    // do something with entry.Value or entry.Key
                    var innerDictionary = entry.Value;
                    int priorMidiNote = innerDictionary[i-1];
                    innerDictionary.Add(i, priorMidiNote + 12);
                }
            }
        }
    
        void InitNoteRelationships(){
            // midiNote, relationship, midiNote, eg. (60, MajorThird, 56)
            // ? 
        }
    } // end Init()

    /// <summary>
    /// Creates a scale from specified parameters.
    /// </summary>
    /// <returns>Midi Notes in scale (int[])</returns>
    static public List<int> GetMidiNotesInScale(Scale scale, RootNote rootNote, int firstOctave, int numberOfNotes){
        List<int> newIntList = new List<int>();
        if(!rootNotes.TryGetValue(rootNote, out var innerDic)){ Debug.LogError("No key found!"); }
        if(!innerDic.TryGetValue(firstOctave, out int firstMidiNote)){ Debug.LogError("No key found!"); } // first midiNote is the root note in specified octave
        int midiNote = firstMidiNote;
        Debug.Log("First midi note " + midiNote);
        if(!scales.TryGetValue(scale, out int[] scaleArray)) { Debug.LogError("No key found!"); }
        int j = 0; int k = 0;

        for(int i = 0; i < numberOfNotes; i++){
            if(midiNote >= 127) { break; } // range is 0 - 126
            newIntList.Add(midiNote);
            if(j < scaleArray.Length -1) { j++; } else { j = 0; k++; }
            //Debug.Log($"j : {j}, k : {k}");
            //if(j == 0) { midiNote += scale[j]; } else { midiNote += scale[j] - scale[j-1]; } 
            midiNote = (firstMidiNote + (k * 12)) + scaleArray[j] - 1;
        }

        return newIntList; // TODO copy into array of exact size!
        
    }

    static public int[] GetMidiNotesInChord(){ // Chord chord, Root, scale, numNotes
        //  create a (chord, relationship, chord) tuple list! -- relationships can be of same scale diatonic etc...
        // Study some music theory for inspiration!!
        // Advanced relations ?? 

        int[] newArray = new int[0];
        

        return newArray;
    }
    /// <summary>
    /// Input a midinote, a relation(eg. MajorThird), the relative octave(0 for current, -1 for previous, 1 for next)
    /// Note : A MajorThird in the previous octave is the MajorThird minus an octave. The Ninth is the Second plus an octave.
    /// </summary>
    /// <returns>The other midinote in the relation (int)</returns>
    static public int GetMidiNoteInRelation(){ // int midiNote, relation, relative octave (0 for current) 
        return 0;
    }
    
}

}