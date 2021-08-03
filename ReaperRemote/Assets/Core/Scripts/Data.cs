using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Core.Data{
    public enum RootNote{
        C, C_Sharp, D_Flat, D, D_Sharp, E_Flat, E, F, F_Sharp, G_Flat, G, G_Sharp, A_Flat, A, A_Sharp, B_Flat, B 
    }
    public enum Scale{
        Major, Minor7, MinorHarmonic 
    }


public static class Data{ // need init from GameObject
    // private struct ScaleObject{
    //     int[] Scale;
        
    // }
    private static bool init = false;
    
    private static Dictionary<Scale, int[]> scales; // # in scale (all 12 tones in octave), root = 1, note before octave = 12
    public static Dictionary<Scale, int[]> Scales {
        get => scales;
    }
    private static Dictionary<RootNote, Dictionary<int, int>> rootNotes; // get all octaves and midiNotes for a root note. (inner dictionary : Octave, midiNote) 
    public static Dictionary<RootNote, Dictionary<int, int>> RootNotes {
        get => rootNotes;
    }

    // called from Unity Scene
    static public void Init(){
        if(init) {Debug.LogError("Data class already initialized!");}
        init = true; 
        Debug.Log("Static \"constructor\" called!");

        InitScales();
        InitRootNotes();

        void InitScales(){
            scales = new Dictionary<Scale, int[]>(){
                {Scale.Major, new[] {1,3,5,6,8,10,12}},
                {Scale.Minor7, new[] {1,3,4,6,8,9,11}}
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
    } // end Init()

    
}

}