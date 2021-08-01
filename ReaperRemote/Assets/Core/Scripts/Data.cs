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
    private static bool init = false;
    private static Dictionary<Scale, int[]> scales = new Dictionary<Scale, int[]>();

    static public void Init(){
        if(init) {Debug.LogError("Data class already initialized!");}
        init = true; 
        Debug.Log("Static constructor called!");

        
    }
}

}