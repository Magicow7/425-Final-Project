using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Serialization;

public class DungeonRoomObject : MonoBehaviour
{
    [FormerlySerializedAs("navMeshSurface")]
    public NavMeshSurface _navMeshSurface;

    [FormerlySerializedAs("chestSpawnPoint"), SerializeField]
    private GameObject _chestSpawnPoint;

    [FormerlySerializedAs("spawnedChest"), SerializeField]
    private GameObject _spawnedChest;

    private void OnDestroy()
    {
        Destroy(_spawnedChest);
    }

    public void SpawnChest(GameObject chestPrefab)
    {
        if (_chestSpawnPoint != null)
        {
            _spawnedChest = Instantiate(chestPrefab, _chestSpawnPoint.transform.position, _chestSpawnPoint.transform.rotation);
        }
        else
        {
            Debug.Log("no chest spawn point");
        }
    }
}