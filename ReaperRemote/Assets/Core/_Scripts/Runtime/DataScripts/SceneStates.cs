using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Controls;

namespace Core.SceneManagement{
internal static class SceneStates
{
    public static ControllerHand PencilHand = ControllerHand.None;
    /// <summary>
    /// Main Controller Hand has teleport and snap turn movement. 
    /// </summary>
    public static ControllerHand MainControllerHand = ControllerHand.Right;

}

}