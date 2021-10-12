using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

/// <summary>
/// A Brush is a collection of 1d arrays of different pixel sizes
/// </summary>
// public struct Brush{ // contains several arrays of diffrent sizes 
//     private int height;
//     public int Height {get => height;}
//     private int width;
//     public int Width {get => width;}
//     private float softness;
//     public float Softness {get => softness;}
//     private float roundness;
//     public float Roundness {get => Roundness;}
//     private int numberOfSizes;
//     //
//     private float[][] brushSizesValues; // 0 - 1f transparency of brush. 0 then no pixel. 1 then 100% strength.
//     // first index = size of brush, 2nd index  = transparency of pixels for that size of brush

    /// <summary>
    /// Softness and Roundness values must be between 0 and 1!
    /// </summary>
//     public Brush(int height = 5, int width = 5, int numberOfSizes = 10, float softness = 0f, float roundness = 1f){ // min pixel size
//         this.height = height;
//         this.width = width;
//         this.softness = softness;
//         this.roundness = roundness;
//         this.brushSizesValues = new float[numberOfSizes][]; // needed init in a struct!
//         this.numberOfSizes = numberOfSizes;
//         // Generate size arrays
//         GenerateSizeArrays();
//     }
//     private void GenerateSizeArrays(){
//         // use null for no pixel 
//         // index 0 = org array
//         // +1 = grow 1p , so 1p wider than org
//         //

//         // 1st version : square a x b
//         // softness : 
        
//         // 
//         int counter;
//         for (var i = 0; i < numberOfSizes; i++)
//         {   
//             counter = 0;
//             for (var j = 0; j < height + i; j++)
//             {
//                 for (var k = 0; k < width + i; k++)
//                 {
//                     brushSizesValues[i][counter] = 1f;
//                     counter++;
//                 }
//             }
//         }
//     }
// }
public static class BrushGenerator
{
    static public _Pixel[] smallBrush3x3 = {
        new _Pixel(new Vector2Int(-1,-1), Color.blue),
        new _Pixel(new Vector2Int(0,-1), Color.blue),
        new _Pixel(new Vector2Int(1,-1), Color.blue),
        new _Pixel(new Vector2Int(-1,0), Color.blue),
        new _Pixel(new Vector2Int(0,0), Color.blue),
        new _Pixel(new Vector2Int(1,0), Color.blue),
        new _Pixel(new Vector2Int(-1,1), Color.blue),
        new _Pixel(new Vector2Int(0,1), Color.blue),
        new _Pixel(new Vector2Int(1,1), Color.blue)
    };

    // public static Brush GenerateBrush(int height = 5, int width = 5, float softness = 0f){ // minimum pixel sizes for brush!
    //     return new Brush();
    // }



}
