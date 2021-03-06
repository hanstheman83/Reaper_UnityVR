using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Transparency on level of Pixel : for soft brush strokes!

public class LayerManager : MonoBehaviour
{
    [SerializeField] Background background;
    [SerializeField] List<Layer> layersList; // top layer is index 0
    // or get calc combined!
    private Layer activeLayer;
    // private Color32[] combinedLayers;
    // public Color32[] CombinedLayers {get => combinedLayers;}
    [HideInInspector] public Texture2D texture;
    int topPixelCounter = 0;
    int CalculateTopPixelCounter = 0;


    // Start is called before the first frame update
    void Awake()
    {
        activeLayer = layersList[0];
        // need null pointer as last entry in list
        layersList.Add(null);
        if(layersList[2] == null) Debug.Log("layer is null");

        
        // if a on draw on active layer is 1 set thin to cl, otherwise calc color!
        
    }

    private void Start() {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeAllLayers(int width, int height){
        background.InitializeBackground(width, height, Colors.White);
        foreach (var layer in layersList)
        {
            layer?.InitializeLayer(width, height);
        }
        //combinedLayers = new Color32[width * height];
        // init combinedlayers
        CalculateCombinedLayers(); 
    }
    private void CalculateCombinedLayers(){
        for (var i = 0; i < background.Pixels.Length; i++)
        {
            var topPixel = CalculateTopPixel(i, 0);
            //combinedLayers[i] = topPixel; // recursive, start from top layer, index = 0
            var data = texture.GetRawTextureData<Color32>();
            data[i] = topPixel;
        }
    }

    /// <summary>
    /// Recursive Function. Calculate pixel at layerPosition in list. Index 0 is top layer.
    /// </summary>
    private Color32 CalculateTopPixel(int pixel, int layerPosition){ // recursively from top, stop if layer has pixel alpha of 255!
        CalculateTopPixelCounter++; // debug only
        Color32 topPixel = layersList[layerPosition].Pixels[pixel]; 
        if(topPixel.a == 255){
            // topPixel has right color since this layer has alpha of 255!
            // do nothing
            topPixelCounter++; // debug only
        }else if(layersList[layerPosition + 1] != null){
            topPixel = PixelBlend(topPixel, CalculateTopPixel(pixel, (layerPosition + 1) ));
        }else {// if no layer blend with background
            // blend with background
            topPixel = PixelBlend(topPixel, background.Pixels[pixel]);
        }
        return topPixel;
    }

    // returning calculated pixel for combinedLayer - not setting alpha levels of layers. 
    // treating bottom pixel as alpha : a = 1 - topPixel.a in range 0 to 1 
    private Color32 PixelBlend(Color32 topPixel, Color32 bottomPixel){ // top - bottom : relative to their layer
        //Debug.Log("Pixel blending...");
        // blend based on top layer alpha
        // alpha is 0 to 255
        float topPixelAlphaPercentage = (topPixel.a/255f);
        return new Color32( (byte)Mathf.Clamp( ((topPixel.r * topPixelAlphaPercentage) + (bottomPixel.r * (1 - topPixelAlphaPercentage))), 0, 255 ),
                            (byte)Mathf.Clamp( ((topPixel.g * topPixelAlphaPercentage) + (bottomPixel.g * (1 - topPixelAlphaPercentage))), 0, 255 ),
                            (byte)Mathf.Clamp( ((topPixel.b * topPixelAlphaPercentage) + (bottomPixel.b * (1 - topPixelAlphaPercentage))), 0, 255 ),
                            1);
    }

    // only 2 layers + background before performance opt
    // public void CreateLayer(int height, int width){

    // }

    public void DrawPixelOnActiveLayer(int n, Color32 color){
        activeLayer.DrawPixel(n, color);
        var data = texture.GetRawTextureData<Color32>();
        var topPixel = CalculateTopPixel(n, 0);
        // combinedLayers[n] = topPixel; // update combined
        data[n] = topPixel;
    }
}
