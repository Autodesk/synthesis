using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An event that occurred during a Synthesis game that caused the player to earn points.
/// Examples include shooting to the hopper, crossing an obstacle, or the opponent earning a penalty.
/// </summary>
public struct ScoreEvent
{
    public int pointValue;     // Point value of the event.
    public float timeStamp;    // When the event occured. (Seconds since the start of the game)
    public string description; // Description of the event.
    
    /// <summary>
    /// Convert the score event to a string that can be read by the user.
    /// </summary>
    /// <returns>A human readable string.</returns>
    public string ToHumanReadable()
    {
        if (timeStamp < 0) // Time is negative, which means no timestamp was available when the event was created.
            return description + "  (" + pointValue.ToString() + " points)";
        else
            return "(" + timeStamp.ToString("0.00") + "s)  " + description + "  (" + pointValue.ToString() + " points)";
    }

    /// <summary>
    /// Convert the score event into a string that can be used as a line in a CSV file.
    /// </summary>
    /// <returns>A line of CSV.</returns>
    public string ScoreEventToCSV()
    {
        return timeStamp.ToString() + "," + pointValue.ToString() + "," + description.ToString();
    }
}

/// <summary>
/// Manages scorekeeping as well as the updating the score display and score log windows.
/// </summary>
public class Scoreboard : MonoBehaviour
{
    GameObject canvas;

    GameObject scoreWindow;
    GameObject scoreLogWindow;

    Text scoreDisplay;
    Text scoreLog;

    Scrollbar scoreLogScrollbar;
    GameplayTimer timer;
    DriverPracticeMode dpm;

    List<ScoreEvent> scoreEvents;

    private void Start()
    {
        ResetScore();
    }

    private void Update()
    {
        if (timer == null || dpm == null)
        {
            timer = GetComponent<GameplayTimer>();
            dpm = GetComponent<DriverPracticeMode>();
        }
        else if (scoreDisplay == null || scoreLog == null)
            FindElements();
    }

    /// <summary>
    /// Find and store the necessary UI elements related to the scoreboard.
    /// </summary>
    void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        scoreWindow = AuxFunctions.FindObject(canvas, "ScorePanel");
        scoreLogWindow = AuxFunctions.FindObject(canvas, "ScoreLogPanel");

        scoreDisplay = AuxFunctions.FindObject(scoreWindow, "ScoreText").GetComponent<Text>();
        scoreLog = AuxFunctions.FindObject(scoreLogWindow, "ScoreLogText").GetComponent<Text>();
        scoreLogScrollbar = AuxFunctions.FindObject(scoreLogWindow, "Scrollbar Vertical").GetComponent<Scrollbar>();
    }

    /// <summary>
    /// Add a certain number of points to the total score by creating a scoring event with the given description.
    /// The timestamp of the event is set to the time since game start, or -1 if no game has started.
    /// </summary>
    /// <param name="points">Points to add to the score.</param>
    /// <param name="description">Description of what caused the points to be earned.</param>
    public void AddPoints(int points, string description)
    {
        if (dpm.gameStarted && !dpm.gameEnded || !dpm.gameStarted)
        {
            ScoreEvent scrEvnt = new ScoreEvent();

            scrEvnt.pointValue = points;

            if (dpm.gameStarted && !dpm.gameEnded)
                scrEvnt.timeStamp = timer.GetTimeSinceStart();
            else
                scrEvnt.timeStamp = -1; // Game has not started, no point of reference for time.

            scrEvnt.description = description;

            scoreEvents.Add(scrEvnt);

            UpdateDisplay();
        }
    }

    /// <summary>
    /// Get the total score of the current game.
    /// </summary>
    /// <returns>The total score of the current game.</returns>
    public int GetScoreTotal()
    {
        int totalPoints = 0;

        for (int i = 0; i < scoreEvents.Count; i++)
        {
            totalPoints += scoreEvents[i].pointValue;
        }

        return totalPoints;
    }

    /// <summary>
    /// Get the list of scoring events of the current game.
    /// </summary>
    /// <returns>The list of recorded scoring events.</returns>
    public ScoreEvent[] GetScoreEvents()
    {
        return scoreEvents.ToArray();
    }

    /// <summary>
    /// Reset the list of scoring events (set the score to 0).
    /// </summary>
    public void ResetScore()
    {
        scoreEvents = new List<ScoreEvent>();
        UpdateDisplay();
    }

    /// <summary>
    /// Update the score display to the sum of the scoring events of the current game (the total score).
    /// </summary>
    public void UpdateDisplay()
    {
        if (scoreDisplay != null && scoreLog != null)
        {
            scoreDisplay.text = GetScoreTotal().ToString();

            scoreLog.text = string.Join("\n", scoreEvents.Select(x => x.ToHumanReadable()).ToArray()); // Convert events to strings, place in array, and join into string
        }

        if (scoreLogScrollbar != null)
            scoreLogScrollbar.value = 0;
    }

    /// <summary>
    /// Saves the scoring events of the current game to a text file.
    /// </summary>
    public void Save(string filePath, string fileName)
    {
        if (File.Exists(filePath + fileName))
        {
            Debug.Log("Overriding existing file");
            File.Delete(filePath + fileName);
        }
        Debug.Log("Saving to " + filePath + fileName);
        using (StreamWriter writer = new StreamWriter(filePath + fileName, false))
        {
            string fieldName = new DirectoryInfo(PlayerPrefs.GetString("simSelectedField")).Name;
            writer.WriteLine("Field: " + fieldName);

            bool lastEventGameStarted = false;

            for (int i = 0; i < scoreEvents.Count; i++)
            {
                if (scoreEvents[i].timeStamp > 0 && !lastEventGameStarted)
                {
                    writer.WriteLine("Game Start");
                    lastEventGameStarted = true;
                }
                else if (scoreEvents[i].timeStamp < 0 && lastEventGameStarted)
                {
                    writer.WriteLine("Game End");
                    lastEventGameStarted = false;
                }

                writer.WriteLine(scoreEvents[i].ToHumanReadable());
            }

            if (lastEventGameStarted)
                writer.WriteLine("Game End");

            writer.WriteLine("Total Score: " + GetScoreTotal().ToString());

            writer.Close();
        }

        Debug.Log("Save successful!");
    }

}
