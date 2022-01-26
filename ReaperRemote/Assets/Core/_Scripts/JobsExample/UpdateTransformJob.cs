using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct UpdateTransformJob : IJobParallelForTransform
{
    public NativeArray<float3> Positions;

    public static JobHandle Begin(TransformAccessArray array, 
                                NativeArray<float3> positions,
                                JobHandle dependency)
    {
        var job = new UpdateTransformJob(){
            Positions = positions
        };

        return IJobParallelForTransformExtensions.Schedule(job, array, dependency);
    }

    public void Execute(int index, TransformAccess transform)
    {
        transform.position = Positions[index];
    }
}