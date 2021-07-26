using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Harp", menuName = "Instruments/harp")]


public class Harp : Instrument
{
    public int firstNote, lastNote;
    public float distanceBetweenStrings, stringWidth, stringLength;
}
