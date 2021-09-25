using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Transparency on level of Pixel : for soft brush strokes!

public class LayerManager_GPU : MonoBehaviour
{
    [SerializeField] Background_GPU background;
    [SerializeField] List<Layer_GPU> layersList; // top layer is index 0
    private Layer_GPU activeLayer;

    void Awake()
    {
        activeLayer = layersList[0];
        // need null pointer as last entry in list
        layersList.Add(null);
    }

    public void InitializeAllLayers(int width, int height){
        background.InitializeBackground(width, height, Colors.White);
        foreach (var layer in layersList)
        {
            layer?.InitializeLayer(width, height);
        }
    }
    // TODO: update active layer after/during drawing stroke - pass array to drawing class
}
