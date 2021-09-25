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
    private Color32[] pixels;
    public Color32[] Pixels {get => pixels;}
    
    public void InitializeLayer(int width, int height){
        pixels = new Color32[width * height];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Colors.Transparent;
        }
    }

    
}
