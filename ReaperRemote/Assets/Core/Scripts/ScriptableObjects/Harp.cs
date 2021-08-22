using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Instruments{

[CreateAssetMenu(fileName = "Harp", menuName = "Instruments/harp")]
public class Harp : Instrument
{
    public Material stringOff, stringOn;
    public int numberOfStrings;
    public int firstOctave; // A0 = midi note 21
    public RootNote rootNote;
    public Scale scale;
    public float distanceBetweenStrings, stringWidth, stringLength;
}

}