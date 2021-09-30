using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSwatch : MonoBehaviour
{
    [SerializeField] Color color;

    private void Start() {
        GetComponent<Renderer>().material.color = color;
    }

    private void OnTriggerEnter(Collider other) {
        // set draw color
        // set brush color
        DrawingStickController drawingStickController = other.GetComponentInParent<DrawingStickController>();
        drawingStickController.drawingColor = color;
        drawingStickController.stickRenderer.material.color = color;
    }
}
