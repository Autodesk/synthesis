using Synthesis.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Synthesis.States;
using Synthesis.Utils;
using System.IO;
using Synthesis.MixAndMatch;

public class SimStart : MonoBehaviour
{
    /// <summary>
    /// Starts the simulation by adding a <see cref="MainState"/> to the scene's <see cref="StateMachine"/>.
    /// </summary>
    private void Start()
    {
        StateMachine stateMachine = GetComponent<StateMachine>();

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("FieldDirectory")))
        {
            PlayerPrefs.SetString("FieldDirectory", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields");
        }

        if (string.IsNullOrEmpty(PlayerPrefs.GetString("RobotDirectory")))
        {
            PlayerPrefs.SetString("RobotDirectory", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Robots");
        }

        DetermineSelected();

        if (stateMachine == null)
            Debug.LogError("Could not find the required StateMachine component!");
        else
            stateMachine.PushState(new MainState());
    }

    private bool DetermineSelected()
    {
        if (!AppModel.InitialLaunch)
            return false;

        AppModel.InitialLaunch = false;

        //Loads robot and field directories from command line arguments if valid.
        string[] args = Environment.GetCommandLineArgs();
        bool robotDefined = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i].ToLower().Equals("-robot"))
            {
                if (i < args.Length - 1)
                {
                    string robotFile = args[++i];

                    DirectoryInfo dirInfo = new DirectoryInfo(robotFile);
                    PlayerPrefs.SetString("RobotDirectory", dirInfo.Parent.FullName);
                    PlayerPrefs.SetString("simSelectedRobot", robotFile);
                    PlayerPrefs.SetString("simSelectedRobotName", dirInfo.Name);
                    robotDefined = true;
                }
            }
        }

        if (robotDefined)
        {
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);

            RobotTypeManager.SetProperties(false);
            return true;
        }

        return false;
    }

}
