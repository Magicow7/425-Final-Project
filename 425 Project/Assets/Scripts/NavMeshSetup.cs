using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class NavMeshSetup : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    // Update is called once per frame
    public void SetupNavMesh()
    {
        navMeshSurface = gameObject.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
        
    }
}
