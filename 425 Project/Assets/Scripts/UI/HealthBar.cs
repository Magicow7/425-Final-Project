using System.Collections;
using System.Collections.Generic;
using Stat;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image _barImage;
    private ResourceStat _health;
    private bool _lowHealth = false;

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
        if (!_lowHealth && _health.Percentage < .35)
        {
            _lowHealth = true;
            StartCoroutine(LowHealthSounds());
        }
    }

    private IEnumerator LowHealthSounds()
    {
        SoundManager.ChangeBackgroundMusic(SoundManager.Sound.LowHpBackground);
        SoundManager.PlaySound(SoundManager.Sound.PlayerLowHp);
        while (_health.Percentage < .5)
        {
            yield return null;
        }
        float volume = SoundManager.GetVolume(SoundManager.Sound.LowHpBackground);
        for (float t = 0; t < 1; t += Time.deltaTime)
        {
            SoundManager.ChangeBackgroundVolume(Mathf.Lerp(volume, 0, t));
            yield return null;
        }
        SoundManager.ChangeBackgroundMusic(SoundManager.Sound.NormalBackground);
        _lowHealth = false;
    }

    public bool AddHealth(int amt)
    {
        return _health.TrySpendResource(amt);
    }
}