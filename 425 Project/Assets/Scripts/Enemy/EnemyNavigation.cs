using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyNavigation : MonoBehaviour
{
    [FormerlySerializedAs("player"), SerializeField]
    private Transform _player;

    [SerializeField] private LayerMask _groundLayer;

    private NavMeshAgent _agent;
    private bool _linkTraversing;

    private void Start()
    {
        //this wasn't getting updated so I added this -Silas
        _player = DungeonGenerator.Instance?._player;
        _agent = GetComponent<NavMeshAgent>();
        _agent.autoTraverseOffMeshLink = false;

        StartCoroutine(CheckHasPath());
    }

    private void Update()
    {
        if (_agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseOffMeshLink());
            _linkTraversing = true;
        }

        if (_linkTraversing)
        {
            return;
        }

        try
        {
            // Code that might throw an exception
            if (Physics.Raycast(_player.position, Vector3.down, out var hit, _groundLayer))
            {
                _agent.destination = hit.point;
            }
        }
        catch (Exception e)
        {
            // Code that handles the exception
            Debug.LogError("failed to set agent destination, respawning enemy");
            Respawn();
        }
    }

    private IEnumerator CheckHasPath()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            if (!_agent.hasPath)
            {
                Respawn();
            }
        }
    }

    //this is spagetti code to put a band-aid over the bug with nav mesh links and vertical rooms.
    private IEnumerator TraverseOffMeshLink()
    {
        var endPos = _agent.currentOffMeshLinkData.endPos + Vector3.up * _agent.baseOffset / 2.25f;
        var savedVelocity = _agent.velocity;
        _agent.enabled = false;
        while (Vector3.Distance(_agent.transform.position, endPos) > 0.5f)
        {
            _agent.transform.position = Vector3.MoveTowards(_agent.transform.position, endPos, _agent.speed * Time.deltaTime);
            yield return null;
        }

        _agent.enabled = true;
        _agent.CompleteOffMeshLink();
        _agent.velocity = savedVelocity;
        _linkTraversing = false;
    }

    private void Respawn()
    {
        var randomValidSpawnPoint = DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)];
        randomValidSpawnPoint.y += 0.5f;
        _agent.Warp(randomValidSpawnPoint);
    }
}