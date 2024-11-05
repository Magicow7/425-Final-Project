using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;
    public int enemyCount = 5;

    private void Start()
    {
        Navigation enemyNav = enemy.GetComponent<Navigation>();
        enemyNav.playerObject = player;
    }

    public void SpawnEnemies(List<Vector3> spawnPositions, Vector3 modifier)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Instantiate(enemy, spawnPositions[Random.Range(0, spawnPositions.Count)] + modifier, Quaternion.identity);
        }
    }
}
