using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.FSM;
using System.Linq;
using Synthesis.GUI;
using Synthesis.DriverPractice;
using Synthesis.GUI.Scrollables;
using Synthesis.States;
using Synthesis.Utils;
using Assets.Scripts.GUI;

/// <summary>
/// Class for controlling the various aspects of local multiplayer
/// </summary>
public class LocalMultiplayer : LinkedMonoBehaviour<MainState>
{

    private GameObject canvas;
    private SimUI simUI;

    private GameObject multiplayerWindow;
    private GameObject addRobotWindow;

    private GameObject mixAndMatchPanel;

    private GameObject[] robotButtons = new GameObject[6];
    private int activeIndex = 0;
    private GameObject highlight;

    /// <summary>
    /// FInds all the gameobjects and stores them in variables for efficiency
    /// </summary>
    private void Start()
    {
        canvas = GameObject.Find("Canvas");
        multiplayerWindow = Auxiliary.FindObject(canvas, "MultiplayerPanel");
        addRobotWindow = Auxiliary.FindObject(canvas, "AddRobotPanel");

        for (int i = 0; i < robotButtons.Length; i++)
        {
            robotButtons[i] = Auxiliary.FindObject(canvas, "Robot" + (i + 1) + "Button");
        }

        simUI = StateMachine.SceneGlobal.gameObject.GetComponent<SimUI>();
        highlight = Auxiliary.FindObject(canvas, "HighlightActiveRobot");
        mixAndMatchPanel = Auxiliary.FindObject(canvas, "MixAndMatchPanel");
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
            UpdateUI();
        }
    }

    /// <summary>
    /// Changes which robot is currently the active robot
    /// </summary>
    /// <param name="index">the index of the new active robot</param>
    public void ChangeActiveRobot(int index)
    {
        if (index < State.SpawnedRobots.Count)
        {
            State.SwitchActiveRobot(index);
            activeIndex = index;
            UpdateUI();

            State.SwitchActiveRobot(index);
        }
    }

    /// <summary>
    /// Adds a new robot to the field based on user selection in the popup robot list window
    /// </summary>
    public void AddRobot()
    {
        AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.AddRobot,
            AnalyticsLedger.EventAction.Clicked,
            "",
            AnalyticsLedger.getMilliseconds().ToString());

        GameObject panel = GameObject.Find("RobotListPanel");
        string directory = PlayerPrefs.GetString("RobotDirectory") + Path.DirectorySeparatorChar + panel.GetComponent<ChangeRobotScrollable>().selectedEntry;
        if (Directory.Exists(directory))
        {
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            State.LoadRobot(directory, false);
        }
        else
        {
            UserMessageManager.Dispatch("Robot directory not found!", 5);
        }
        ToggleAddRobotWindow();
        UpdateUI();

        PlayerPrefs.SetInt("hasManipulator", 0); //0 for false, 1 for true
    }

    /// <summary>
    /// Adds a new robot to the field based on user selection in the popup robot list window
    /// </summary>
    public void AddMaMRobot(string baseDirectory, string manipulatorDirectory, bool hasManipulator)
    {
        if (hasManipulator)
            State.LoadRobotWithManipulator(baseDirectory, manipulatorDirectory);
        else
            State.LoadRobot(baseDirectory, true);

        UpdateUI();
    }

    /// <summary>
    /// Removes a robot from the field and shifts the indexes to remove any gaps
    /// </summary>
    public void RemoveRobot()
    {
        if (State.SpawnedRobots.Count > 1)
        {
            State.RemoveRobot(activeIndex);
            activeIndex = State.SpawnedRobots.IndexOf(State.ActiveRobot);
            State.SwitchActiveRobot(activeIndex);
            UpdateUI();
        }
        else UserMessageManager.Dispatch("Cannot Delete. Must Have At Least One Robot on Field.", 5);
    }

    /// <summary>
    /// Toggles the popup add robot window
    /// </summary>
    public void ToggleAddRobotWindow()
    {
        if (addRobotWindow.activeSelf)
        {
            addRobotWindow.SetActive(false);
            DynamicCamera.ControlEnabled = true;
        }
        else
        {
            addRobotWindow.SetActive(true);
        }
    }

    /// <summary>
    /// Updates the multiplayer window to reflect changes in indexes, controls, etc.
    /// </summary>
    public void UpdateUI()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i < State.SpawnedRobots.Count)
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

        Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("ActiveRobotText")).First().GetComponent<Text>().text = "Robot: " + State.SpawnedRobots[activeIndex].RobotName;
        Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("ControlIndexDropdown")).First().GetComponent<Dropdown>().value = State.ActiveRobot.ControlIndex;
        // GameObject.Find("ActiveRobotText").GetComponent<Text>().text = "Robot: " + mainState.SpawnedRobots[activeIndex].RobotName;
        //GameObject.Find("ControlIndexDropdown").GetComponent<Dropdown>().value = mainState.activeRobot.controlIndex;
    }

    /// <summary>
    /// Permanently hides the multiplayer tooltip
    /// </summary>
    public void HideTooltip()
    {
        Auxiliary.FindObject(multiplayerWindow, "MultiplayerTooltip").SetActive(false);
    }

    /// <summary>
    /// Changes the control index of the active robot
    /// </summary>
    public void ChangeControlIndex(int index)
    {
        State.ChangeControlIndex(index);
        UpdateUI();
    }

    public void EndProcesses()
    {
        if (multiplayerWindow.activeSelf) multiplayerWindow.SetActive(false);
    }
}