using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.UI;

public class ColorSwatch : MonoBehaviour
{
    [SerializeField] Color m_Color;
    public Color Color {get => m_Color; 
                        set {m_Color = value; 
                            GetComponent<Renderer>().material.color = m_Color;}
                        }
    [SerializeField] ColorSwatches_UI m_ColorSwatches_UI;

    private void Start() {
        GetComponent<Renderer>().material.color = m_Color;
    }

    private void OnTriggerEnter(Collider other) {
        // set draw color
        // set brush color
        DrawingStickController drawingStickController = other.GetComponentInParent<DrawingStickController>();
        drawingStickController.drawingColor = m_Color;
        drawingStickController.stickRenderer.material.color = m_Color;
        m_ColorSwatches_UI.SetActiveColorSwatch(this);
    }
}
