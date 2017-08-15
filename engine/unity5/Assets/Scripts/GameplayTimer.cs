using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTimer : MonoBehaviour
{
    public Text timerText;

    public bool timerRunning;
    public float timeLimit = 135.0f;

    float timeStart;
    float timeStop;

    void Update()
    {
        UpdateTimer();
    }

    public void StartTimer()
    {
        timeStart = Time.time;
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        timeStart = Time.time - (timeStop - timeStart);
        timerRunning = true;
    }

    public void UpdateTimer()
    {
        if (timerRunning)
            timeStop = Time.time;

        timerText.text = ((int) (timeLimit - (timeStop - timeStart))).ToString();

        if (timeLimit - (Time.time - timeStart) <= 0)
        {
            // End of game
            StopTimer();
            timeStop = timeStart + timeLimit;

            //GameBehaviour.GameEnd();
            //return;
        }
    }
}