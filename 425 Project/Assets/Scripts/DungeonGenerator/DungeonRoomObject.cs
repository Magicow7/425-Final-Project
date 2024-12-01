using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class DungeonRoomObject : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    [SerializeField]
    private GameObject chestSpawnPoint;
    [SerializeField]
    private GameObject spawnedChest;

    public void SpawnChest(GameObject chestPrefab){
        if(chestSpawnPoint != null){
            spawnedChest = Instantiate(chestPrefab, chestSpawnPoint.transform.position, chestSpawnPoint.transform.rotation);
        }else{
            Debug.Log("no chest spawn point");
        }
    }

    void OnDestroy(){
        Destroy(spawnedChest);
    }
}
