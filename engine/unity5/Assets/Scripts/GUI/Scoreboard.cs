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
        return "(" + timeStamp.ToString() + "s)    " + description + "  (" + value.ToString() + " points)";
    }
}

public class Scoreboard : MonoBehaviour
{
    public Text scoreDisplay;
    public Text scoreLog;

    List<ScoreEvent> scoreEvents;

    Scrollbar scoreLogScrollbar;

    private void Awake()
    {
        // Get scrollbar so that it can be reset when log is updated
        GameObject verticalScroll = AuxFunctions.FindObject(scoreLog.transform.parent.parent.parent.gameObject, "Scrollbar Vertical");
        scoreLogScrollbar = verticalScroll.GetComponent<Scrollbar>();
    }

    private void OnEnable()
    {
        ResetScore();
    }

    public void AddPoints(int points, string description)
    {
        ScoreEvent scrEvnt = new ScoreEvent();

        scrEvnt.value = points;
        scrEvnt.timeStamp = Time.time;
        scrEvnt.description = description;

        scoreEvents.Add(scrEvnt);

        UpdateDisplay();
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
