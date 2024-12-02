using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TreasureChest : DungeonRoomContainedObject
{
    [SerializeField] private Slider _slider;

    [SerializeField] private float _maxCountdownTime = 10f;

    [SerializeField] private float _countdownTime = 10f;

    [SerializeField] private bool _collected;

    [SerializeField] private bool _inRoom;

    private void Awake()
    {
        _inRoom = false;
        _countdownTime = _maxCountdownTime;
    }

    private void Update()
    {
        if (!_collected && !_inRoom)
        {
            _countdownTime += Time.deltaTime;
            _countdownTime = Mathf.Min(_countdownTime, _maxCountdownTime);
            _slider.value = 1 - _countdownTime / _maxCountdownTime;
        }

        if (_collected || !_inRoom)
        {
            return;
        }

        _countdownTime -= Time.deltaTime;
        _slider.value = 1f - _countdownTime / _maxCountdownTime;
        if (_countdownTime <= 0)
        {
            StartCoroutine(_UnlockChest());
            _collected = true;
        }
    }

    private IEnumerator _UnlockChest()
    {
        // TODO: Someone please animate this and make it look pretty
        yield return new WaitForSeconds(1f);

        GenerateTreasaure();
    }

    private void GenerateTreasaure()
    {
        Debug.LogError("TREASURE!");
    }

    public override void OnPlayerEnter()
    {
        _inRoom = true;
        Debug.LogWarning("Starting Treasure Countdown");
        // Some weird shit is happening, need to talk to Silas
    }

    public override void OnPlayerExit()
    {
        _inRoom = false;
        Debug.LogWarning("Stopping Treasure Countdown" + _countdownTime);
    }
}