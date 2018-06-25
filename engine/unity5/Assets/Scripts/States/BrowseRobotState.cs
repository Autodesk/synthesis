using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BrowseRobotState : State
{
    private FileBrowser fileBrowser;

    public override void OnGUI()
    {
        if (fileBrowser == null)
        {
            string robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
            fileBrowser = new FileBrowser("Choose Robot Directory", robotDirectory, true) { Active = true };
            fileBrowser.OnComplete += OnBrowserComplete;
        }

        fileBrowser.Render();
 
        if (!fileBrowser.Active)
            StateMachine.Instance.PopState();
    }

    public void OnBrowserComplete(object obj)
    {
        string fileLocation = (string)obj;
        // If dir was selected...
        DirectoryInfo directory = new DirectoryInfo(fileLocation);
        if (directory != null && directory.Exists)
        {
            PlayerPrefs.SetString("RobotDirectory", directory.FullName);
            PlayerPrefs.Save();
            StateMachine.Instance.PopState();
        }
        else
        {
            UserMessageManager.Dispatch("Invalid selection!", 10f);
        }
    }
}
