using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image _barImage;
    private ResourceStat _health;

    // Start is called before the first frame update
    void Start()
    {
        _barImage = transform.Find("Bar").GetComponent<Image>();

        if (PlayerStats.Instance == null)
        {
            Debug.LogError("Player Stats Not Initialized?");
        }
        _health = PlayerStats.Instance.Health;
        
    }


    // Update is called once per frame
    void Update()
    {
        _barImage.fillAmount = _health.Percentage;
    }

    public bool AddHealth(int amt)
    {
        return _health.TrySpendResource(amt);
    }
}