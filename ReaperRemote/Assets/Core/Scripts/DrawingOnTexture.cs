using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Core.Interactions;

public class DrawingOnTexture : MonoBehaviour
{
    [SerializeField] InputActionReference spacePressed;

    Renderer textureRenderer;
    Texture2D texture;
    ChildTrigger childTrigger;
    Collider childCollider;
    int pos = 0;

    private void Awake() {
        textureRenderer = GetComponentInChildren<Renderer>();
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        //Material textureMaterial = GetComponentInChildren<Material>();
        childTrigger.childTriggeredEnterEvent += ProcessInput;
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

    void ProcessInput(Collider other){
        Debug.Log("Coordinates ".Colorize(Color.magenta) + childCollider.ClosestPointOnBounds(other.transform.position));
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(.01f,.01f,.01f);
        Vector3 pos = childCollider.ClosestPointOnBounds(other.transform.position);
        sphere.transform.position = pos;
        // Note affected by scale, so always -.5 to .5
        Vector3 localPos = transform.InverseTransformPoint(pos);
        Debug.Log("Local Pos: " + localPos);

        // 
        Vector2 canvasCoordinates = new Vector2(localPos.x + .5f, localPos.y + .5f);
        // in integers, 0 to 511
        //Vector2 pixelCoordinates =



        // to local coordinates, then to 0 to 1 from bl to tr
        // transfer function : 

        // public void SetPixels(int x, int y, int blockWidth, int blockHeight, Color[] colors, int miplevel = 0); 
        // This function is an extended version of SetPixels above; it does not modify the whole mip level but modifies only blockWidth by blockHeight region starting at x,y. The colors array must be blockWidth*blockHeight size, and the modified block must fit into the used mip level.



        // texture.Apply(); // TODO: keep at low frame rate



        // 1.  calculate array 32color, https://docs.unity3d.com/ScriptReference/Texture2D.SetPixels32.html
        // 1d array x * y in size, indexes are 0 to (x * y) -1 
        // brush tip : round = calc pi from point, center is nearest pixel.
        // soft : blurry edges
        // Additive : how are colours mixed together ?? 
        // Pressure - how far into canvas OR trigger
        // Pressure : add tactile feedback

        // find world space contact point : https://answers.unity.com/questions/42933/how-to-find-the-point-of-contact-with-the-function.html

        // get contact point with raytrace : https://answers.unity.com/questions/581977/how-to-get-collision-point-when-using-ontriggerent.html

        // get local coordinate space, size of quad 


        // https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        
        
        
        
        
        
        
        
        
        
        // for undo function save a texture per stroke!

        // Over background texture ?? Layered textures ?

        // 2 hand usage ?? 

        // Save Math strokes - straight etc...
        // Bezier curves etc... experiment..

        // for scaling artifacts check mipmap : https://docs.unity3d.com/Manual/TextureStreaming.html
        // https://bgolus.medium.com/sharper-mipmapping-using-shader-based-supersampling-ed7aadb47bec
        // Unity Mipmap texture streaming API https://docs.unity3d.com/Manual/TextureStreaming-API.html
    
        // Import image runtime : https://gyanendushekhar.com/2017/07/08/load-image-runtime-unity/
        // https://docs.unity3d.com/Manual/TextureStreaming.html
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
