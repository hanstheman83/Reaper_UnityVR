using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//

public class LayerManager : MonoBehaviour
{
    [SerializeField] Background background;
    [SerializeField] List<Layer> layersList; // top layer has most weight
    // or get calc combined!
    private Layer activeLayer;
    private Color32[] combinedLayers;
    public Color32[] CombinedLayers {get => combinedLayers;}


    // Start is called before the first frame update
    void Start()
    {
        activeLayer = layersList[0];
        // init all layers with a = 0, all black
        // init background with all black a = 255
        // 
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateLayer(int height, int width){

    }

    public void DrawPixelOnActiveLayer(int n, Color32 color){
        activeLayer.DrawPixel(n, color);
        // null ?
        // ignore at color.a = 0f

        // check if a < 255, then do blending with layer below
        // 
        //
    }

    public Color32[] GetCombinedLayersArray(){
        combinedLayers = new Color32[0];
        return combinedLayers;
    }


}
