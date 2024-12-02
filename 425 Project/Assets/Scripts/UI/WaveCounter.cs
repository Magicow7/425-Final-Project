using TMPro;
using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    private EnemySpawner _enemySpawner;
    private TMP_Text _waveCounter;

    // Start is called before the first frame update
    private void Start()
    {
        _enemySpawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
        _waveCounter = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    private void Update()
    {
        _waveCounter.text = "Wave: " + _enemySpawner.WaveNumber;
    }
}