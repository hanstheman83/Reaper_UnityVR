using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Drawing{

/// <summary>
/// Creates Undo and Redo layers. </br>
/// Cycles through layers on button presses.
/// </summary>
public class UndoRedoBehaviour : MonoBehaviour
{
    [SerializeField] private DrawingOnTexture_GPU m_DrawingOnTexture;
    // TODO: 1st : Save relevant render textures stacks on finished stroke
    // create undo layer with these textures and store in own stack. 
    // 2nd : on Undo btn, get first layer.. 
    private int m_CurrentUndoLevel = -1;
    [SerializeField] private List<TextureHandler> m_TextureHandlers = new List<TextureHandler>();



    // TODO: each render texture has a stack of undos

    // Start is called before the first frame update
    void Start()
    {
        m_DrawingOnTexture.finishedStroke += SetMarkedTextures;
        InitUndoLevelZero();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable() {
        m_DrawingOnTexture.finishedStroke -= SetMarkedTextures;
    }



#region Initializers
    private void InitUndoLevelZero(){
        int numMarkedTextures = 4;
        int[] allMarked = new int[numMarkedTextures];
        for(int i = 0; i < allMarked.Length; i++){
            allMarked[i] = 2;
        }
        SetMarkedTextures(allMarked);
    }
#endregion Initializers


#region Button Callbacks
    public void OnUndoPressed(){
        Debug.Log("Undo pressed, processing...");
        Debug.Log("Current undo level " + m_CurrentUndoLevel);
        // 
        if(m_CurrentUndoLevel == 0){
            Debug.Log("No undoes available!");
        }else{
            // call all TextureHandlers and go to earlier undo level!
            foreach(TextureHandler t in m_TextureHandlers){
                t.Undo(m_CurrentUndoLevel);
            }
            m_CurrentUndoLevel--;
        }
    }
#endregion Button Callbacks

#region Script callbacks
    private void SetMarkedTextures(int[] markedTextures){ // always includes texture 0 - must be starting from there by default ?!
        m_CurrentUndoLevel++;
        Debug.Log("Setting Marked.. Current undo level " + m_CurrentUndoLevel);
        for (var i = 0; i < markedTextures.Length; i++)
        {
            if(markedTextures[i] == 2) {
                Debug.Log("Mark @ " + i );
                m_TextureHandlers[i].SaveState(m_CurrentUndoLevel);
            }
        }
    }
#endregion Script callbacks


}

}