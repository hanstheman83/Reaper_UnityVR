using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;

namespace Core.Drawing{

public class TextureHandler : MonoBehaviour
{
    [SerializeField] DrawingOnTexture_GPU m_DrawingOnTexture;
    private RenderTexture m_Texture;
    private Color[] m_CPU_TextureData;
    private ComputeBuffer GPU_TextureData;
    private bool isInit = false;
    [SerializeField] private ComputeShader m_TextureHandling_Compute;
    List<RenderTexture> m_Textures = new List<RenderTexture>();

    // Start is called before the first frame update
    void Start()
    {
        SetupDataBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetupDataBuffers()
    {
        SetupCPU_Buffer();
        SetupGPU_Buffer();
    }

    private void SetupGPU_Buffer(){
        int kernel1 = m_TextureHandling_Compute.FindKernel("SaveUndoPoint");
        int kernel2 = m_TextureHandling_Compute.FindKernel("Undo");
        GPU_TextureData = new ComputeBuffer(m_CPU_TextureData.Length, sizeof(float) * 4);
        GPU_TextureData.SetData(m_CPU_TextureData);
        m_TextureHandling_Compute.SetBuffer(kernel1, "_TextureData", GPU_TextureData);
        m_TextureHandling_Compute.SetBuffer(kernel2, "_TextureData", GPU_TextureData);
    }    

    private void SetupCPU_Buffer()
    {
        m_CPU_TextureData = new Color[1024 * 1024]; // TODO: no magic #s
        for (var i = 0; i < m_CPU_TextureData.Length; i++)
        {
            m_CPU_TextureData[i] = Color.magenta;
        }
    }

    private void MakeTextureList(){
        m_Textures.Add(m_DrawingOnTexture.renderTexture_00);
        m_Textures.Add(m_DrawingOnTexture.renderTexture_01);
        m_Textures.Add(m_DrawingOnTexture.renderTexture_02);
        m_Textures.Add(m_DrawingOnTexture.renderTexture_03);
#if !UNITY_EDITOR
        textures.Add(m_DrawingOnTexture.renderTexture_04);
        textures.Add(m_DrawingOnTexture.renderTexture_05);
        textures.Add(m_DrawingOnTexture.renderTexture_06);
        textures.Add(m_DrawingOnTexture.renderTexture_07);
        textures.Add(m_DrawingOnTexture.renderTexture_08);
        textures.Add(m_DrawingOnTexture.renderTexture_09);
        textures.Add(m_DrawingOnTexture.renderTexture_10);
        textures.Add(m_DrawingOnTexture.renderTexture_11);
        textures.Add(m_DrawingOnTexture.renderTexture_12);
        textures.Add(m_DrawingOnTexture.renderTexture_13);
        textures.Add(m_DrawingOnTexture.renderTexture_14);
        textures.Add(m_DrawingOnTexture.renderTexture_15);
        textures.Add(m_DrawingOnTexture.renderTexture_16);
        textures.Add(m_DrawingOnTexture.renderTexture_17);
        textures.Add(m_DrawingOnTexture.renderTexture_18);
        textures.Add(m_DrawingOnTexture.renderTexture_19);
#endif
    }

    private void HookTextureToShader(){
        int kernel1 = m_TextureHandling_Compute.FindKernel("SaveUndoPoint");
        int kernel2 = m_TextureHandling_Compute.FindKernel("Undo");
        m_Texture = m_DrawingOnTexture.renderTexture_00;
        m_TextureHandling_Compute.SetTexture(kernel1, "_RenderTexture", m_Texture);
        m_TextureHandling_Compute.SetTexture(kernel2, "_RenderTexture", m_Texture);
#if !UNITY_EDITOR
        // drawingOnTexture.renderTexture_06; // has access to !UNITY_EDITOR 
#endif        
    }

#region Event Callbacks
    public void SaveState(){
        if(!isInit){
            MakeTextureList();
            HookTextureToShader();
            isInit = true;
        }
        Debug.Log("Saving state..");
        int kernel = m_TextureHandling_Compute.FindKernel("SaveUndoPoint");
        m_TextureHandling_Compute.Dispatch(kernel, 1024, 1024, 1);
        GPU_TextureData.GetData(m_CPU_TextureData);
    }
#endregion Event Callbacks

#region Button Callbacks
    public void Undo(){
        if(!isInit) return;
        Debug.Log("Undo called.....".Colorize(Color.cyan));
        // draw on texture 1 color..
        // dispatch
        int kernel = m_TextureHandling_Compute.FindKernel("Undo");

        m_TextureHandling_Compute.Dispatch(kernel, 1024, 1024, 1);
        GPU_TextureData.GetData(m_CPU_TextureData);
        // TODO: call data to check dispatch is done!
        m_Texture.GenerateMips();
        // TODO: setup .Release() at app quit
    }
    public void Redo(){
        if(!isInit) return;
        // in Undo - save texture before reset
        // read from saved texture..
    }
#endregion Button Callbacks
}

}