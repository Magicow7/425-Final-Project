using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    public GameObject playerObject;
    private Transform player;
    private NavMeshAgent agent;
    
    void Start()
    {
        //this wasn't getting updated so I added this -Silas
        player = DungeonGenerator.instance.player;
        agent = GetComponent<NavMeshAgent>();

        StartCoroutine(CheckHasPath());
    }

    void Update()
    {
        agent.destination = player.position;
    }

    private IEnumerator CheckHasPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            if (!agent.hasPath)
            {
                Respawn();
            }
        }
    }

    private void Respawn()
    {
        Vector3 randomValidSpawnPoint = DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)];
        randomValidSpawnPoint.y += 0.5f;
        agent.Warp(randomValidSpawnPoint);
    }
}
