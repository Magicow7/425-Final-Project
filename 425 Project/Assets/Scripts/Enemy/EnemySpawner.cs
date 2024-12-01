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
    private float waveInterval = 10.0f; // time in between waves
    private float spawnInterval = 0.01f; // lag time so enemies don't spawn on top of one another
    
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
            yield return StartCoroutine(SpawnWave(waveNumber));
            yield return new WaitForSeconds(waveInterval);

            // Decrease wave interval as game progresses
            waveInterval = Mathf.Max(5f, waveInterval - 1.0f);
        }
    }

    private IEnumerator SpawnWave(int waveNumber)
    {
        // Endless mode: small = 15 = 5X, normal = 20 + 5X, large = 3 + X, X = # waves beyond 9
        (int smallCount, int normalCount, int largeCount) = waveConfig.ContainsKey(waveNumber) ?
            waveConfig[waveNumber] : ((15 + (waveNumber - 9) * 5), (20 + (waveNumber - 9) * 5), (3 + (waveNumber - 9) * 1));

        // Spawn small enemies
        for (int i = 0; i < smallCount; i++)
        {
            SpawnEnemy("small");
            yield return new WaitForSeconds(spawnInterval);
        }

        // Spawn normal enemies
        for (int i = 0; i < normalCount; i++)
        {
            SpawnEnemy("normal");
            yield return new WaitForSeconds(spawnInterval);
        }

        // Spawn large enemies
        for (int i = 0; i < largeCount; i++)
        {
            SpawnEnemy("large");
            yield return new WaitForSeconds(spawnInterval);
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
            // Scale stats based on wave number
            float healthMultiplier = 1 + (waveNumber * 0.05f); // 5% flat increase per wave
            float attackDamageMultiplier = 1 + (waveNumber * 0.05f); // 5% flat increase per wave

            // Health, Speed, Scale, AttackDamage
            switch (variant)
            {
                case "small":
                    enemy.ConfigureStats(20 * healthMultiplier, 5, 0.75f, 5 * attackDamageMultiplier);
                    break;
                case "normal":
                    enemy.ConfigureStats(50 * healthMultiplier, 2, 1, 10 * attackDamageMultiplier);
                    break;
                case "large":
                    enemy.ConfigureStats(150 * healthMultiplier, 1, 1.5f, 20 * attackDamageMultiplier);
                    break;
            }
        }
    }

}