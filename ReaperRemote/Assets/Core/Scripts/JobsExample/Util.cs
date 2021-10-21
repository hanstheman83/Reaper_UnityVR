using Unity.Burst;
using Unity.Mathematics;

public static class Util 
{
    [BurstCompile(CompileSynchronously = true)]
    public static float3 MakePos(float3 max){
        return new float3(
            UnityEngine.Random.Range(-max.x, max.x),
            UnityEngine.Random.Range(-max.y, max.y),
            UnityEngine.Random.Range(-max.z, max.z)
        );
    }
}