using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using UnityEngine.Serialization;

public class DungeonRoomObject : MonoBehaviour
{
    [FormerlySerializedAs("navMeshSurface")] public NavMeshSurface _navMeshSurface;
    [FormerlySerializedAs("chestSpawnPoint"),SerializeField]
    private GameObject _chestSpawnPoint;
    [FormerlySerializedAs("spawnedChest"),SerializeField]
    private GameObject _spawnedChest;

    public void SpawnChest(GameObject chestPrefab){
        if(_chestSpawnPoint != null){
            _spawnedChest = Instantiate(chestPrefab, _chestSpawnPoint.transform.position, _chestSpawnPoint.transform.rotation);
        }else{
            Debug.Log("no chest spawn point");
        }
    }

    void OnDestroy(){
        Destroy(_spawnedChest);
    }
}
