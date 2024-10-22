using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;
    public int enemyCount = 5;

    public void SpawnEnemies(List<Vector3> spawnPositions, Vector3 modifier)
    {
        Debug.Log("BUH");
        for (int i = 0; i < enemyCount; i++)
        {
            Instantiate(enemy, spawnPositions[Random.Range(0, spawnPositions.Count)] + modifier, Quaternion.identity);
            Navigation enemyNav = enemy.GetComponent<Navigation>();
            Debug.Log(player);
            enemyNav.playerObject = player;
        }
    }
}
