using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillCounter : MonoBehaviour
{
    private TMP_Text _killCounter;
    private int _killCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        _killCounter = GetComponent<TMP_Text>();
        _killCounter.text = "Kills: " + _killCount;
        Combat.Enemy.EnemyDeath += IncrementKillCounter;
    }

    public void IncrementKillCounter()
    {
        killCount++;
        killCounter.text = "Kills: " + killCount;
        TextUpdates.Instance.setKills(killCount);
    }
}