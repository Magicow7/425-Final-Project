using System.Collections;
using System.Collections.Generic;
using Combat;
using Stat;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Enemy _enemyPrefab;

    [SerializeField] private int _maxEnemies;

    [SerializeField] private float _healthScaling = 0.05f;

    [SerializeField] private float _damageScaling = 0.05f;

    private readonly Vector3 _modifier = new(0.2f, 0.2f, 0.2f); // ensures enemy spawns above floor
    private readonly float _spawnInterval = 0.01f; // lag time so enemies don't spawn on top of one another

    // Waves 1-9 predefined to introduce enemy variants => endless mode after that
    private readonly Dictionary<int, (int smallCount, int normalCount, int largeCount)> _waveConfig = new()
    {
        { 1, (0, 10, 0) },
        { 2, (0, 15, 0) },
        { 3, (0, 20, 0) },
        { 4, (5, 10, 0) },
        { 5, (10, 10, 0) },
        { 6, (15, 10, 0) },
        { 7, (5, 10, 1) },
        { 8, (10, 15, 2) },
        { 9, (15, 20, 3) }
    };

    private int _currentSpawnedEnemy;
    private Enemy[] _spawnedEnemies;
    private float _waveInterval = 30.0f; // time in between waves

    public int WaveNumber { get; private set; }

    private void Awake()
    {
        _spawnedEnemies = new Enemy[_maxEnemies];
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

            // Regenerate 20% of health after each wave
            if (PlayerStats.Instance)
            {
                PlayerStats.Instance.RegenerateHealth(PlayerStats.Instance.Health.MaxValue / 25, 5);
                PlayerStats.Instance.WeaponPower.Value += 0.1f;
            }

            // Decrease wave interval as game progresses
            _waveInterval = Mathf.Max(10f, _waveInterval - 1.0f);
        }
    }

    private IEnumerator SpawnWave(int waveNumber)
    {
        // Endless mode: small = 15 = 5X, normal = 20 + 5X, large = 3 + X, X = # waves beyond 9
        (int smallCount, int normalCount, int largeCount) = _waveConfig.ContainsKey(waveNumber) ? _waveConfig[waveNumber] : (15 + (waveNumber - 9) * 5, 20 + (waveNumber - 9) * 5, 3 + (waveNumber - 9) * 1);

        // Spawn small enemies
        for (var i = 0; i < smallCount; i++)
        {
            SpawnEnemy(EnemyType.Small);
            yield return new WaitForSeconds(_spawnInterval);
        }

        // Spawn normal enemies
        for (var i = 0; i < normalCount; i++)
        {
            SpawnEnemy(EnemyType.Medium);
            yield return new WaitForSeconds(_spawnInterval);
        }

        // Spawn large enemies
        for (var i = 0; i < largeCount; i++)
        {
            SpawnEnemy(EnemyType.Large);
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnEnemy(EnemyType variant)
    {
        if (_spawnedEnemies[_currentSpawnedEnemy])
        {
            Destroy(_spawnedEnemies[_currentSpawnedEnemy].gameObject);
        }

        // Spawn enemy at a random spawner
        _spawnedEnemies[_currentSpawnedEnemy] = Instantiate(_enemyPrefab, DungeonGenerator.possibleSpawnPoints[Random.Range(0, DungeonGenerator.possibleSpawnPoints.Count)] + _modifier, Quaternion.identity);

        // Scale stats based on wave number
        float healthMultiplier = 1 + WaveNumber * _healthScaling; // 5% flat increase per wave
        float attackDamageMultiplier = 1 + WaveNumber * _damageScaling; // 5% flat increase per wave

        // Health, Speed, Scale, AttackDamage
        switch (variant)
        {
            case EnemyType.Small:
                _spawnedEnemies[_currentSpawnedEnemy].ConfigureStats(20 * healthMultiplier, 5, 0.75f, 3 * attackDamageMultiplier, 1);
                break;
            case EnemyType.Medium:
                _spawnedEnemies[_currentSpawnedEnemy].ConfigureStats(50 * healthMultiplier, 2, 1, 5 * attackDamageMultiplier, 1);
                break;
            case EnemyType.Large:
                _spawnedEnemies[_currentSpawnedEnemy].ConfigureStats(150 * healthMultiplier, 1, 1.5f, 10 * attackDamageMultiplier, 1);
                break;
        }

        _currentSpawnedEnemy = (_currentSpawnedEnemy + 1) % _maxEnemies;
    }

    private enum EnemyType
    {
        Small,
        Medium,
        Large
    }
}