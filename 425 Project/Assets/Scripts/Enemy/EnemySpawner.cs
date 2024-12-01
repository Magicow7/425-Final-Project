using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Combat;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    public int waveNumber { get; private set; } = 0;
    public float waveInterval = 5f; // time in between waves
    
    private Vector3 modifier = new Vector3(0.2f, 0.2f, 0.2f); // ensures enemy spawns above floor

    // Waves 1-9 predefined to introduce enemy variants => endless mode after that
    private Dictionary<int, (int smallCount, int normalCount, int largeCount)> waveConfig = new Dictionary<int, (int, int, int)>
    {
        { 1, (0, 10, 0) },
        { 2, (0, 15, 0) },
        { 3, (0, 20, 0) },
        { 4, (5, 10, 0) },
        { 5, (10, 10, 0) },
        { 6, (15, 10, 0) },
        { 7, (5, 10, 1) },
        { 8, (10, 15, 2) },
        { 9, (15, 20, 3) },
    };

    // called from dungeon generator script
    public void SpawnEnemies()
    {
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        // Endless waves
        while (true)
        {
            waveNumber += 1;
            SpawnWave(waveNumber);
            yield return new WaitForSeconds(waveInterval);

            // Decrease wave interval as game progresses
            waveInterval = Mathf.Max(1f, waveInterval - 0.5f);
        }
    }

    private void SpawnWave(int waveNumber)
    {
        // Endless mode: small = 15 = 5X, normal = 20 + 5X, large = 3 + X, X = # waves beyond 9
        (int smallCount, int normalCount, int largeCount) = waveConfig.ContainsKey(waveNumber) ?
            waveConfig[waveNumber] : ((15 + (waveNumber - 9) * 5), (20 + (waveNumber - 9) * 5), (3 + (waveNumber - 9) * 1));

        // Spawn small enemies
        for (int i = 0; i < smallCount; i++)
        {
            SpawnEnemy("small");
        }

        // Spawn normal enemies
        for (int i = 0; i < normalCount; i++)
        {
            SpawnEnemy("normal");
        }

        // Spawn large enemies
        for (int i = 0; i < largeCount; i++)
        {
            SpawnEnemy("large");
        }
    }

    private void SpawnEnemy(string variant)
    {
        // Spawn enemy at a random spawner
        GameObject enemyObject = Instantiate(enemyPrefab, DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)] + modifier, Quaternion.identity);

        // Configure enemy stats based on variant
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        if (enemy != null)
        {
            // Health, Speed, Scale, AttackDamage
            switch (variant)
            {
                case "small":
                    enemy.ConfigureStats(20, 5, 0.75f, 5);
                    break;
                case "normal":
                    enemy.ConfigureStats(50, 2, 1, 10);
                    break;
                case "large":
                    enemy.ConfigureStats(150, 1, 1.5f, 20);
                    break;
            }
        }
    }

}
