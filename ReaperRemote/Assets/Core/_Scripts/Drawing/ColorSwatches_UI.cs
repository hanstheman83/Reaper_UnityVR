using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Drawing;

namespace Core.UI{

public class ColorSwatches_UI : MonoBehaviour
{
    ColorSwatch m_ActiveColorSwatch = null;
    [SerializeField] SliderController m_Hue_Slider;
    [SerializeField] SliderController m_Saturation_Slider;
    [SerializeField] SliderController m_Value_Slider;

    // Start is called before the first frame update
    void Start()
    {
        m_Hue_Slider.SetValue(1f);
        m_Saturation_Slider.SetValue(1f);
        m_Value_Slider.SetValue(1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable() {
        m_Hue_Slider.onHandleMoved += OnSliderChanged;
        m_Saturation_Slider.onHandleMoved += OnSliderChanged;
        m_Value_Slider.onHandleMoved += OnSliderChanged;
    }
    private void OnDisable() {
        m_Hue_Slider.onHandleMoved -= OnSliderChanged;
        m_Saturation_Slider.onHandleMoved -= OnSliderChanged;
        m_Value_Slider.onHandleMoved -= OnSliderChanged;
    }

    public void SetActiveColorSwatch(ColorSwatch swatch){
        m_ActiveColorSwatch = swatch;
        Color color = swatch.Color;
        // set sliders
        Color.RGBToHSV(color, out float h, out float s, out float v);
        SetSlider(m_Hue_Slider, h);
        SetSlider(m_Saturation_Slider, s);
        SetSlider(m_Value_Slider, v);
    }

    void SetSlider(SliderController slider, float value){
        slider.SetValue(value);
    }

    void ChangeColorInActiveSwatch(Color color){
        if(m_ActiveColorSwatch != null){
            m_ActiveColorSwatch.Color = color;
        }
    }

    void OnSliderChanged(){
        Debug.Log("Slider changed!");
        Debug.Log("Value hue : " + m_Hue_Slider.GetValue());
        Color newColor = Color.HSVToRGB(m_Hue_Slider.GetValue(), 
                                        m_Saturation_Slider.GetValue(), 
                                        m_Value_Slider.GetValue());
        ChangeColorInActiveSwatch(newColor);
    }
}

}