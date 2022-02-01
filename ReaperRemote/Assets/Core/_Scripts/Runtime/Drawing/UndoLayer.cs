using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Core.Drawing{
/// <summary>
/// 
/// </summary>
public class UndoLayer
{
    // holds data, 20 1D arrays, saved in compute shader, 
    // 
    List<Color32[]> textureColorList = new List<Color32[]>();

    public UndoLayer(int resolution, int numberOfTextures){
        InitTextureList(resolution, numberOfTextures);
    }
    private void InitTextureList(int resolution, int numberOfTextures){
        for (var i = 0; i < numberOfTextures; i++)
        {
            textureColorList.Add(new Color32[resolution * resolution]);
        }
    }




}

}