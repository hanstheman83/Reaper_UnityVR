using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Data;

[CreateAssetMenu(fileName = "Harp", menuName = "Instruments/harp")]


public class Harp : Instrument
{
    public int numStrings;
    public int startOctave; // A0 = midi note 21
    public RootNote rootNote;
    public Scale scale;
    public float distanceBetweenStrings, stringWidth, stringLength;
}
