using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

// https://forum.unity.com/threads/converting-a-computebuffer-to-a-nativearray.722651/
// https://docs.unity3d.com/ScriptReference/Rendering.AsyncGPUReadback.RequestIntoNativeArray.html
public static class DrawingData
{
    static bool m_IsInitialized = false;
    // Still can't get data from it directly..  
    // https://forum.unity.com/threads/asyncgpureadback-requestintonativearray-causes-invalidoperationexception-on-nativearray.1011955/
    public static NativeArray<float4> Pixels;
    public static void InitData(int numberOfPixels){
        if(!m_IsInitialized){
            Pixels = new NativeArray<float4>(numberOfPixels, Allocator.Persistent);
            m_IsInitialized = true;
        }
    }

    // TODO: copy data 
}
