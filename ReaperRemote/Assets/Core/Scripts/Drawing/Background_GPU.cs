using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background_GPU : MonoBehaviour
{
    private Color[] pixels;
    public Color[] Pixels {get => pixels;}
    //public const Color32 defaultColor = Colors.Spring;


    public void InitializeBackground(int width, int height, Color32 color = default){
        pixels = new Color[width * height];
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
    }
}
