using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;

public class DrawingOnTexture : MonoBehaviour
{
    [SerializeField] InputActionReference spacePressed;

    Renderer textureRenderer;
    Texture2D texture;
    int pos = 0;

    private void Awake() {
        textureRenderer = GetComponentInChildren<Renderer>();
        //Material textureMaterial = GetComponentInChildren<Material>();
        spacePressed.action.performed += ProcessInput;
    }
    void Start()
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(512, 512, TextureFormat.ARGB32, false);
        
        // set the pixel values
        texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
        texture.SetPixel(1, 0, Color.clear);
        texture.SetPixel(0, 1, Color.white);
        texture.SetPixel(1, 1, Color.black);
    
        // Apply all SetPixel calls
        texture.Apply();
        
        // connect texture to material of GameObject this script is attached to
        textureRenderer.material.mainTexture = texture;
    }

    void ProcessInput(InputAction.CallbackContext obj){
        Debug.Log("Space ".Colorize(Color.magenta) + obj.ReadValue<float>());
            texture.SetPixel(pos, pos, Color.black);
            texture.Apply();

            if(pos < 255) pos++;
            else pos = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKey(KeyCode.Space )){
        //     texture.SetPixel(pos, pos, Color.black);
        //     texture.Apply();



        //     if(pos < 255) pos++;
        //     else pos = 0;
        // }


    }
}
