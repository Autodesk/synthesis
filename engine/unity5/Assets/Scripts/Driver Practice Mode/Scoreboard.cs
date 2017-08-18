using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEditor;
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

    public void UpdateDisplay()
    {
        scoreDisplay.text = GetScoreTotal().ToString();

        scoreLog.text = string.Join("\n", scoreEvents.Select(x => x.ToString()).ToArray()); // Convert events to strings, place in array, and join into string
        scoreLogScrollbar.value = 0;
    }

    /// <summary>
    /// Saves the events of the current game to a file.
    /// </summary>
    /// <returns>The directory of the stat file.</returns>
   
    // Opens a file selection dialog for a PNG file and saves a selected texture to the file.


public class EditorUtilitySaveFilePanel : MonoBehaviour
{
    [MenuItem("Save Database to file")]
    static void NewFile()
    {
        Texture2D texture = Selection.activeObject as Texture2D;
        if (texture == null)
        {
            EditorUtility.DisplayDialog(
                "Select Texture",
                "You Must Select a Texture first!",
                "Ok");
            return;
        }

        var path = EditorUtility.SaveFilePanel(
                "Save texture as PNG",
                "",
                texture.name + ".png",
                "png");

        if (path.Length != 0)
        {
            var pngData = texture.EncodeToPNG();
            if (pngData != null)
                File.WriteAllBytes(path, pngData);
        }
    }
}
    public string Save()
    {
        string filePath = PlayerPrefs.GetString("simSelectedRobot") + "\\";
        string fileName = string.Format("stats_{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", System.DateTime.Now);
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

            writer.Close();
        }

        Debug.Log("Save successful!");

        return new DirectoryInfo(filePath).Name + "\\" + fileName;
    }
    public class OpenFilePanelExample : EditorWindow
    {
        [MenuItem("Load Database")]
        static void LoadFile()
        {
            Texture2D texture = Selection.activeObject as Texture2D;
            if (texture == null)
            {
                EditorUtility.DisplayDialog("Select Texture", "You must select a texture first!", "OK");
                return;
            }

            string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");
            if (path.Length != 0)
            {
                WWW www = new WWW("file:///" + path);
                www.LoadImageIntoTexture(texture);
            }
        }
    }

}
