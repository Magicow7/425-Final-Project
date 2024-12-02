using System.Collections;
using Stat;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [SerializeField] private Slider _mainBar;
    [SerializeField] private Slider _backBar;

    [SerializeField] private float _resetTime;
    [SerializeField] private float _lastUpdate;
    private bool _coroutineLock;

    private ResourceStat _mana;
    
    private float _lastMana;
    private bool _lowMana;

    // Start is called before the first frame update
    private void Start()
    {
        _mana = PlayerStats.Instance.Mana;
        
        _lastMana = _mana.Value;

        if (PlayerStats.Instance == null)
        {
            Debug.LogError("Player Stats Not Initialized?");
        }

        _mainBar.value = _mana.Value / _mana.MaxValue;
        _backBar.value = _mana.Value / _mana.MaxValue;
    }

    // Update is called once per frame
    private void Update()
    {
        if (_lastMana / _mana.MaxValue - _mana.Percentage >= 0.001)
        {
            _lastUpdate = _resetTime;
            StartCoroutine(LoseMana());
        }
        
        _mainBar.value = _mana.Percentage;
        if (_backBar.value < _mainBar.value)
        {
            _backBar.value = _mainBar.value;
        }
        
        _lastUpdate -= Time.deltaTime;
        _lastMana = _mana.Value;

        if (!_lowMana && _mana.Percentage < .35)
        {
            _lowMana = true;
            StartCoroutine(LowManaSound());
        }
    }

    private IEnumerator LoseMana()
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
    
    private IEnumerator LowManaSound()
    {
        SoundManager.PlaySound(SoundManager.Sound.LowMana);
        while (_mana.Percentage < .3)
        {
            yield return null;
        }

        _lowMana = false;
    }
}