﻿using Assets.Scripts.FSM;
using System;
using UnityEngine;

public class LoadRobotState : State
{
    private string robotDirectory;
    private SelectRobotScrollable robotList;

    /// <summary>
    /// Initializes the <see cref="LoadRobotState"/>.
    /// </summary>
    public override void Start()
    {
        robotDirectory = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
        robotList = GameObject.Find("SimLoadRobotList").GetComponent<SelectRobotScrollable>();
    }

    /// <summary>
    /// Updates the robot list when this state is activated.
    /// </summary>
    public override void Resume()
    {
        robotList.Refresh(PlayerPrefs.GetString("RobotDirectory"));
    }

    /// <summary>
    /// Pops the current state when the back button is pressed.
    /// </summary>
    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    /// <summary>
    /// Saves the current selected robot and pops the current <see cref="State"/> when
    /// the select robot button is pressed.
    /// </summary>
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

    /// <summary>
    /// Launches the browser and opens the robot export tutorials webpage.
    /// </summary>
    public void OnRobotExportButtonPressed()
    {
        Application.OpenURL("http://bxd.autodesk.com/synthesis/tutorials-robot.html");
    }

    /// <summary>
    /// Pushes a new <see cref="BrowseRobotState"/> when the change robot directory
    /// button is pressed.
    /// </summary>
    public void OnChangeRobotButtonPressed()
    {
        StateMachine.Instance.PushState(new BrowseRobotState());
    }
}
