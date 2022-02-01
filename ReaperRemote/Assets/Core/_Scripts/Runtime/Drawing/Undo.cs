using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Drawing{

/// <summary>
/// Drawing class will store Undo data stack in this script..
/// </summary>
public class Undo : MonoBehaviour
{
    Stack<UndoLayer> undoStack = new Stack<UndoLayer>();

    // pull from drawing class - callbacks when done drawing a stroke!

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Initialize(){
        // called from drawing script,
        // 
        undoStack.Push(new UndoLayer(1024, 20));
    }

    // At startup - store UndoLayer

}

}