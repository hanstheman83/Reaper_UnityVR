using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Core;
using Core.Interactions;

// COLOR32 //
// Each color component is a byte value with a range from 0 to 255.
// Color32.Lerp

// TODO: Shader to combine background and layers! 
// https://docs.unity3d.com/Manual/OptimizingGraphicsPerformance.html

// Drawing Stroke handling, set data in layers, get data from layers, update FinalTexture
public class DrawingOnTexture : MonoBehaviour
    {
    [SerializeField] int textureHeight = 1024, textureWidth = 1024;
    [SerializeField][Range(0.02f, 2f)] float drawSpeed = 0.02f;
    [SerializeField][Range(0.02f, 0.5f)] float rawTextureRefreshRate = 0.02f;
    [SerializeField][Range(0.02f, 1f)] float renderTextureMipsRefreshRate = 0.03f;
    [SerializeField] InputActionReference spacePressed;
    [SerializeField] GameObject strokePosition;
    [SerializeField] GameObject targetPosition;
    [SerializeField] GameObject depthPosition;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] LayerManager layerManager;
    [SerializeField] Renderer rawTextureRenderer;
    Transform strokePositionTransform, targetPositionTransform, depthPositionTransform;
    StrokePositionController strokePositionController;
    DrawingStickController drawingStickController;
    Coroutine refreshRenderTextureMips;
    Coroutine refreshRawTexture;
    Texture2D texture;
    List<Texture2D> oldTextures;
    ChildTrigger childTrigger;
    Collider childCollider;
    int pos = 0;
    bool isDrawing = false;
    bool[] hasColor;
    
    Transform otherObject;

    Color32[] colors1D;
    // last stroke, in float %
    Vector2 lastStroke = new Vector2(-1f, -1f); // init

    private void Awake() {
        childTrigger = transform.GetComponentInChildren<ChildTrigger>();
        childCollider = childTrigger.GetComponent<Collider>();
        //Material textureMaterial = GetComponentInChildren<Material>();
        childTrigger.childTriggeredEnterEvent += StartStroke;
        childTrigger.childTriggeredExitEvent += StopStroke;
    }
    void Start()
    {
        texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false, true);
        layerManager.texture = texture;
        layerManager.InitializeAllLayers(textureWidth, textureHeight);


        // https://docs.unity3d.com/Manual/LinearRendering-LinearOrGammaWorkflow.html
        // Note: If your Textures are in linear color space, you need to disable sRGB sampling. See documentation on Linear Textures for more information.
        // https://docs.unity3d.com/Manual/LinearRendering-LinearTextures.html

        // Working with Linear textures to avoid sqroot!
        
        // connect texture to material of GameObject this script is attached to
        rawTextureRenderer.material.mainTexture = texture;

        strokePositionTransform = strokePosition.transform;
        targetPositionTransform = targetPosition.transform;
        depthPositionTransform = depthPosition.transform;
        strokePositionController = strokePosition.GetComponent<StrokePositionController>();

        // TODO: auto fill texture
        //renderTexture.GenerateMips(); // -- update this when moving canvas

        //Brush someB = new Brush();

        // init color array
        colors1D = new Color32[25];
        Color c = Colors.Spring;
        for (var i = 0; i < colors1D.Length; i++)
        {
            colors1D[i] = c;
        }
        
    }
    private void OnDestroy() {
        childTrigger.childTriggeredEnterEvent += null;
        childTrigger.childTriggeredExitEvent += null;
    }

   
   // TODO: Depth : No new stroke.. 
   // Trigger : erase hasColor array when changed! 
    void StartStroke(Collider other){
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
        if(refreshRawTexture == null) refreshRawTexture = StartCoroutine(ApplyTexture());
        if(refreshRenderTextureMips == null) refreshRenderTextureMips = StartCoroutine(UpdateRendureTextureMips());
        targetPositionTransform.position = otherObject.position;
        targetPositionTransform.localPosition = new Vector3(targetPositionTransform.localPosition.x, 
                                                            targetPositionTransform.localPosition.y, 0f);
    }
    void StopStroke(Collider other){
        isDrawing = false;
        otherObject = null;
        StopCoroutine(refreshRawTexture);
        refreshRawTexture = null;
        StopCoroutine(refreshRenderTextureMips);
        refreshRenderTextureMips = null;
        drawingStickController.StopResistance();
        drawingStickController = null;
        StartCoroutine(UpdateTexturesOnce()); // need to wait
    }

    // simple delay
    IEnumerator UpdateTexturesOnce(){
        // var data = texture.GetRawTextureData<Color32>(); // copy of pointer
        // var newData = layerManager.CombinedLayers;
        // for (var i = 0; i < data.Length; i++)
        // {
        //     data[i] = newData[i];
        // }
        texture.Apply(); // otherwise applying texture will be delayed untill next stroke!
        yield return new WaitForEndOfFrame();
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
        //var data = texture.GetRawTextureData<Color32>(); // copy of pointer
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
                    //data[currentN] = colors1D[currentColorIndex];
                    layerManager.DrawPixelOnActiveLayer(currentN, colors1D[currentColorIndex]);
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

    // TODO: gradient between points, trigger = value, depth = size
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
        // for network or storing in database : 
        // public byte[] GetRawTextureData(); 
        // use with  Texture2D.LoadRawTextureData
        // https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html
        while(true){
            // var data = texture.GetRawTextureData<Color32>(); // copy of pointer
            // var newData = layerManager.CombinedLayers;
            // for (var i = 0; i < data.Length; i++)
            // {
            //     data[i] = newData[i];
            // }
            texture.Apply();
            yield return new WaitForSeconds(rawTextureRefreshRate);
        }
    }

    private IEnumerator UpdateRendureTextureMips(){
        while(true){
            renderTexture.GenerateMips();
            yield return new WaitForSeconds(renderTextureMipsRefreshRate);
        }
    }
}
