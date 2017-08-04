﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;

/// <summary>
/// Class for controlling the various aspects of local multiplayer
/// </summary>
public class LocalMultiplayer : MonoBehaviour {

    private GameObject canvas;
    private MainState mainState;
    private SimUI simUI;

    private GameObject multiplayerWindow;
    private GameObject addRobotWindow;

    private GameObject[] robotButtons = new GameObject[6];
    private int activeIndex = 0;
    private GameObject highlight;

    /// <summary>
    /// FInds all the gameobjects and stores them in variables for efficiency
    /// </summary>
    void Start () {
        canvas = GameObject.Find("Canvas");
        multiplayerWindow = AuxFunctions.FindObject(canvas, "MultiplayerPanel");
        addRobotWindow = AuxFunctions.FindObject(canvas, "AddRobotPanel");

        for (int i = 0; i < robotButtons.Length; i++)
        {
            robotButtons[i] = AuxFunctions.FindObject(canvas, "Robot" + (i + 1) + "Button");
        }

        simUI = StateMachine.Instance.gameObject.GetComponent<SimUI>();
        highlight = AuxFunctions.FindObject(canvas, "HighlightActiveRobot");
    }
	
	/// <summary>
    /// Since main state is not initialized immediately (after a frame or two), we look for it in the update function which is called every step
    /// </summary>
	void Update () {
        if (mainState == null)
        {
            mainState = ((MainState)StateMachine.Instance.CurrentState);
        }
    }
    
    /// <summary>
    /// Updates the multiplayer window after enabling it
    /// </summary>
    private void OnEnable()
    {
        if (mainState != null)
        {
            UpdateUI();
        }
    }

    /// <summary>
    /// Toggles the multiplayer window
    /// </summary>
    public void ToggleMultiplayerWindow()
    {
        if (multiplayerWindow.activeSelf)
        {
            multiplayerWindow.SetActive(false);
        }
        else
        {
            simUI.EndOtherProcesses();
            multiplayerWindow.SetActive(true);
        }
    }

    /// <summary>
    /// Changes which robot is currently the active robot
    /// </summary>
    /// <param name="index">the index of the new active robot</param>
    public void ChangeActiveRobot(int index)
    {
        if (index < mainState.SpawnedRobots.Count)
        {
            mainState.SwitchActiveRobot(index);
            activeIndex = index;
            UpdateUI();

            GetComponent<DriverPracticeMode>().ChangeActiveRobot(index);
        }


    }

    /// <summary>
    /// Adds a new robot to the field based on user selection in the popup robot list window
    /// </summary>
    public void AddRobot()
    {
        GameObject panel = GameObject.Find("RobotListPanel");
        string directory = PlayerPrefs.GetString("RobotDirectory") + "\\" + panel.GetComponent<ChangeRobotScrollable>().selectedEntry;
        if (Directory.Exists(directory))
        {
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            mainState.LoadRobot(directory);
        }
        else
        {
            UserMessageManager.Dispatch("Robot directory not found!", 5);
        }
        ToggleAddRobotWindow();
        UpdateUI();
    }

    /// <summary>
    /// Removes a robot from the field and shifts the indexes to remove any gaps
    /// </summary>
    public void RemoveRobot()
    {
        mainState.RemoveRobot(activeIndex);
        activeIndex = mainState.SpawnedRobots.IndexOf(mainState.activeRobot);
        GetComponent<DriverPracticeMode>().ChangeActiveRobot(activeIndex);
        UpdateUI();
    }

    /// <summary>
    /// Toggles the popup add robot window
    /// </summary>
    public void ToggleAddRobotWindow()
    {
        if (addRobotWindow.activeSelf)
        {
            addRobotWindow.SetActive(false);
        }
        else
        {
            simUI.EndOtherProcesses();
            addRobotWindow.SetActive(true);
        }
    }   

    /// <summary>
    /// Updates the multiplayer window to reflect changes in indexes, controls, etc.
    /// </summary>
    private void UpdateUI()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i < mainState.SpawnedRobots.Count)
            {
                //robotButtons[i].GetComponent<Image>().color = new Color(1,1,1,1);
                robotButtons[i].GetComponent<Button>().interactable = true;
                robotButtons[i].GetComponentInChildren<Text>().color = new Color(1, 1, 1, 1);
            }
            else
            {
                //robotButtons[i].GetComponent<Image>().color = new Color(.8f, .8f, .8f, .5f);
                robotButtons[i].GetComponent<Button>().interactable = false;
                robotButtons[i].GetComponentInChildren<Text>().color = new Color(.8f, .8f, .8f, .5f);
            }
        }

        highlight.transform.position = robotButtons[activeIndex].transform.position;

        GameObject.Find("ActiveRobotText").GetComponent<Text>().text = "Robot: " + mainState.SpawnedRobots[activeIndex].RobotName;
        GameObject.Find("ControlIndexText").GetComponent<Text>().text = "Control Index: " + (mainState.SpawnedRobots[activeIndex].controlIndex + 1);
    }

    /// <summary>
    /// Permanently hides the multiplayer tooltip
    /// </summary>
    public void HideTooltip()
    {
        GameObject.Find("MultiplayerTooltip").SetActive(false);
    }
}
