using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Drawing;

namespace Core.UI{

public class SliderController : MonoBehaviour
{
    [SerializeField] float m_MinX = -0.433f;
    [SerializeField] float m_MaxX = 0.441f;
    //float m_MinimumDelta = 0.001f;
    [SerializeField] float m_UpdateTime = .1f;
    [SerializeField] ChildTrigger m_ChildTrigger;

    private DrawingStickController m_DrawingStickController;
    private bool isMoving = false;
    private Coroutine m_Update;
    public delegate void OnHandleMoved();
    public event OnHandleMoved onHandleMoved;
    [SerializeField] Transform m_Handle;

#region Unity Methods
    private void OnEnable() {
        m_ChildTrigger.childTriggeredEnterEvent += ChildTriggeredEnter;
        m_ChildTrigger.childTriggeredExitEvent += ChildTriggeredExit;
    }
    private void OnDisable() {
        m_ChildTrigger.childTriggeredEnterEvent -= ChildTriggeredEnter;
        m_ChildTrigger.childTriggeredExitEvent -= ChildTriggeredExit;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving){
            m_Handle.position = m_DrawingStickController.ColorPickingDrawPoint.position;
            //Debug.Log("Moving handle..");
            // lowest x 
            // highest x 
            m_Handle.localPosition = new Vector3(Mathf.Clamp(m_Handle.localPosition.x, m_MinX, m_MaxX ), 0f, 0f);
            // moved by drawing stick within borders
        }
        // check value has changed - raise event "OnValueChanged"
    }

    void ChildTriggeredEnter(Collider other){
        Debug.Log("starting moving handle..");
        isMoving = true;
        m_DrawingStickController = other.GetComponentInParent<DrawingStickController>();
        m_Update = StartCoroutine(UpdateState());
    }

    void ChildTriggeredExit(Collider other){
        Debug.Log("stopping moving handle..");
        isMoving = false;        
        m_DrawingStickController = null;
        if(m_Update != null){
            StopCoroutine(m_Update);
            m_Update = null;
        }

    }
#endregion Unity Methods



    private IEnumerator UpdateState(){
        while(true){
            Debug.Log("updating state..");
            onHandleMoved?.Invoke();
            yield return new WaitForSeconds(m_UpdateTime);
        }
    }

    public float GetValue(){
        float totalRange = Mathf.Abs(m_MinX) + m_MaxX;
        float normalized = (m_Handle.localPosition.x + Mathf.Abs(m_MinX)) / totalRange;

        return normalized;
    }
    public void SetValue(float value){
        if(value < 0f || value > 1f) Debug.LogError("Value should be normalized!");
        float totalRange = Mathf.Abs(m_MinX) + m_MaxX;
        float newValue = value * totalRange;
        newValue += m_MinX;
        //update handle transform
        m_Handle.localPosition = new Vector3(newValue, 0f, 0f);
    }


}

}