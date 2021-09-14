using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Default
/// </summary>
public struct Brush{ // contains several arrays of diffrent sizes
    private int height;
    public int Height {get => height;}
    private int width;
    public int Width {get => width;}
    private float softness;
    public float Softness {get => softness;}
    private float roundness;
    public float Roundness {get => Roundness;}
    //
    private Color[][] brushSizes; // first index = sizeArray, 2nd index(in sizeArray) = pixels in brush
    // can add extra color arrays for "dirty" colors - procedually generated random dirt algos!

    /// <summary>
    /// Softness and Roundness values must be between 0 and 1!
    /// </summary>
    public Brush(int height = 5, int width = 5, float softness = 0f, float roundness = 1f){ // min pixel size
        this.height = height;
        this.width = width;
        this.softness = softness;
        this.roundness = roundness;
        this. brushSizes = new Color[0][]; // needed init in a struct!
        // Generate size arrays
        GenerateSizeArrays();
    }
    private void GenerateSizeArrays(){
        // use null for no pixel 
        // index 0 = org array
        // +1 = grow 1p , so 1p wider than org
        //

    }
}
public static class BrushGenerator
{

    public static Brush GenerateBrush(int height = 5, int width = 5, float softness = 0f){ // minimum pixel sizes for brush!
        return new Brush();
    }

}
