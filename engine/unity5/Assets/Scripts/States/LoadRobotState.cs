using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// TODO: Do something similar for the field loading state.

public class LoadRobotState : State
{
    private string robotDirectory;
    private SelectRobotScrollable robotList;

    public override void Start()
    {
        robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
        robotList = GameObject.Find("SimLoadRobotList").GetComponent<SelectRobotScrollable>();
    }

    public override void Resume()
    {
        robotList.Refresh(PlayerPrefs.GetString("RobotDirectory"));
    }

    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    public void OnSelectRobotButtonPressed()
    {
        GameObject robotList = GameObject.Find("SimLoadRobotList");
        string entry = (robotList.GetComponent<SelectRobotScrollable>().selectedEntry);
        if (entry != null)
        {
            string simSelectedRobotName = robotList.GetComponent<SelectRobotScrollable>().selectedEntry;

            PlayerPrefs.SetString("simSelectedRobot", robotDirectory + "\\" + simSelectedRobotName + "\\");
            PlayerPrefs.SetString("simSelectedRobotName", simSelectedRobotName);

            StateMachine.Instance.PopState();
        }
        else
        {
            UserMessageManager.Dispatch("No Robot Selected!", 2);
        }
    }

    public void OnRobotExportButtonPressed()
    {
        Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
    }

    public void OnChangeRobotButtonPressed()
    {
        StateMachine.Instance.PushState(new BrowseRobotState());
    }
}
