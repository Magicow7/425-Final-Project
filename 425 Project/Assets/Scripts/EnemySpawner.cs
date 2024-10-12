using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public GameObject player;
    public int enemyCount = 5;

    public void SpawnEnemies(List<GameObject> spawnPositions, Vector3 modifier)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Instantiate(enemy, spawnPositions[Random.Range(0, spawnPositions.Count)].transform.position + modifier, Quaternion.identity);
            Navigation enemyNav = enemy.GetComponent<Navigation>();
            Debug.Log(player);
            enemyNav.playerObject = player;
        }
    }
}
