using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;

    private int waveNumber = 0;
    public float waveInterval = 5f; // time spent waiting after running SpawnWave() to completion
    public float spawnInterval = 0.1f; // Delayed so enemies don't spawn on top of each other
    public int enemiesPerWave = 5;

    private Vector3 modifier = new Vector3(0.5f,0.5f,0.5f);

    
    public void SpawnEnemies()
    {
        StartCoroutine(SpawnWaves());
    }

    // Time Between Waves = enemiesPerWave * spawnInterval + waveInterval
    private IEnumerator SpawnWaves()
    {
        // Endless waves
        for(;;)
        {
            Debug.Log("WAVE NUMBER: " + waveNumber);
            yield return StartCoroutine(SpawnWave());

            // Increase number of enemies for next wave
            enemiesPerWave += 5;
            // Decrease wave interval as game progresses
            waveInterval = Mathf.Max(1f, waveInterval - 0.5f);
            waveNumber += 1;

            yield return new WaitForSeconds(waveInterval);
        }
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++) {
            Instantiate(enemy, DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)] + modifier, Quaternion.identity);
            //this is now handled by the EnemyNavigation Start Method where it refrences the player refrenced by the dungeon generator
            /*
            EnemyNavigation enemyNav = enemy.GetComponent<EnemyNavigation>();
            enemyNav.playerObject = player;*/
            yield return new WaitForSeconds(spawnInterval);
        }
    }   
}
