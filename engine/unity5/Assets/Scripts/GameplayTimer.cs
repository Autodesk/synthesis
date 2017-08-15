using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTimer : MonoBehaviour
{
    public Text timerText;
    public Image scoreBackground;

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
        {
            timeStop = Time.time;

            scoreBackground.color = Color.green;
        }
        else
        {
            scoreBackground.color = Color.red;
        }

        float timeLeft = timeLimit - (timeStop - timeStart);

        timerText.text = ((int) timeLeft).ToString();

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