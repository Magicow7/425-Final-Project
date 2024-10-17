using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    private Image _barImage;
    private ResourceStat _mana;

    // Start is called before the first frame update
    void Start()
    {
        _barImage = transform.Find("Bar").GetComponent<Image>();

        if (PlayerStats.Instance == null)
        {
            Debug.LogError("Player Stats Not Initialized?");
        }
        _mana = PlayerStats.Instance.Mana;
    }


    // Update is called once per frame
    void Update()
    {
        _barImage.fillAmount = _mana.Percentage;
    }

    public bool UseMana(int amt)
    {
        return _mana.TrySpendResource(amt);
    }
}