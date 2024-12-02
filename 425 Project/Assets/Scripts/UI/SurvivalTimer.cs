using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SurvivalTimer : MonoBehaviour
{
    private float _currentTime = 0;
    private TMP_Text _timer;

    // Start is called before the first frame update
    void Start()
    {
        _timer = GetComponent<TMP_Text>();
        _timer.text = _currentTime.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        _currentTime += Time.deltaTime;

        TimeSpan timeSpan = TimeSpan.FromSeconds(_currentTime);
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        _timer.text = formattedTime;
    }
}
