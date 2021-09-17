using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    private Color32[] pixels;
    public Color32[] Pixels {get => pixels;}
    //public const Color32 defaultColor = Colors.Spring;


    public void InitializeBackground(int width, int height, Color32 color = default){
        pixels = new Color32[width * height];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
