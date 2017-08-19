using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using System;

public class LoadSavePanel : MonoBehaviour {

    private FileBrowser saveGameBrowser = null; //robot directory browser
    private bool customgameon = true; //whether the robot directory browser is on
    public static string saveGameDirectory; //file path for field directory
    private ArrayList gameSaves;
    void OnGUI()
    {
        if (saveGameDirectory != null) InitSaveGameBrowser();
    }

    public void InitSaveGameBrowser()
    {
        if (saveGameBrowser == null)
        {
            saveGameBrowser = new FileBrowser("Choose Field Directory", saveGameDirectory, true);
            saveGameBrowser.Active = true;
            saveGameBrowser.OnComplete += (object obj) =>
            {
                saveGameBrowser.Active = true;
                string fileLocation = (string)obj;
                // If dir was selected...
                DirectoryInfo directory = new DirectoryInfo(fileLocation);
                if (directory != null && directory.Exists)
                {
                    Debug.Log(directory);
                    saveGameDirectory = (directory.FullName);
                    customgameon = false;
                    PlayerPrefs.SetString("SaveGameDirectory", saveGameDirectory);
                    PlayerPrefs.Save();
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!", 10f);
                }
            };
        }
        if (customgameon) saveGameBrowser.Render();
    }

    public void LoadGameDirectory()
    {
        if (!saveGameBrowser.Active)
        {
            saveGameBrowser.Active = true;
        }
        customgameon = true;
        //currentTab = Tab.GameDir;
    }

    void Start()
    {
        saveGameDirectory = PlayerPrefs.GetString("SaveGameDirectory", (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "//synthesis//GameSaves"));
        saveGameDirectory = (Directory.Exists(saveGameDirectory)) ? saveGameDirectory : saveGameDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); //if the Game Save directory no longer exists, set it to the default application path.
       
   
        gameSaves = new ArrayList();

    }

}
