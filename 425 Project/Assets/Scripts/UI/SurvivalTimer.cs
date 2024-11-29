using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SurvivalTimer : MonoBehaviour
{
    private float currentTime = 0;
    private TMP_Text timer;

    // Start is called before the first frame update
    void Start()
    {
        timer = GetComponent<TMP_Text>();
        timer.text = currentTime.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;

        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
        timer.text = formattedTime;
    }
}
