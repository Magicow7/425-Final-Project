using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Navigation : MonoBehaviour
{
    public GameObject playerObject;
    private Transform player;
    private NavMeshAgent agent;
    
    // Start is called before the first frame update
    void Start()
    {
        player = playerObject.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = player.position;
    }
}
