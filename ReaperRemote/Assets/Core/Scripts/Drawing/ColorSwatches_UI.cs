using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSwatches_UI : MonoBehaviour
{
    private ColorSwatch m_ActiveColorSwatch;
    // [SerializeField] List<ColorSwatch> m_ColorSwatches;
    // [SerializeField]private Slider m_Hue_Slider;
    // [SerializeField]private Slider m_Saturation_Slider;
    // [SerializeField]private Slider m_Value_Slider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable() {
        // m_Hue_Slider.onValueChanged.AddListener(delegate { OnSliderChanged(); }) ;
        // m_Saturation_Slider.onValueChanged.AddListener(delegate { OnSliderChanged(); }) ;
        // m_Value_Slider.onValueChanged.AddListener(delegate { OnSliderChanged(); }) ;
    }
    private void OnDisable() {
        
    }

    public void SetActiveColorSwatch(ColorSwatch swatch){
        m_ActiveColorSwatch = swatch;
        Color color = swatch.Color;
        // convert and to sliders

    }
    public void OnSliderChanged(){
        Debug.Log("Slider changed!");
    }
}
