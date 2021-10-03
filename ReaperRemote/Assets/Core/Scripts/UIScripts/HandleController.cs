using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleController : MonoBehaviour
{
    [SerializeField] float m_MinX = -0.433f;
    [SerializeField] float m_MaxX = 0.441f;

    private DrawingStickController m_DrawingStickController;
    private bool isMoving = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving){
            transform.position = m_DrawingStickController.ColorPickingDrawPoint.position;
            Debug.Log("Moving handle..");
            // lowest x 
            // highest x 
            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, m_MinX, m_MaxX ), 0f, 0f);
            // moved by drawing stick within borders
        }
        // check value has changed - raise event "OnValueChanged"
    }

    public float GetValue(){
        float totalRange = Mathf.Abs(m_MinX) + m_MaxX;
        float normalized = (transform.localPosition.x + Mathf.Abs(m_MinX)) / totalRange;

        return normalized;
    }





    private void OnTriggerEnter(Collider other) {
        Debug.Log("starting moving handle..");
        isMoving = true;
        m_DrawingStickController = other.GetComponentInParent<DrawingStickController>();

    }
    private void OnTriggerExit(Collider other) {
        Debug.Log("stopping moving handle..");
        isMoving = false;        
        m_DrawingStickController = null;
    }
}
