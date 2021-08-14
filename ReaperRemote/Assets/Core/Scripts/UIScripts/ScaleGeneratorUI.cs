using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.UI;
using TMPro;
using Core.Instruments;
using System.Linq;

namespace Core.UI{

public class ScaleGeneratorUI : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown rootDropdown, scaleDropdown, firstOctaveDropdown; // callbacks from UI setup dynamically to avoid circular dependency
    [SerializeField] private InstrumentGenerator instrumentGenerator;
    private int[] firstOctaveArray;
    
    private void Awake()
        {
            rootDropdown.onValueChanged.AddListener(delegate { OnValueChange(); });
            scaleDropdown.onValueChanged.AddListener(delegate { OnValueChange(); });
            firstOctaveDropdown.onValueChanged.AddListener(delegate { OnValueChange(); });
        }

    // Start is called before the first frame update
    void Start()
    {
        InitRootNotesDropdown();
        InitScalesDropdown();
        InitFirstOctaveDropdown();

        void InitFirstOctaveDropdown(){
            firstOctaveArray = new int[]{1,2,3,4,5};
            var tmpList = new List<TMP_Dropdown.OptionData>();
            foreach(int i in firstOctaveArray){
                tmpList.Add( new TMP_Dropdown.OptionData(i.ToString()) );
            }
            firstOctaveDropdown.options = tmpList;
        }



        void InitScalesDropdown(){
            var scalesOptions = new List<TMP_Dropdown.OptionData>();
            string[] scalesList = System.Enum.GetNames (typeof(Scale));
            for(int i = 0; i < scalesList.Length; i++){
                scalesOptions.Add( new TMP_Dropdown.OptionData(
                    string.Concat(scalesList[i].Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '))); // add whitespace between upper case words
            }
            scaleDropdown.options = scalesOptions;
        }
        
        // can also add 1 at a time.. eg Maindropdown.options.Add (new Dropdown.OptionData() {text=c});
        void InitRootNotesDropdown(){
            // list root notes
            var  rootNotesOptions = new List<TMP_Dropdown.OptionData>();
            string[] rootsList = System.Enum.GetNames (typeof(RootNote));
            for(int i = 0; i < rootsList.Length; i++){
                // Debug.Log (rootsList[i]);
                rootNotesOptions.Add( new TMP_Dropdown.OptionData(rootsList[i]) );
            }
            rootDropdown.options = rootNotesOptions;
        }
    }

    #region UI callbacks (set on runtime)
    public void OnValueChange(){
        Debug.Log("on value change");
        // get data
        //rootDropdown.value -- int index
        //rootDropdown.itemText
        // rootDropdown.options

        // Debug.Log("major "+Scale.Major);
        //     int someInt = 2;
        //     Debug.Log("major "+(Scale)someInt);
        
        
        instrumentGenerator.ChangeScale((RootNote)rootDropdown.value, (Scale)scaleDropdown.value, firstOctaveArray[firstOctaveDropdown.value]);
    
    
    }
    #endregion UI callbacks (set on runtime)
}

}