using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Core.Interactions;



// Drawing Stroke handling, set data in layers, get data from layers, update FinalTexture
public class DrawingOnTexture : MonoBehaviour
    {
    [SerializeField] int textureHeight = 1024, textureWidth = 1024;
    [SerializeField][Range(0.02f, 2f)] float drawSpeed = 0.02f;
    [SerializeField][Range(0.02f, 0.5f)] float rawTextureRefreshRate = 0.02f;
    //[SerializeField][Range(0.02f, 0.5f)] float renderTextureMipsRefreshRate = 0.03f;
    [SerializeField] InputActionReference spacePressed;
    [SerializeField] GameObject strokePosition;
    [SerializeField] GameObject targetPosition;
    [SerializeField] GameObject depthPosition;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] LayerManager layerManager;
    Transform strokePositionTransform, targetPositionTransform, depthPositionTransform;
    StrokePositionController strokePositionController;
    DrawingStickController drawingStickController;
    Coroutine refreshRoutine;
    Renderer textureRenderer;
    Texture2D texture;
    List<Texture2D> oldTextures;
    ChildTrigger childTrigger;
    Collider childCollider;
    int pos = 0;
    bool isDrawing = false;
    bool[] hasColor;
    
    Transform otherObject;

    Color[] colors1D;
    // last stroke, in float %
    Vector2 lastStroke = new Vector2(-1f, -1f); // init

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
        // texture = new Texture2D(textureHeight, textureWidth, TextureFormat.RGBA32, false);
        texture = new Texture2D(textureHeight, textureWidth, TextureFormat.RGBA32, false, true);
        
        // connect texture to material of GameObject this script is attached to
        textureRenderer.material.mainTexture = texture;

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        depthPositionTransform = depthPosition.transform;
        strokePositionController = strokePosition.GetComponent<StrokePositionController>();

        // TODO: auto fill texture
        //renderTexture.GenerateMips(); // -- update this when moving canvas

        Brush someB = new Brush();

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

    // Set background color
    // apply texture - 1st : combine layers
    // 2nd : apply
    void StartStroke(Collider other){
        // create new texture - copy old to this
        // without mipmaps but with linear space
        // public Texture2D(int width, int height, TextureFormat textureFormat = TextureFormat.RGBA32, bool mipChain = true, bool linear = false); 


        isDrawing = true;
        var data = texture.GetRawTextureData<Color32>(); // copy of pointer
        hasColor = new bool[data.Length]; // reset per stroke!
        lastStroke = new Vector2(-1f, -1f); // skip a frame
        otherObject = other.transform.Find("DrawPoint");
        drawingStickController = other.GetComponentInParent<DrawingStickController>();
        drawingStickController.StartResistance();
        strokePositionTransform.position = otherObject.position;
        depthPositionTransform.position = otherObject.position;
        strokePositionTransform.localPosition = new Vector3(strokePositionTransform.localPosition.x, 
                                                            strokePositionTransform.localPosition.y, 0f);
        if(refreshRoutine == null) refreshRoutine = StartCoroutine(ApplyTexture());

        targetPositionTransform.position = otherObject.position;
        targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, 
                                                            targetPositionTransform.localPosition.y, 0f);
    }
    void StopStroke(Collider other){
        isDrawing = false;
        otherObject = null;
        StopCoroutine(refreshRoutine);
        refreshRoutine = null;

        // apply with new mipmaps
        // Texture.mipMapBias - pos for more blurry
        // save texture in oldTextures (will fill mem) TODO: check how much - can delete first created
        // create Redo Undo pattern - on ab or yx

        // public void Apply(bool updateMipmaps = true, bool makeNoLongerReadable = false); 
        // So copy into new texture - attach this new t to material/renderer
        // 

        texture.Apply(); // otherwise applying texture will be delayed untill next stroke!

        // debug : Texture2D.loadedMipmapLevel -- put in update, print on change, cache

        drawingStickController.StopResistance();
        drawingStickController = null;
        Invoke("UpdateMipsInRenderTextureOnce", rawTextureRefreshRate/2f); // need to wait
        Debug.Log($"Mips active {renderTexture.useMipMap}".Colorize(Color.cyan));
        Debug.Log("Mip count :".Colorize(Color.magenta) + renderTexture.mipmapCount);
    }

    // simple delay
    void UpdateMipsInRenderTextureOnce(){
        renderTexture.GenerateMips();
    }
    
    // Update is called once per frame
    void Update()
    {
        if(isDrawing)
        {
            UpdateStrokeAndTargetAndDepth();
            UpdateResistance();
            Vector2 canvasCoordinates = CalculateCanvasCoordinates();
            DrawBrushStroke(canvasCoordinates);

            //if dist too long draw line between strokes
            if(lastStroke.x != -1f && lastStroke.y != -1){
                float distanceBetweenBrushHits = Vector2.Distance(canvasCoordinates, lastStroke);
                //Debug.Log("Distance between : ".Colorize(Color.magenta) + distanceBetweenBrushHits);
                float r = .001f; // TODO: match brush radius
                bool isDistance = distanceBetweenBrushHits > r; //
                if(isDistance){
                    Vector2[] inBetweenPoints = CalculateInBetweenPoints(lastStroke, canvasCoordinates);
                    // in between strokes
                    foreach(Vector2 v in inBetweenPoints){
                        DrawBrushStroke(v);
                    }
                }
            }

            lastStroke = canvasCoordinates;
        }
    }// end Update()

    void UpdateResistance(){
        float resistance = Mathf.Clamp( (depthPositionTransform.localPosition.z + .5f), 0f, 1f );
        // if(resistance > .8f) Debug.Log("Resistance : ".Colorize(Color.white) + resistance);
        drawingStickController.HandleResistance(resistance);
    }

    void DrawBrushStroke(Vector2 canvasCoordinates){
        // brush stamp, 5x5 brush
        var data = texture.GetRawTextureData<Color32>(); // copy of pointer
        Vector2Int pixelCoordinates = Vector2Int.RoundToInt(canvasCoordinates * (textureWidth-1)); // TODO: allow non uniform scale of canvas!
            //Debug.Log("hit in pixel coordinates " + pixelCoordinates);

        // offset from brush size 5x5
        int startX = pixelCoordinates.x - 2;
        int startY = pixelCoordinates.y - 2;
        //Debug.Log("Start in pCoord : " + (startX, startY));

        int startN = TransferXYtoN(startX, startY);
        int currentX, currentY, currentN;
        currentX = startX;
        currentY = startY;
        currentN = startN;
        int currentColorIndex = 0; // in colors 1D array

        

        // TODO: Dynamic brush size!
        for (var i = 0; i < 5; i++)
        {
            for (var j = 0; j < 5; j++)
            {
                if(currentN == -1) Debug.Log("current N was -1");
                //if(hasColor[currentN] == true) Debug.Log("pixel already had color!");

                if( (currentN == -1) || (hasColor[currentN] == true) ) { // don't draw, only iterate/update
                    currentX++;
                    currentN = TransferXYtoN(currentX, currentY);
                    currentColorIndex++;
                }else{ // draw and iterate/update
                    data[currentN] = colors1D[currentColorIndex];
                    hasColor[currentN] = true;
                    currentX++;
                    currentN = TransferXYtoN(currentX, currentY);
                    currentColorIndex++;
                }
            }
            currentX = startX;
            currentY++;
        }
    }

    Vector2[] CalculateInBetweenPoints(Vector2 start, Vector2 end){
        //Debug.Log("Calculating in between points".Colorize(Color.white));
        float r = .001f; // TODO: dynamic, brush radius
        float distanceBetweenBrushHits = Vector2.Distance(end, start);
        int numberOfStamps = Mathf.RoundToInt( (distanceBetweenBrushHits / r) );// 
        //Debug.Log("number of stamps :".Colorize(Color.blue) + numberOfStamps);
        float percentageIncrease = 1f/numberOfStamps;
        Vector2 deltaVector = end - start;
        Vector2[] inBetweenPoints = new Vector2[numberOfStamps];

        for (var i = 1; i <= numberOfStamps; i++) // TODO: double check in log
        {
            inBetweenPoints[i - 1] = start + deltaVector * (percentageIncrease * i);
        }
        return inBetweenPoints;
    }

    /// <summary>
    /// Transfer the X and Y coordinates in a 2D pixel grid to a 1D array coordinate.
    /// </summary>
    int TransferXYtoN(int x, int y){
        int n = x + (textureWidth * y);
        if(n >= textureWidth * textureHeight) return -1; 
        return n;
    }

    void UpdateStrokeAndTargetAndDepth(){
        targetPositionTransform.position = otherObject.position;
        depthPositionTransform.position = otherObject.position;
        targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, 
                                                            targetPositionTransform.localPosition.y, 0f);
        strokePositionTransform.localPosition = Vector2.MoveTowards(strokePositionTransform.localPosition, 
                                                                    targetPositionTransform.localPosition, 
                                                                    drawSpeed * Time.deltaTime);
    }

    Vector2 CalculateCanvasCoordinates(){
        return new Vector2(Mathf.Clamp((strokePositionTransform.localPosition.x + .5f), 0, 1), 
                           Mathf.Clamp((strokePositionTransform.localPosition.y + .5f), 0, 1));
    }

    private IEnumerator ApplyTexture()
    {
        while(true){
            texture.Apply();
            yield return new WaitForEndOfFrame();
            renderTexture.GenerateMips();
            yield return new WaitForSeconds(rawTextureRefreshRate);
 

        }
    }
}
