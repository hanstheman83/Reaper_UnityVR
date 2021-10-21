using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;

[AddComponentMenu("Demo / Star Manager")]
public class StarManager : MonoBehaviour
{
    [SerializeField] GameObject m_Prefab = null;
    [SerializeField] int m_NumberOfObjects = 20000;
    [SerializeField] float3 Range = new float3(50, 50, 50);
    // The Burst compiler is tuned to using the .Mathematics library - float3 instead of Vector3
    // using Value types - copying instead of passing ref (to the Heap)
    NativeArray<float3> m_Positions; // native array use Blit functions to copy really really efficiently
    TransformAccessArray m_TransformArray;
    JobHandle m_MoveJob;
    JobHandle m_TransformJob;
    bool m_IsInitialized = false;
    // Transform[] m_Transforms;



    // Start is called before the first frame update
    void Start()
    {

        if(m_Prefab == null) { Debug.LogWarning("Prefab not set!");}
        else {
            Allocate();
            SetAllPositions();
            CreateGameObjects();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(m_IsInitialized && m_MoveJob.IsCompleted && m_TransformJob.IsCompleted){ // positions will be adapted in the background, seperating the main thread from the new positions
            // if the job has been done lets use those positions - if not let us leave them alone.. 
            JobHandle.CompleteAll(ref m_MoveJob, ref m_TransformJob); // releasing the lock to the job handle - unlocks the thread so that you can add new data. Without it no access to the native array. 
            // m_MoveJob.Complete(); 
            // UpdateTransforms(); 
            m_MoveJob = MoveStarJob.Begin(m_Positions);
            m_TransformJob = UpdateTransformJob.Begin(m_TransformArray, m_Positions, m_MoveJob);

        }
    }

    private void OnDestroy() {
        JobHandle.ScheduleBatchedJobs(); // force all jobs to run - some might be lingering.. 
        // m_MoveJob.Complete();
        JobHandle.CompleteAll(ref m_MoveJob, ref m_TransformJob); // releasing the lock to the job handle - unlocks the thread so that you can add new data. Without it no access to the native array. 
        m_Positions.Dispose();
        m_TransformArray.Dispose();
    }

    // void UpdateTransforms(){
    //     for (var i = 0; i < m_Transforms.Length; i++)
    //     {
    //         m_Transforms[i].position = m_Positions[i]; // Automatic casting between float3 and Vector3
    //     }
    // }

    void Allocate(){
        m_Positions = new NativeArray<float3>(m_NumberOfObjects, Allocator.Persistent);
        // m_Transforms = new Transform[m_NumberOfObjects]; 
        m_TransformArray = new TransformAccessArray(m_NumberOfObjects); // native transform array
    }

    void SetAllPositions() {
        for (var i = 0; i < m_Positions.Length; i++)
        {
            m_Positions[i] = Util.MakePos(Range);
        }
        m_IsInitialized = true;
    }

    void CreateGameObjects(){
        for (var i = 0; i < m_Positions.Length; i++)
        {
            var gameObject = GameObject.Instantiate(m_Prefab, m_Positions[i], Quaternion.identity);
            // m_Transforms[i] = gameObject.GetComponent<Transform>();
            var thingy = gameObject.GetComponent<Transform>();
            m_TransformArray.Add(thingy);
        }

    }
}
