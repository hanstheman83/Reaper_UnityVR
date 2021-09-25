using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background_GPU : MonoBehaviour
{
    private Color32[] pixels;
    public Color32[] Pixels {get => pixels;}
    //public const Color32 defaultColor = Colors.Spring;


    public void InitializeBackground(int width, int height, Color32 color = default){
        pixels = new Color32[width * height];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
    }
}
