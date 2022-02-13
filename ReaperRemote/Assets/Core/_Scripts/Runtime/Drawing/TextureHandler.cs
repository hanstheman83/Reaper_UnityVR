using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;

namespace Core.Drawing{

public class TextureHandler : MonoBehaviour
{
    [SerializeField] int m_RenderTextureIndex = 0;
    [SerializeField] DrawingOnTexture_GPU m_DrawingOnTexture;
    //private RenderTexture m_Texture;
    private Dictionary<int, Color[]> m_Undoes = new Dictionary<int, Color[]>();
    private Color[] m_CPU_TextureData;
    private ComputeBuffer GPU_TextureData;
    private bool isInit = false;
    [SerializeField] private ComputeShader m_TextureHandling_Compute;
    List<RenderTexture> m_Textures = new List<RenderTexture>();

    // Start is called before the first frame update
    void Start()
    {
        m_TextureHandling_Compute = Instantiate(m_TextureHandling_Compute);
        SetupDataBuffers();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable() {
        GPU_TextureData?.Release();
    }

    private void SetupDataBuffers()
    {
        SetupCPU_Buffer();
    }

    private void SetupCPU_Buffer()
    {
        m_CPU_TextureData = new Color[1024 * 1024]; // TODO: no magic #s
        // for (var i = 0; i < m_CPU_TextureData.Length; i++)
        // {
        //     m_CPU_TextureData[i] = Color.green;
        // }
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
        //m_Texture = m_DrawingOnTexture.renderTexture_00;
        m_TextureHandling_Compute.SetTexture(kernel1, "_RenderTexture", m_Textures[m_RenderTextureIndex]);
        m_TextureHandling_Compute.SetTexture(kernel2, "_RenderTexture", m_Textures[m_RenderTextureIndex]);
#if !UNITY_EDITOR
        // drawingOnTexture.renderTexture_06; // has access to !UNITY_EDITOR 
#endif        
    }

#region Event Callbacks
    public void SaveState(int undoIndex){
        if(!isInit){
            MakeTextureList();
            HookTextureToShader();
            isInit = true;
        }
        SetupCPU_Buffer();
        Debug.Log("Saving state..");
        int kernel = m_TextureHandling_Compute.FindKernel("SaveUndoPoint");
        //m_TextureHandling_Compute.SetTexture(kernel, "_RenderTexture", m_Textures[m_RenderTextureIndex]);
        GPU_TextureData = new ComputeBuffer(m_CPU_TextureData.Length, sizeof(float) * 4);

        GPU_TextureData.SetData(m_CPU_TextureData);
        m_TextureHandling_Compute.SetBuffer(kernel, "_TextureData", GPU_TextureData);
        m_TextureHandling_Compute.Dispatch(kernel, 1024, 1024, 1);
        GPU_TextureData.GetData(m_CPU_TextureData); //
        m_Undoes.Add(undoIndex, m_CPU_TextureData); // 
        //
    
        GPU_TextureData.Release();
    }
#endregion Event Callbacks

#region Button Callbacks
    public void Undo(int undoIndex){
        if(!isInit) return;
        Debug.Log("Undo called.....".Colorize(Color.cyan));
        // draw on texture 1 color..
        // dispatch
        int kernel = m_TextureHandling_Compute.FindKernel("Undo");
        // TODO: add pre compile defs..
        
        //SetupCPU_Buffer();
        
        if(m_Undoes.ContainsKey(undoIndex)){
            m_CPU_TextureData = m_Undoes[undoIndex]; 
            if(m_Undoes.Remove(undoIndex)) Debug.Log("undo" + undoIndex + " removed");;
        }else{
            Debug.Log("No undo for texture " + m_RenderTextureIndex);
            return;
        }
        
        m_TextureHandling_Compute.SetTexture(kernel, "_RenderTexture", m_Textures[m_RenderTextureIndex]);
        GPU_TextureData = new ComputeBuffer(m_CPU_TextureData.Length, sizeof(float) * 4);
        GPU_TextureData.SetData(m_CPU_TextureData);
        m_TextureHandling_Compute.SetBuffer(kernel, "_TextureData", GPU_TextureData);
        m_TextureHandling_Compute.Dispatch(kernel, 1024, 1024, 1);
        GPU_TextureData.GetData(m_CPU_TextureData);
        GPU_TextureData.Release();
        // TODO: call data to check dispatch is done!
        m_Textures[m_RenderTextureIndex].GenerateMips();
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