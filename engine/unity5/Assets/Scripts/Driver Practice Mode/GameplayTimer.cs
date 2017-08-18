using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayTimer : MonoBehaviour
{
    public Color scoreInactiveColor = new Color(255 / 255f, 255 / 255f, 255 / 255f, 50 / 255f);
    public Color timerStopColor = new Color(235 / 255f, 0 / 255f, 0 / 255f, 50 / 255f);
    public Color scoreStopColor = new Color(255 / 255f, 0 / 255f, 0 / 255f, 127 / 255f);
    public Color timerStartColor = new Color(0 / 255f, 235 / 255f, 0 / 255f, 127 / 255f);
    public Color scoreStartColor = new Color(0 / 255f, 255 / 255f, 0 / 255f, 127 / 255f);

    GameObject canvas;

    GameObject timerWindow;
    GameObject scoreWindow;

    Text timerText;
    Image timerBackground;
    Image scoreBackground;

    public float timeLimit = 135.0f;

    bool timerRunning;
    float timeStart;
    float timeStop;
    float timeLeft;

    void Update()
    {
        if (timerText == null)
            FindElements();

        UpdateTimer();
    }

    void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        timerWindow = AuxFunctions.FindObject(canvas, "GameplayTimerPanel");
        scoreWindow = AuxFunctions.FindObject(canvas, "ScorePanel");

        timerText = AuxFunctions.FindObject(timerWindow, "TimerText").GetComponent<Text>();
        timerBackground = AuxFunctions.FindObject(timerWindow, "TimerTextField").GetComponent<Image>();
        scoreBackground = AuxFunctions.FindObject(scoreWindow, "Score").GetComponent<Image>();
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

        if (timerText != null)
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
        SetColors(timerStartColor, scoreStartColor);
    }

    public void StopTimer()
    {
        timerRunning = false;
        SetColors(timerStopColor, scoreInactiveColor);
    }

    public void ResumeTimer()
    {
        timeStart = Time.time - (timeStop - timeStart);
        timerRunning = true;
        SetColors(timerStartColor, scoreStartColor);
    }

    public void EndOfGame()
    {
        timerRunning = false;
        SetColors(timerStopColor, scoreStopColor);
    }

    void SetColors(Color timerColor, Color scoreColor)
    {
        if (timerBackground != null)
            timerBackground.color = timerColor;
        if (scoreBackground != null)
            scoreBackground.color = scoreColor;
    }
}