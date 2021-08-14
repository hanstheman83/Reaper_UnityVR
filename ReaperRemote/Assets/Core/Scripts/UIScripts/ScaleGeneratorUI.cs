using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using TMPro;
using Core.Instruments;

namespace Core.UI{

public class ScaleGeneratorUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown rootDropdown, scaleDropdown, firstOctaveDropdown; // callbacks from UI setup dynamically to avoid circular dependency
    [SerializeField] private InstrumentGenerator instrumentGenerator;
    
    public void OnValueChange(){
        // get data
        //instrumentGenerator.ChangeScale()
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        // read from Data and generate options list in dropdown
        // str = string.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

}