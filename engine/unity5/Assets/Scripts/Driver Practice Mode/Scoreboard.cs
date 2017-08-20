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
    public string ToCSV()
    {
        return timeStamp.ToString() + ",\"" + description.ToString() + "\"," + pointValue.ToString();
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

    public static string SaveDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\synthesis\\GameSaves\\";

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
        else if (scoreDisplay == null || scoreLog == null || scoreLogScrollbar == null)
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
    /// Get a list of all known save files.
    /// </summary>
    /// <returns>A string list of the save file names (no extension).</returns>
    public static List<string> GetSaveFileList()
    {
        DirectoryInfo folder = new DirectoryInfo(SaveDirectory);
        FileInfo[] saveFiles = folder.GetFiles("*.csv");

        List<string> fileNames = new List<string>();

        foreach (FileInfo file in saveFiles)
            fileNames.Add(Path.GetFileNameWithoutExtension(file.Name));

        Debug.Log(fileNames);

        return fileNames;
    }

    /// <summary>
    /// Create a new CSV file for saving game events to.
    /// </summary>
    /// <param name="fileName">The name of the file (no extension).</param>
    public static void CreateNewSaveFile(string saveName)
    {
        Debug.Log(saveName);
        // If the save directory doesn't exist, create it
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }

        // Create CSV file
        using (StreamWriter writer = new StreamWriter(SaveDirectory + saveName + ".csv", true))
        {
            writer.WriteLine("\"Time\",\"Event Type\",\"Details\""); // Header line
            writer.Close();
        }
    }

    /// <summary>
    /// Delete a save file.
    /// </summary>
    /// <param name="saveFile">The name of the save file to delete.</param>
    public static void DeleteSaveFile(string saveFile)
    {
        if (File.Exists(SaveDirectory + saveFile + ".csv"))
        {
            File.Delete(SaveDirectory + saveFile + ".csv");
        }
    }

    /// <summary>
    /// Export a game log CSV file to a location on the computer.
    /// </summary>
    /// <param name="saveFile">The save file to export.</param>
    /// <param name="exportDirectory">The location to export the CSV to.</param>
    public static void ExportSaveFile(string saveFile, string exportDirectory)
    {
        if (File.Exists(exportDirectory))
        {
            File.Delete(exportDirectory);
        }

        FileUtil.CopyFileOrDirectory(SaveDirectory + saveFile + ".csv", exportDirectory);
        UserMessageManager.Dispatch("Export successful!", 5);
    }

    /// <summary>
    /// Saves the scoring events of the current game to a text file.
    /// </summary>
    public void Save(string filePath)
    {
        bool newFile = !File.Exists(filePath);
        
        Debug.Log("Saving to " + filePath);
        using (StreamWriter writer = new StreamWriter(filePath, true))
        {
            if (newFile)
                writer.WriteLine("\"Time\",\"Event Type\",\"Details\""); // Header line

            writer.WriteLine("0,\"New Session\"");

            string fieldName = new DirectoryInfo(PlayerPrefs.GetString("simSelectedField")).Name;
            writer.WriteLine("0,\"Field\",\"" + fieldName + "\"");

            string robotName = new DirectoryInfo(PlayerPrefs.GetString("simSelectedRobot")).Name;
            writer.WriteLine("0,\"Robot\",\"" + robotName + "\"");

            bool lastEventGameStarted = false;
            float lastTime = 0;

            for (int i = 0; i < scoreEvents.Count; i++)
            {
                if (scoreEvents[i].timeStamp > 0 && !lastEventGameStarted)
                {
                    writer.WriteLine("0,\"Game Start\"");
                    lastEventGameStarted = true;
                }
                else if (scoreEvents[i].timeStamp < 0 && lastEventGameStarted)
                {
                    writer.WriteLine(lastTime.ToString() + ",\"Game End\"");
                    lastEventGameStarted = false;
                }
                
                writer.WriteLine(scoreEvents[i].ToCSV());
                lastTime = scoreEvents[i].timeStamp;
            }

            if (lastEventGameStarted)
                writer.WriteLine(lastTime.ToString() + ",\"Game End\"");

            writer.WriteLine(lastTime.ToString() + ",\"Total Score\"," + GetScoreTotal().ToString());
            writer.WriteLine(lastTime.ToString() + ",\"End Session\"");

            writer.Close();
        }

        Debug.Log("Save successful!");
    }
}
