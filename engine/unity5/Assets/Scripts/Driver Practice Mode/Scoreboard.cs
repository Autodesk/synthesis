using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public struct ScoreEvent
{
    public int value;          // Point value of the event
    public float timeStamp;    // When the event occured since the start of the scene
    public string description; // Description of the event (ball in hopper, gear on hook, etc.)

    public override string ToString()
    {
        if (timeStamp < 0) // No timestamp
            return description + "  (" + value.ToString() + " points)";
        else
            return "(" + timeStamp.ToString("0.00") + "s)  " + description + "  (" + value.ToString() + " points)";
    }
}

public class Scoreboard : MonoBehaviour
{
    public Text scoreDisplay;
    public Text scoreLog;

    Scrollbar scoreLogScrollbar;
    GameplayTimer timer;
    DriverPracticeMode dpm;

    List<ScoreEvent> scoreEvents;

    private void Start()
    {
        // Get scrollbar so that it can be reset when log is updated
        GameObject verticalScroll = AuxFunctions.FindObject(scoreLog.transform.parent.parent.parent.gameObject, "Scrollbar Vertical");
        scoreLogScrollbar = verticalScroll.GetComponent<Scrollbar>();
        
        timer = GetComponent<GameplayTimer>();
        dpm = GetComponent<DriverPracticeMode>();

        ResetScore();
    }

    public void AddPoints(int points, string description)
    {
        if (dpm.gameStarted && !dpm.gameEnded || !dpm.gameStarted)
        {
            ScoreEvent scrEvnt = new ScoreEvent();

            scrEvnt.value = points;

            if (dpm.gameStarted && !dpm.gameEnded)
                scrEvnt.timeStamp = timer.GetTimeSinceStart();
            else
                scrEvnt.timeStamp = -1; // Game has not started, no point of reference for time.

            scrEvnt.description = description;

            scoreEvents.Add(scrEvnt);

            UpdateDisplay();
        }
    }

    public int GetScoreTotal()
    {
        int totalPoints = 0;

        for (int i = 0; i < scoreEvents.Count; i++)
        {
            totalPoints += scoreEvents[i].value;
        }

        return totalPoints;
    }

    public ScoreEvent[] GetScoreEvents()
    {
        return scoreEvents.ToArray();
    }

    public void ResetScore()
    {
        scoreEvents = new List<ScoreEvent>();
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        scoreDisplay.text = GetScoreTotal().ToString();

        scoreLog.text = string.Join("\n", scoreEvents.Select(x => x.ToString()).ToArray()); // Convert events to strings, place in array, and join into string
        scoreLogScrollbar.value = 0;
    }
}
