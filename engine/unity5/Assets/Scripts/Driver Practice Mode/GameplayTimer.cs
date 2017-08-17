using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTimer : MonoBehaviour
{
    public Text timerText;
    public Image timerBackground;
    public Image scoreBackground;

    public float timeLimit = 135.0f;

    bool timerRunning;
    float timeStart;
    float timeStop;
    float timeLeft;

    void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (timerRunning)
        {
            timeStop = Time.time;
            
            if (Time.time - timeStart >= timeLimit)
            {
                timeStop = timeStart + timeLimit + 0.01f; // Adjust past time limit to ensure time left is negative (prevents the ceiling function from returning 1 as the time)
                EndOfGame();
            }
        }

        timeLeft = timeLimit - (timeStop - timeStart);

        timerText.text = Mathf.CeilToInt(timeLeft).ToString();
    }

    public bool IsTimerRunning()
    {
        return timerRunning;
    }

    public float GetTimeLeft()
    {
        UpdateTimer();
        return timeLeft;
    }

    public float GetTimeSinceStart()
    {
        return timeLimit - GetTimeLeft();
    }

    public void StartTimer()
    {
        timeStart = Time.time;
        timerRunning = true;
        scoreBackground.color = new Color(0 /255f,  235 /255f,  0 /255f,  127 /255f);
        timerBackground.color = new Color(0 / 255f, 255 / 255f, 0 / 255f, 127 / 255f);
    }

    public void StopTimer()
    {
        timerRunning = false;
        scoreBackground.color = new Color(255 /255f,  255 /255f, 255 /255f, 50 /255f);
        timerBackground.color = new Color(255 / 255f, 0 / 255f,  0 / 255f,  127 / 255f);
    }

    public void ResumeTimer()
    {
        timeStart = Time.time - (timeStop - timeStart);
        timerRunning = true;
    }

    public void EndOfGame()
    {
        timerRunning = false;
        scoreBackground.color = new Color(235 / 255f, 0 / 255f, 0 / 255f, 50 / 255f);
        timerBackground.color = new Color(255 / 255f, 0 / 255f, 0 / 255f, 127 / 255f);
    }
}