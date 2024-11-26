using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    private Image _barImage;
    private ResourceStat _mana;

    private bool canShakeBar = true;
    private RectTransform _rectTransform;

    // Start is called before the first frame update
    void Start()
    {
        _barImage = transform.Find("Bar").GetComponent<Image>();
        _rectTransform = GetComponent<RectTransform>();
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

    private IEnumerator ShakeBar()
    {
        Vector3 pos = _rectTransform.anchoredPosition;
        
        for (int i = 0; i < 3; i++)
        {
            _rectTransform.anchoredPosition = pos + new Vector3(-5, 0, 0);
            yield return new WaitForSeconds(0.05f);
            _rectTransform.anchoredPosition = pos + new Vector3(5, 0, 0);
            yield return new WaitForSeconds(0.05f);
        }

        _rectTransform.anchoredPosition = pos;
        canShakeBar = true;
    }

    public bool UseMana(int amt)
    {
        bool success = _mana.TrySpendResource(amt);

        if (success)
        {
            // nothing to do
        }
        else
        {
            if (canShakeBar)
            {
                canShakeBar = false;
                // not enough mana probably
                
                StartCoroutine(ShakeBar());
            }

        }
        return success;
    }
}