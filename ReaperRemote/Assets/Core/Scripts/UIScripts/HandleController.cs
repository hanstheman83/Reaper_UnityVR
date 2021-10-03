using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleController : MonoBehaviour
{
    private DrawingStickController m_DrawingStickController;
    private bool isMoved = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoved){
            transform.position = m_DrawingStickController.ColorPickingDrawPoint.position;
            Debug.Log("Moving handle..");
            // lowest x 
            // highest x 
            transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -0.433f, 0.441f), 0f, 0f);
            // moved by drawing stick within borders


        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log("starting moving handle..");
        isMoved = true;
        m_DrawingStickController = other.GetComponentInParent<DrawingStickController>();

    }
    private void OnTriggerExit(Collider other) {
        Debug.Log("stopping moving handle..");
        isMoved = false;        
        m_DrawingStickController = null;
    }
}
