using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Core.Interactions;

public class DrawingOnTexture : MonoBehaviour
{
    [SerializeField][Range(0.02f, 0.4f)] float drawSpeed = 0.02f;
    [SerializeField] InputActionReference spacePressed;
    [SerializeField] GameObject strokePosition;
    [SerializeField] GameObject targetPosition;
    Transform strokePositionTransform, targetPositionTransform;
    StrokePositionController strokePositionController;

    Renderer textureRenderer;
    Texture2D texture;
    ChildTrigger childTrigger;
    Collider childCollider;
    int pos = 0;
    bool isDrawing = false;
    Transform otherObject;

    Color[] colors1D;
    

    private void Awake() {
        textureRenderer = GetComponentInChildren<Renderer>();
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        //Material textureMaterial = GetComponentInChildren<Material>();
        childTrigger.childTriggeredEnterEvent += StartStroke;
        childTrigger.childTriggeredExitEvent += StopStroke;
    }
    void Start()
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        // connect texture to material of GameObject this script is attached to
        textureRenderer.material.mainTexture = texture;

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        strokePositionController = strokePosition.GetComponent<StrokePositionController>();




        // init color array
        colors1D = new Color[25];
        Color c = new Vector4(0, 0, 1, 0);
        for (var i = 0; i < colors1D.Length; i++)
        {
            colors1D[i] = c;
        }
        
    }
    private void OnDestroy() {
        childTrigger.childTriggeredEnterEvent += null;
        childTrigger.childTriggeredExitEvent += null;
        
    }

    void StartStroke(Collider other){
        isDrawing = true;
        otherObject = other.transform;
        strokePositionTransform.position = otherObject.position;
        strokePositionTransform.localPosition = new Vector3(strokePositionTransform.localPosition.x, strokePositionTransform.localPosition.y, 0f);
    }
    void StopStroke(Collider other){
        isDrawing = false;
        otherObject = null;
    }

    void ProcessInput(Collider other){
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(.01f,.01f,.01f);
        Vector3 pos = childCollider.ClosestPoint(other.transform.position);
        sphere.transform.position = pos;
        // Note : affected by scale, so always -.5 to .5
        Vector3 localPos = transform.InverseTransformPoint(pos);
        Debug.Log("Local Pos: " + localPos);
        Vector2 canvasCoordinates = new Vector2(Mathf.Clamp((localPos.x + .5f), 0, 1), Mathf.Clamp((localPos.y + .5f), 0, 1));
        // in integers, 0 to 511
        Debug.Log("Canvas Coord: " + canvasCoordinates);
        Vector2Int pixelCoordinates = Vector2Int.RoundToInt(canvasCoordinates * 511f);
        Debug.Log("Pixel Vector ".Colorize(Color.magenta) + pixelCoordinates);

        texture.SetPixels(pixelCoordinates.x, pixelCoordinates.y, 4, 4, colors1D, 0); 
        // This function is an extended version of SetPixels above; it does not modify the whole mip level but modifies only blockWidth by blockHeight region starting at x,y. The colors array must be blockWidth*blockHeight size, and the modified block must fit into the used mip level.
        // https://docs.unity3d.com/ScriptReference/Color.html


        texture.Apply(); // TODO: keep at low frame rate



        // 1.  calculate array 32color, https://docs.unity3d.com/ScriptReference/Texture2D.SetPixels32.html
        // 1d array x * y in size, indexes are 0 to (x * y) -1 
        // brush tip : round = calc pi from point, center is nearest pixel.
        // soft : blurry edges
        // Additive : how are colours mixed together ?? 
        // Pressure - how far into canvas OR trigger
        // Pressure : add tactile feedback

        // https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToMatrix/index.htm
        
        // for undo function save a texture per stroke!

        // Over background texture ?? Layered textures ?

        // 2 hand usage ?? 

        // Save Math strokes - straight etc...
        // Bezier curves etc... experiment..
    
        // Import image runtime : https://gyanendushekhar.com/2017/07/08/load-image-runtime-unity/
        // https://docs.unity3d.com/Manual/TextureStreaming.html
    }
    
    // Update is called once per frame
    void Update()
    {
        if(isDrawing){
            // turn on move towards mode on block ()
            targetPositionTransform.position = otherObject.position;
            targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, targetPositionTransform.localPosition.y, 0f);
            strokePositionTransform.localPosition = Vector2.MoveTowards(strokePositionTransform.localPosition, targetPositionTransform.localPosition, drawSpeed * Time.deltaTime);



            // Vector3 pos = childCollider.ClosestPoint(otherObject.position);
            //Vector3 localPos = transform.InverseTransformPoint(pos);


            // float depth = localPos.z;
            // Debug.Log("Depth : ".Colorize(Color.red) + depth);
            // outher bounds : -0.005, inner bounds : ~= 0 - clamp..
            Vector2 canvasCoordinates = new Vector2(Mathf.Clamp((strokePositionTransform.localPosition.x + .5f), 0, 1), Mathf.Clamp((strokePositionTransform.localPosition.y + .5f), 0, 1));
            Vector2Int pixelCoordinates = Vector2Int.RoundToInt(canvasCoordinates * 511f);
            Debug.Log("hit in pixel coordinates " + pixelCoordinates);
            var data = texture.GetRawTextureData<Color32>();
            Debug.Log("Length of raw array : ".Colorize(Color.magenta) + data.Length);
            Debug.Log("512 x 512 = " + 512*512);



            // texture.SetPixels(pixelCoordinates.x, pixelCoordinates.y, 4, 4, colors, 0); 
            // for 512x512
            // https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html
            // find median of colors : 
            // ushort : Holds unsigned 16-bit (2-byte) integers ranging in value from 0 through 65,535.
            int startX = pixelCoordinates.x - 2; // TODO: check out of bounds!!
            int startY = pixelCoordinates.y - 2;
            Debug.Log("Start in pCoord : " + (startX, startY));
            int startN = TransferXYtoN(startX, startY);
            int currentX, currentY, currentN;
            currentX = startX;
            currentY = startY;
            currentN = startN;
            int currentColorIndex = 0; // in colors 1D array

            for (var i = 0; i < 5; i++)
            {
                for (var j = 0; j < 5; j++)
                {
                    Debug.Log("Current Color index : " + currentColorIndex);
                    Debug.Log(("current N : " + currentN));
                    Debug.Log("Current X and Y : " + (currentX, currentY));
                    data[currentN] = colors1D[currentColorIndex];                    
                    currentX++; // check bounds

                    currentN = TransferXYtoN(currentX, currentY);
                    currentColorIndex++;
                }
                currentX = startX;
                currentY++; // check bounds
            }
            texture.Apply();
        }
        
    }// end Update()



    /// <summary>
    /// Transfer the X and Y coordinates in a 2D pixel grid to a 1D array coordinate.
    /// </summary>
    int TransferXYtoN(int x, int y){
        return x + (512 * y);
    }
}
