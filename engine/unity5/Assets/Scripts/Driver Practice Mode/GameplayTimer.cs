using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the starting, stopping, etc. of the game timer, as well as the text of the timer display.
/// </summary>
public class GameplayTimer : MonoBehaviour
{
    public float timeLimit = 135.0f;

    private GameObject canvas;

    GameObject timerWindow;

    Text timerText;

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

    /// <summary>
    /// Find and store the necessary UI elements related to the timer.
    /// </summary>
    void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        timerWindow = AuxFunctions.FindObject(canvas, "GameplayTimerPanel");

        timerText = AuxFunctions.FindObject(timerWindow, "TimerText").GetComponent<Text>();
    }

    /// <summary>
    /// Update the timer display and the value of the timerLeft variable.
    /// </summary>
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

    /// <summary>
    /// Whether the timer is currently running or not.
    /// </summary>
    /// <returns>True if the timer is running, false otherwise.</returns>
    public bool IsTimerRunning()
    {
        return timerRunning;
    }

    /// <summary>
    /// Get the amount of time in seconds left on the timer.
    /// </summary>
    /// <returns>Time in seconds left on timer.</returns>
    public float GetTimeLeft()
    {
        UpdateTimer();
        return timeLeft;
    }

    /// <summary>
    /// Get the amount of time in seconds since the timer was started.
    /// </summary>
    /// <returns>Time in seconds since timer start.</returns>
    public float GetTimeSinceStart()
    {
        return timeLimit - GetTimeLeft();
    }

    /// <summary>
    /// Start the timer and reset the time.
    /// </summary>
    public void StartTimer()
    {
        timeStart = Time.time;
        timerRunning = true;
    }

    /// <summary>
    /// Stop the timer.
    /// </summary>
    public void StopTimer()
    {
        timerRunning = false;
    }

    /// <summary>
    /// Start the timer without resetting the time.
    /// </summary>
    public void ResumeTimer()
    {
        timeStart = Time.time - (timeStop - timeStart);
        timerRunning = true;
    }

    /// <summary>
    /// Run when the game ends by the timer running out.
    /// </summary>
    public void EndOfGame()
    {
        timerRunning = false;
    }
}