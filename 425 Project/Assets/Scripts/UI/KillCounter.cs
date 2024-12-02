using Combat;
using TMPro;
using UnityEngine;

public class KillCounter : MonoBehaviour
{
    private int _killCount;
    private TMP_Text _killCounter;

    // Start is called before the first frame update
    private void Start()
    {
        _killCounter = GetComponent<TMP_Text>();
        _killCounter.text = "Kills: " + _killCount;
        Enemy.EnemyDeath += IncrementKillCounter;
    }

    public void IncrementKillCounter()
    {
        _killCount++;
        _killCounter.text = "Kills: " + _killCount;
        TextUpdates.Instance.setKills(_killCount);
    }
}