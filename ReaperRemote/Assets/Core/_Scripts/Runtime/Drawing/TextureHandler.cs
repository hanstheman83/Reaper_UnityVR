using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;

namespace Core.Drawing{

public class TextureHandler : MonoBehaviour
{
    [SerializeField] DrawingOnTexture_GPU m_DrawingOnTexture;
    [SerializeField] Drawing_ButtonInputController m_ButtonController;
    private RenderTexture m_Texture00;
    //private Color[]
    // private ComputeBuffer GPU_Texture00_Buffer;
    private bool isInit = false;
    [SerializeField] private ComputeShader m_TextureHandling_Compute;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void HookTextureToShader(){
        int kernel = m_TextureHandling_Compute.FindKernel("CSMain");
        m_TextureHandling_Compute.SetTexture(kernel, "_RenderTexture00", m_DrawingOnTexture.renderTexture_00);
#if !UNITY_EDITOR
        // drawingOnTexture.renderTexture_06; // has access to !UNITY_EDITOR 
#endif        
    }

    public void Undo(){
        Debug.Log("Undo called.....".Colorize(Color.cyan));
        if(!isInit){
            HookTextureToShader();
            isInit = true;
        }

        // draw on texture 1 color..
        // dispatch
        int kernel = m_TextureHandling_Compute.FindKernel("CSMain");

        m_TextureHandling_Compute.Dispatch(kernel, 1024, 1024, 1);
        
    }
}

}