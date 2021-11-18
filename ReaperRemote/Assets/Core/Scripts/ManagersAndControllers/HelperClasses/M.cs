using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Controls{
public static class M
{
    public static void SpecifyControllerHand(){
        Debug.LogError("You need to specify a controller hand!");
    }
    public static void NoItemRemoved(){
        Debug.LogError("No item removed!");
    }
    public static void ItemRemoved(){
        Debug.Log("Item removed");
    }
}

}