﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Assets.Scripts.FSM;
using System.IO;
using UnityEngine.SceneManagement;

/// <summary>
/// SimUI serves as an interface between the Unity button UI and the various functions within the simulator.
/// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
/// </summary>
public class SimUI : MonoBehaviour
{
    MainState main;
    DynamicCamera camera;
    Toolkit toolkit;
    DriverPracticeMode dpm;
    LocalMultiplayer multiplayer;
    SensorManagerGUI sensorManagerGUI;
    SensorManager sensorManager;
    RobotCameraManager robotCameraManager;
    RobotCameraGUI robotCameraGUI;

    GameObject canvas;

    GameObject freeroamCameraWindow;
    GameObject spawnpointWindow;

    GameObject swapWindow;

    GameObject wheelPanel;
    GameObject driveBasePanel;
    GameObject manipulatorPanel;

    GameObject changeRobotPanel;
    GameObject robotListPanel;
    GameObject changeFieldPanel;
    GameObject addRobotPanel;

    GameObject driverStationPanel;

    GameObject inputManagerPanel;
    GameObject unitConversionSwitch;

    GameObject mixAndMatchPanel;

    public bool swapWindowOn = false; //if the swap window is active
    public bool wheelPanelOn = false; //if the wheel panel is active
    public bool driveBasePanelOn = false; //if the drive base panel is active
    public bool manipulatorPanelOn = false; //if the manipulator panel is active

    GameObject exitPanel;

    GameObject orientWindow;
    bool isOrienting = false;
    GameObject resetDropdown;

    GameObject loadingPanel;

    private bool freeroamWindowClosed = false;

    private bool oppositeSide = false;

    private void Update()
    {
        if (main == null)
        {
            main = transform.GetComponent<StateMachine>().CurrentState as MainState;
        }
        else if (dpm == null)
        {
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

            toolkit = GetComponent<Toolkit>();
            dpm = GetComponent<DriverPracticeMode>();
            multiplayer = GetComponent<LocalMultiplayer>();
            sensorManagerGUI = GetComponent<SensorManagerGUI>();

            FindElements();
        }
        else if (camera == null)
        {
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
        }
        else
        {
            UpdateWindows();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (StateMachine.Instance.CurrentState.GetType().Equals(typeof(MainState)))
                {
                    if (!exitPanel.activeSelf) MainMenuExit("open");
                    else MainMenuExit("cancel");
                }
            }

        }
    }

    private void OnGUI()
    {
        UserMessageManager.Render();
    }

    /// <summary>
    /// Finds all the necessary UI elements that need to be updated/modified
    /// </summary>
    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        freeroamCameraWindow = AuxFunctions.FindObject(canvas, "FreeroamPanel");
        spawnpointWindow = AuxFunctions.FindObject(canvas, "SpawnpointPanel");

        swapWindow = AuxFunctions.FindObject(canvas, "SwapPanel");
        wheelPanel = AuxFunctions.FindObject(canvas, "WheelPanel");
        driveBasePanel = AuxFunctions.FindObject(canvas, "DriveBasePanel");
        manipulatorPanel = AuxFunctions.FindObject(canvas, "ManipulatorPanel");

        addRobotPanel = AuxFunctions.FindObject("MultiplayerPanel");

        driverStationPanel = AuxFunctions.FindObject(canvas, "DriverStationPanel");
        changeRobotPanel = AuxFunctions.FindObject(canvas, "ChangeRobotPanel");
        robotListPanel = AuxFunctions.FindObject(changeRobotPanel, "RobotListPanel");

        changeFieldPanel = AuxFunctions.FindObject(canvas, "ChangeFieldPanel");

        inputManagerPanel = AuxFunctions.FindObject(canvas, "InputManagerPanel");
        unitConversionSwitch = AuxFunctions.FindObject(canvas, "UnitConversionSwitch");

        orientWindow = AuxFunctions.FindObject(canvas, "OrientWindow");
        resetDropdown = GameObject.Find("Reset Robot Dropdown");

        exitPanel = AuxFunctions.FindObject(canvas, "ExitPanel");
        loadingPanel = AuxFunctions.FindObject(canvas, "LoadingPanel");

        sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
        robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();
        robotCameraGUI = GameObject.Find("StateMachine").GetComponent<RobotCameraGUI>();
        mixAndMatchPanel = AuxFunctions.FindObject(canvas, "MixAndMatchPanel");
    }
    
    private void UpdateWindows()
    {
        if (main != null)
            UpdateFreeroamWindow();
        UpdateSpawnpointWindow();
        UpdateDriverStationPanel();
    }
    
    #region change robot/field functions

    public void SetIsMixAndMatch (bool isMixAndMatch)
    {
        if (isMixAndMatch)
        {
            PlayerPrefs.SetInt("mixAndMatch", 1); //0 is false, 1 is true
        } else
        {
            PlayerPrefs.SetInt("mixAndMatch", 0);
        }
    }
    public void ChangeRobot()
    {
        GameObject panel = GameObject.Find("RobotListPanel");
        string directory = PlayerPrefs.GetString("RobotDirectory") + "\\" + panel.GetComponent<ChangeRobotScrollable>().selectedEntry;
        if (Directory.Exists(directory))
        {
            panel.SetActive(false);
            changeRobotPanel.SetActive(false);
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            PlayerPrefs.SetString("simSelectedRobot", directory);
            PlayerPrefs.SetString("simSelectedRobotName", panel.GetComponent<ChangeRobotScrollable>().selectedEntry);
            PlayerPrefs.Save();

            robotCameraManager.DetachCamerasFromRobot(main.activeRobot);
            sensorManager.RemoveSensorsFromRobot(main.activeRobot);

            main.ChangeRobot(directory);
        }
        else
        {
            UserMessageManager.Dispatch("Robot directory not found!", 5);
        }
    }

    /// <summary>
    /// Changes the drive base, destroys old manipulator and creates new manipulator, sets wheels
    /// </summary>
    public void MaMChangeRobot(string robotDirectory, string manipulatorDirectory, int robotHasManipulator)
    {
        robotCameraManager.DetachCamerasFromRobot(main.activeRobot);
        sensorManager.RemoveSensorsFromRobot(main.activeRobot);

        main.ChangeRobot(robotDirectory);

        //If the current robot has a manipulator, destroy the manipulator
        if (robotHasManipulator == 1) //0 is false, 1 is true
        {
            main.DeleteManipulatorNodes();

        }

        //If the new robot has a manipulator, load the manipulator
        int newRobotHasManipulator = PlayerPrefs.GetInt("hasManipulator");
        if (newRobotHasManipulator == 1) //0 is false, 1 is true
        {
            main.LoadManipulator(manipulatorDirectory, main.activeRobot.gameObject);
        } else
        {
            main.activeRobot.robotHasManipulator = 0; 
        }
    }

    public void ToggleChangeRobotPanel()
    {
        if (changeRobotPanel.activeSelf)
        {
            changeRobotPanel.SetActive(false);
        }
        else
        {
            EndOtherProcesses();
            changeRobotPanel.SetActive(true);
            robotListPanel.SetActive(true);
        }
    }

    public void ChangeField()
    {
        GameObject panel = GameObject.Find("FieldListPanel");
        string directory = PlayerPrefs.GetString("FieldDirectory") + "\\" + panel.GetComponent<ChangeFieldScrollable>().selectedEntry;
        if (Directory.Exists(directory))
        {
            panel.SetActive(false);
            changeFieldPanel.SetActive(false);
            loadingPanel.SetActive(true);
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            PlayerPrefs.SetString("simSelectedField", directory);
            PlayerPrefs.SetString("simSelectedFieldName", panel.GetComponent<ChangeFieldScrollable>().selectedEntry);
            PlayerPrefs.Save();

            int isMixAndMatch = PlayerPrefs.GetInt("mixAndMatch"); //0 is false, 1 is true
            if (isMixAndMatch == 1)
            {
                SceneManager.LoadScene("MixAndMatch");
            } else
            {
                SceneManager.LoadScene("Scene");
            }
            
        }
        else
        {
            UserMessageManager.Dispatch("Field directory not found!", 5);
        }
    }

    public void ToggleChangeFieldPanel()
    {
        if (changeFieldPanel.activeSelf)
        {
            changeFieldPanel.SetActive(false);
        }
        else
        {
            EndOtherProcesses();
            changeFieldPanel.SetActive(true);
        }

    }
    
    #endregion
    #region camera button functions
    /// <summary>
    /// Toggles between different dynamic camera states
    /// </summary>
    /// <param name="joe"></param>
    public void SwitchCameraView(int joe)
    {
        //Debug.Log(joe);
        switch (joe)
        {
            case 1:
                camera.SwitchCameraState(new DynamicCamera.DriverStationState(camera));
                DynamicCamera.MovingEnabled = true;
                break;
            case 2:
                camera.SwitchCameraState(new DynamicCamera.OrbitState(camera));
                DynamicCamera.MovingEnabled = true;
                break;
            case 3:
                camera.SwitchCameraState(new DynamicCamera.FreeroamState(camera));
                DynamicCamera.MovingEnabled = true;
                break;
            case 4:
                camera.SwitchCameraState(new DynamicCamera.OverviewState(camera));
                DynamicCamera.MovingEnabled = true;
                break;
        }
    }

    /// <summary>
    /// Change camera tool tips
    /// </summary>
    public void CameraToolTips()
    {
        if (camera.cameraState.GetType().Equals(typeof(DynamicCamera.DriverStationState)))
            camera.GetComponent<Text>().text = "Driver Station";
        else if (camera.cameraState.GetType().Equals(typeof(DynamicCamera.FreeroamState)))
            camera.GetComponent<Text>().text = "Freeroam";
        else if (camera.cameraState.GetType().Equals(typeof(DynamicCamera.OrbitState)))
            camera.GetComponent<Text>().text = "Orbit Robot";
        else if (camera.cameraState.GetType().Equals(typeof(DynamicCamera.OverviewState)))
            camera.GetComponent<Text>().text = "Overview";
    }

    /// <summary>
    /// Pop freeroam instructions when using freeroam camera, won't show up again if the user closes it
    /// </summary>
    private void UpdateFreeroamWindow()
    {
        if (camera.cameraState.GetType().Equals(typeof(DynamicCamera.FreeroamState)) && !freeroamWindowClosed)
        {
            if (!freeroamWindowClosed)
            {
                freeroamCameraWindow.SetActive(true);
            }

        }
        else if (!camera.cameraState.GetType().Equals(typeof(DynamicCamera.FreeroamState)))
        {
            freeroamCameraWindow.SetActive(false);
        }
    }

    /// <summary>
    /// Close freeroam camera tool tip
    /// </summary>
    public void CloseFreeroamWindow()
    {
        freeroamCameraWindow.SetActive(false);
        freeroamWindowClosed = true;
    }


    /// <summary>
    /// Activate driver station tool tips if the main camera is in driver station state
    /// </summary>
    private void UpdateDriverStationPanel()
    {
        driverStationPanel.SetActive(camera.cameraState.GetType().Equals(typeof(DynamicCamera.DriverStationState)));
    }

    /// <summary>
    /// Change to driver station view to the opposite side
    /// </summary>
    public void ToggleDriverStation()
    {
        oppositeSide = !oppositeSide;
        camera.SwitchCameraState(new DynamicCamera.DriverStationState(camera, oppositeSide));
    }
    #endregion
    #region orient button functions

    public void ToggleOrientWindow()
    {
        if (isOrienting)
        {
            isOrienting = false;
            main.EndRobotReset();
        }
        else
        {
            EndOtherProcesses();
            isOrienting = true;
            main.BeginRobotReset();
        }
        orientWindow.SetActive(isOrienting);
    }

    public void OrientLeft()
    {
        main.RotateRobot(new Vector3(Mathf.PI * 0.25f, 0f, 0f));
    }
    public void OrientRight()
    {
        main.RotateRobot(new Vector3(-Mathf.PI * 0.25f, 0f, 0f));
    }
    public void OrientForward()
    {
        main.RotateRobot(new Vector3(0f, 0f, Mathf.PI * 0.25f));
    }
    public void OrientBackward()
    {
        main.RotateRobot(new Vector3(0f, 0f, -Mathf.PI * 0.25f));
    }

    public void DefaultOrientation()
    {
        main.ResetRobotOrientation();
        orientWindow.SetActive(isOrienting = false);
    }

    public void SaveOrientation()
    {
        main.SaveRobotOrientation();
        orientWindow.SetActive(isOrienting = false);
    }

    public void CloseOrientWindow()
    {
        isOrienting = false;
        orientWindow.SetActive(isOrienting);
        main.EndRobotReset();
    }

    #endregion
    #region control panel functions
    public void ShowControlPanel(bool show)
    {
        if (show)
        {
            EndOtherProcesses();
            inputManagerPanel.SetActive(true);
        }
        else
        {
            inputManagerPanel.SetActive(false);
        }
    }

    public void ShowControlPanel()
    {
        ShowControlPanel(!inputManagerPanel.activeSelf);
    }

    /// <summary>
    /// Open totorial link
    /// </summary>
    public void OpenTutorialLink()
    {
        Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
    }

    /// <summary>
    /// Toggles between meter and feet measurements
    /// </summary>
    public void ToggleUnitConversion()
    {
        int i = (int)unitConversionSwitch.GetComponent<Slider>().value;

        main.IsMetric = (i == 1 ? true : false);
    }

    #endregion
    #region reset functions
    /// <summary>
    /// Pop reset instructions when main is in reset spawnpoint mode
    /// </summary>
    private void UpdateSpawnpointWindow()
    {
        if (main.activeRobot.IsResetting)
        {
            spawnpointWindow.SetActive(true);
        }
        else
        {
            spawnpointWindow.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles between quick reset and reset spawnpoint
    /// </summary>
    /// <param name="i"></param>
    public void ChooseResetMode(int i)
    {
        switch (i)
        {
            case 1:
                main.BeginRobotReset();
                main.EndRobotReset();
                resetDropdown.GetComponent<Dropdown>().value = 0;
                break;
            case 2:
                EndOtherProcesses();
                main.IsResetting = true;
                main.BeginRobotReset();
                resetDropdown.GetComponent<Dropdown>().value = 0;
                break;
        }
    }
    #endregion

    /// <summary>
    /// Exit to main menu window
    /// </summary>
    /// <param name="option"></param>
    public void MainMenuExit(string option)
    {
        EndOtherProcesses();
        switch (option)
        {
            case "open":
                exitPanel.SetActive(true);
                break;
            case "exit":
                Application.LoadLevel("MainMenu");
                break;

            case "cancel":
                exitPanel.SetActive(false);
                break;
        }
    }


    /// <summary>
    /// Call this function whenever the user enters a new state (ex. selecting a new robot, using ruler function, orenting robot)
    /// </summary>
    public void EndOtherProcesses()
    {
        changeFieldPanel.SetActive(false);
        changeRobotPanel.SetActive(false);
        exitPanel.SetActive(false);
        mixAndMatchPanel.SetActive(false);

        CloseOrientWindow();
        main.IsResetting = false;

        dpm.EndProcesses();
        toolkit.EndProcesses();
        multiplayer.EndProcesses();
        sensorManagerGUI.EndProcesses();
        robotCameraGUI.EndProcesses();
    }

    /// <summary>
    /// Enters replay mode
    /// </summary>
    public void EnterReplayMode()
    {
        main.EnterReplayState();
    }
    #region swap part
    /// <summary>
    /// Toggles the Driver Practice Mode window
    /// </summary>
    public void SwapToggleWindow()
    {
        swapWindowOn = !swapWindowOn;
        swapWindow.SetActive(swapWindowOn);
    }

    public void TogglePanel(GameObject panel)
    {
        if (panel.activeSelf == true)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
    }

    public void PartToggleWindow(string Window)
    {
        List<GameObject> swapPanels = new List<GameObject> { wheelPanel, driveBasePanel, manipulatorPanel };
        switch (Window)
        {
            case "wheel":
                TogglePanel(wheelPanel);
                driveBasePanel.SetActive(false);
                manipulatorPanel.SetActive(false);
                break;
            case "driveBase":
                TogglePanel(driveBasePanel);
                wheelPanel.SetActive(false);
                manipulatorPanel.SetActive(false);
                break;
            case "manipulator":
                TogglePanel(manipulatorPanel);
                driveBasePanel.SetActive(false);
                wheelPanel.SetActive(false);
                break;
            default:
                wheelPanel.SetActive(false);
                driveBasePanel.SetActive(false);
                manipulatorPanel.SetActive(false);
                break;
        }
    }
    #endregion
}