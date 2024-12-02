using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveCounter : MonoBehaviour
{
    private TMP_Text _waveCounter;
    private EnemySpawner _enemySpawner;

    // Start is called before the first frame update
    void Start()
    {
        _enemySpawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
        _waveCounter = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        _waveCounter.text = "Wave: " + _enemySpawner.WaveNumber;
    }
}
