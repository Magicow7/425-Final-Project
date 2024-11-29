using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveCounter : MonoBehaviour
{
    private TMP_Text waveCounter;
    private EnemySpawner enemySpawner;

    // Start is called before the first frame update
    void Start()
    {
        enemySpawner = GameObject.Find("EnemySpawner").GetComponent<EnemySpawner>();
        waveCounter = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        waveCounter.text = "Wave: " + enemySpawner.waveNumber;
    }
}
