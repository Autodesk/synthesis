using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
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

    void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        scoreWindow = AuxFunctions.FindObject(canvas, "ScorePanel");
        scoreLogWindow = AuxFunctions.FindObject(canvas, "ScoreLogPanel");

        scoreDisplay = AuxFunctions.FindObject(scoreWindow, "ScoreText").GetComponent<Text>();
        scoreLog = AuxFunctions.FindObject(scoreLogWindow, "ScoreLogText").GetComponent<Text>();
        scoreLogScrollbar = AuxFunctions.FindObject(scoreLogWindow, "Scrollbar Vertical").GetComponent<Scrollbar>();
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

    public void UpdateDisplay()
    {
        if (scoreDisplay != null && scoreLog != null)
        {
            scoreDisplay.text = GetScoreTotal().ToString();

            scoreLog.text = string.Join("\n", scoreEvents.Select(x => x.ToString()).ToArray()); // Convert events to strings, place in array, and join into string
        }

        if (scoreLogScrollbar != null)
            scoreLogScrollbar.value = 0;
    }

    /// <summary>
    /// Saves the events of the current game to a file.
    /// </summary>
    /// <returns>The directory of the stat file.</returns>
    public string Save()
    {
        string filePath = PlayerPrefs.GetString("simSelectedRobot") + "\\";
        string fileName = string.Format("score_log_{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", System.DateTime.Now);
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

                writer.WriteLine(scoreEvents[i].ToString());
            }

            if (lastEventGameStarted)
                writer.WriteLine("Game End");

            writer.WriteLine("Total Score: " + GetScoreTotal().ToString());

            writer.Close();
        }

        Debug.Log("Save successful!");
        
        return new DirectoryInfo(filePath).Name + "\\" + fileName;
    }
}
