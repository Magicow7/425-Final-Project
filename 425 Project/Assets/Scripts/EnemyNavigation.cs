using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavigation : MonoBehaviour
{
    public GameObject playerObject;
    private Transform player;
    private NavMeshAgent agent;
    private bool linkTraversing = false;
    
    void Start()
    {
        //this wasn't getting updated so I added this -Silas
        player = DungeonGenerator.instance.player;
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;

        StartCoroutine(CheckHasPath());
    }

    void Update()
    {
        if(agent.isOnOffMeshLink){
            StartCoroutine(TraverseOffMeshLink());
            linkTraversing = true;
        }
        if(!linkTraversing){
            agent.destination = player.position;
        }
            
        
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

    //this is spagetti code to put a band-aid over the bug with nav mesh links and vertical rooms.
    private IEnumerator TraverseOffMeshLink(){
        Vector3 endPos = agent.currentOffMeshLinkData.endPos + Vector3.up * agent.baseOffset/2.25f;
        Vector3 savedVelocity = agent.velocity;
        agent.enabled = false;
        while(Vector3.Distance(agent.transform.position, endPos) > 0.5f){
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
            yield return null;
        }
        agent.enabled = true;
        agent.CompleteOffMeshLink();
        agent.velocity = savedVelocity;
        linkTraversing = false;
    }

    private void Respawn()
    {
        Debug.Log("respawning enemy");
        Vector3 randomValidSpawnPoint = DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)];
        randomValidSpawnPoint.y += 0.5f;
        agent.Warp(randomValidSpawnPoint);
    }
}
