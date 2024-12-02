using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class NavMeshSetup : MonoBehaviour
{
    [FormerlySerializedAs("navMeshSurface")] public NavMeshSurface _navMeshSurface;

    // Update is called once per frame
    public void SetupNavMesh()
    {
        Debug.Log("Setup!");
        _navMeshSurface = gameObject.GetComponent<NavMeshSurface>();
        _navMeshSurface.BuildNavMesh();
        
    }
}
