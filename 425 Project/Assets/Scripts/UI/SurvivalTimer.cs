using System;
using TMPro;
using UnityEngine;

public class SurvivalTimer : MonoBehaviour
{
    private float _currentTime;
    private TMP_Text _timer;

    // Start is called before the first frame update
    private void Start()
    {
        _timer = GetComponent<TMP_Text>();
        _timer.text = _currentTime.ToString();
    }

    // Update is called once per frame
    private void Update()
    {
        _currentTime += Time.deltaTime;

        var timeSpan = TimeSpan.FromSeconds(_currentTime);
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        _timer.text = formattedTime;
        TextUpdates.Instance.setTimeAlive(_currentTime);
    }
}