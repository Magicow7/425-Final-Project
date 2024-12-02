using System.Collections;
using Stat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider _mainBar;
    [SerializeField] private Slider _backBar;

    [SerializeField] private float _resetTime;
    [SerializeField] private float _lastUpdate;
    private bool _coroutineLock;

    private ResourceStat _health;


    private float _lastHealth;
    private bool _lowHealth;

    // Start is called before the first frame update
    private void Start()
    {
        _health = PlayerStats.Instance.Health;
        
        _lastHealth = _health.Value;

        if (PlayerStats.Instance == null)
        {
            Debug.LogError("Player Stats Not Initialized?");
        }

        _mainBar.value = _health.Value / _health.MaxValue;
        _backBar.value = _health.Value / _health.MaxValue;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_lastHealth / _health.MaxValue - _health.Percentage >= 0.001)
        {
            _lastUpdate = _resetTime;
            StartCoroutine(LoseHealth());
        }
        
        _mainBar.value = _health.Percentage;
        if (_backBar.value < _mainBar.value)
        {
            _backBar.value = _mainBar.value;
        }
        
        _lastUpdate -= Time.deltaTime;
        _lastHealth = _health.Value;

        if (!_lowHealth && _health.Percentage < .35)
        {
            _lowHealth = true;
            StartCoroutine(LowHealthSounds());
        }
    }

    private IEnumerator LoseHealth()
    {
        if (!_coroutineLock)
        {
            _coroutineLock = true;
            yield return new WaitUntil(() => _lastUpdate <= 0);
            while (_backBar.value > _mainBar.value)
            {
                _backBar.value -= 0.001f;
                yield return new WaitForNextFrameUnit();
            }

            _coroutineLock = false;
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
}