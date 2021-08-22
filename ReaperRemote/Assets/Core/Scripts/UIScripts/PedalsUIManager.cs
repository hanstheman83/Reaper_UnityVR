using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.IO;

namespace Core.UI{

public class PedalsUIManager : MonoBehaviour
{
    [SerializeField] private Toggle autoMutePedal;
    [SerializeField] private Toggle softPedal;
    [SerializeField] private Toggle sustainPedal;
    private MTransmitter mTransmitter;

    #region Unity Methods
    void Awake()
    {
        mTransmitter = FindObjectOfType<MTransmitter>();
        autoMutePedal.isOn = false;
        softPedal.isOn = false;
        sustainPedal.isOn = false;
    }
    private void OnEnable() {

        autoMutePedal.onValueChanged.AddListener( (value) => { OnAutoMutePedalChanged(value); } );
        softPedal.onValueChanged.AddListener( (value) => { OnSoftPedalChanged(value); } );
        sustainPedal.onValueChanged.AddListener( (value) => { OnSustainPedalChanged(value); } );
    }
    private void OnDisable() {

        autoMutePedal.onValueChanged.RemoveListener( (value) => { OnAutoMutePedalChanged(value); } );
        softPedal.onValueChanged.RemoveListener( (value) => { OnSoftPedalChanged(value); } );
        sustainPedal.onValueChanged.RemoveListener( (value) => { OnSustainPedalChanged(value); } );
    }

    #endregion Unity Methods
    
    
    private void OnAutoMutePedalChanged(bool isOn){
        Debug.Log($"Auto mute pedal is on ? {isOn}".Colorize(Color.yellow));        
        //mTransmitter.TransmitMidiCC(0, );
        // change note Off type!
    }
    private void OnSoftPedalChanged(bool isOn){
        Debug.Log($"Soft pedal is on ? {isOn}".Colorize(Color.yellow));        
        mTransmitter.TransmitMidiCC(0, 67, isOn);
    }
    private void OnSustainPedalChanged(bool isOn){
        Debug.Log($"Sustian pedal is on ? {isOn}".Colorize(Color.yellow));        
        mTransmitter.TransmitMidiCC(0, 64, isOn);

    }
    

    
}

}