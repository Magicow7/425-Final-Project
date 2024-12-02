using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Combat;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private Enemy _enemyPrefab;
    [SerializeField]
    private int _maxEnemies;

    public int WaveNumber { get; private set; } = 0;
    private float _waveInterval = 30.0f; // time in between waves
    private readonly float _spawnInterval = 0.01f; // lag time so enemies don't spawn on top of one another
    
    private readonly Vector3 _modifier = new Vector3(0.2f, 0.2f, 0.2f); // ensures enemy spawns above floor
    private int[] _spawnedEnemies;

    // Waves 1-9 predefined to introduce enemy variants => endless mode after that
    private readonly Dictionary<int, (int smallCount, int normalCount, int largeCount)> _waveConfig = new Dictionary<int, (int, int, int)>
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
    
    private enum EnemyType
    {
        Small,
        Medium,
        Large
    }

    private void Awake()
    {
        _spawnedEnemies = new int[_maxEnemies];
    }

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
            WaveNumber += 1;
            yield return StartCoroutine(SpawnWave(WaveNumber));
            yield return new WaitForSeconds(_waveInterval);

            // Decrease wave interval as game progresses
            _waveInterval = Mathf.Max(10f, _waveInterval - 1.0f);
        }
    }

    private IEnumerator SpawnWave(int waveNumber)
    {
        // Endless mode: small = 15 = 5X, normal = 20 + 5X, large = 3 + X, X = # waves beyond 9
        (int smallCount, int normalCount, int largeCount) = _waveConfig.ContainsKey(waveNumber) ?
            _waveConfig[waveNumber] : ((15 + (waveNumber - 9) * 5), (20 + (waveNumber - 9) * 5), (3 + (waveNumber - 9) * 1));

        // Spawn small enemies
        for (int i = 0; i < smallCount; i++)
        {
            SpawnEnemy(EnemyType.Small);
            yield return new WaitForSeconds(_spawnInterval);
        }

        // Spawn normal enemies
        for (int i = 0; i < normalCount; i++)
        {
            SpawnEnemy(EnemyType.Medium);
            yield return new WaitForSeconds(_spawnInterval);
        }

        // Spawn large enemies
        for (int i = 0; i < largeCount; i++)
        {
            SpawnEnemy(EnemyType.Large);
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnEnemy(EnemyType variant)
    {
        // Spawn enemy at a random spawner
        Enemy enemy = Instantiate(_enemyPrefab, DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)] + _modifier, Quaternion.identity);

        if (enemy != null)
        {
            // Scale stats based on wave number
            float healthMultiplier = 1 + (WaveNumber * 0.05f); // 5% flat increase per wave
            float attackDamageMultiplier = 1 + (WaveNumber * 0.05f); // 5% flat increase per wave

            // Health, Speed, Scale, AttackDamage
            switch (variant)
            {
                case EnemyType.Small:
                    enemy.ConfigureStats(20 * healthMultiplier, 5, 0.75f, 5 * attackDamageMultiplier);
                    break;
                case EnemyType.Medium:
                    enemy.ConfigureStats(50 * healthMultiplier, 2, 1, 10 * attackDamageMultiplier);
                    break;
                case EnemyType.Large:
                    enemy.ConfigureStats(150 * healthMultiplier, 1, 1.5f, 20 * attackDamageMultiplier);
                    break;
            }
        }
    }

}