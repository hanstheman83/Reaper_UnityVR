using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1d array of colors
// layer properties : locked, visible
// blend mode
public class Layer_GPU : MonoBehaviour
{
    private new string name;
    public string Name {get => name;}
    private Vector4[] pixels;
    public Vector4[] Pixels {get => pixels; set => pixels = value;}
    
    public void InitializeLayer(int width, int height){
        pixels = new Vector4[width * height];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = (Vector4)Color.white;
        }
    }

    
}
