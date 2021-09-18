using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//

public class LayerManager : MonoBehaviour
{
    [SerializeField] Background background;
    [SerializeField] List<Layer> layersList; // top layer is index 0
    // or get calc combined!
    private Layer activeLayer;
    private Color32[] combinedLayers;
    public Color32[] CombinedLayers {get => combinedLayers;}



    // Start is called before the first frame update
    void Start()
    {
        activeLayer = layersList[0];

        
        // if a on draw on active layer is 1 set thin to cl, otherwise calc color!
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeAllLayers(int width, int height){
        background.InitializeBackground(width, height, Colors.White);
        foreach (var layer in layersList)
        {
            layer.InitializeLayer(width, height);
        }
        combinedLayers = new Color32[width * height];
        // init combinedlayers
        for (var i = 0; i < combinedLayers.Length; i++)
        {
            combinedLayers[i] = Colors.Black;
        }
    }

    // public void CreateLayer(int height, int width){

    // }

    public void DrawPixelOnActiveLayer(int n, Color32 color){
        activeLayer.DrawPixel(n, color);
        // null ?
        // ignore at color.a = 0f

        // check if a < 255, then do blending with layer below
        // //Add together the corresponding value from each colour. int r = Colour1.r + Colour2.r;
        //Then divide the 3 ints by 2: int r /= 2;
        //Set it to a new colour: Color newColour = new Color(r, g, b);
        // in linear color texture! 
        // scale by alpha!
        
        //
        if(color.a < 255) { // calc combined color, background + layers
            // background.pixels[n]
            throw new NotImplementedException();
        }else if(activeLayer == layersList[0]){ // is highest layer
            combinedLayers[n] = color;
        }else if(layersList[0].Pixels[n].a == 255){ // case : the highest layer has a = 255, do nothing
            // do nothing
        }else{
            // blend with top layer
            throw new NotImplementedException();
        }
    }
}
