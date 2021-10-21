using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;

[BurstCompile(CompileSynchronously = true)]
public struct MoveStarJob : IJobParallelFor
{
    const int BATCH_SIZE = 10;
    [ReadOnly]public float DeltaTime; // [Readonly] - message to compiler, optimization

    public NativeArray<float3> Positions;

    public static JobHandle Begin(NativeArray<float3> positions){
        var job = new MoveStarJob(){
            Positions = positions,
            DeltaTime = Time.deltaTime
        };

        // in each thread it will go through that batch size in a little short loop, 
        // - how many iterations per thread. 
        return IJobParallelForExtensions.Schedule(job, positions.Length, BATCH_SIZE);
    }

    public void Execute(int index){
        float3 delta = new float3(DeltaTime, DeltaTime, DeltaTime);
        Positions[index] = Positions[index] + delta;
    }
}