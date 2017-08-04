﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Assets.Scripts.FSM;
using System.IO;

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

    GameObject canvas;

    GameObject freeroamCameraWindow;
    GameObject spawnpointWindow;

    GameObject robotCameraList;
    GameObject robotCameraIndicator;
    GameObject showCameraButton;
    GameObject configureRobotCameraButton;
    GameObject cameraConfigurationModeButton;
    GameObject changeCameraNodeButton;
    GameObject configureCameraPanel;
    GameObject cancelNodeSelectionButton;

    GameObject changeRobotPanel;
    GameObject changeFieldPanel;
    GameObject addRobotPanel;

    GameObject driverStationPanel;

    GameObject inputManagerPanel;

    GameObject exitPanel;

    GameObject orientWindow;
    bool isOrienting = false;
    GameObject resetDropdown;

    Text cameraNodeText;

    GameObject loadingPanel;

    private bool freeroamWindowClosed = false;

    private bool oppositeSide = false;

    /// <summary>
    /// Retreives the Main State instance which controls everything in the simulator.
    /// </summary>
    void Start()
    {
    }

    private void Update()
    {
        if (main == null)
        {
            main = transform.GetComponent<StateMachine>().CurrentState as MainState;
            //Get the render texture from Resources/Images
            toolkit = GetComponent<Toolkit>();
        }
        else if (dpm == null)
        {
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

            toolkit = GetComponent<Toolkit>();
            dpm = GetComponent<DriverPracticeMode>();
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

        addRobotPanel = AuxFunctions.FindObject("MultiplayerPanel");



        driverStationPanel = AuxFunctions.FindObject(canvas, "DriverStationPanel");
        changeRobotPanel = AuxFunctions.FindObject(canvas, "ChangeRobotPanel");
        changeFieldPanel = AuxFunctions.FindObject(canvas, "ChangeFieldPanel");

        driverStationPanel = AuxFunctions.FindObject(canvas, "DriverStationPanel");

        inputManagerPanel = AuxFunctions.FindObject(canvas, "InputManagerPanel");

        orientWindow = AuxFunctions.FindObject(canvas, "OrientWindow");
        resetDropdown = GameObject.Find("Reset Robot Dropdown");

        exitPanel = AuxFunctions.FindObject(canvas, "ExitPanel");
        loadingPanel = AuxFunctions.FindObject(canvas, "LoadingPanel");

    }



    private void UpdateWindows()
    {
        if (main != null)
            UpdateFreeroamWindow();
        UpdateSpawnpointWindow();
        UpdateDriverStationPanel();
    }


    #region main button functions
    /// <summary>
    /// Resets the robot
    /// </summary>
    //public void PressReset()
    //{
    //    main.ResetRobot();
    //}
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
            main.ChangeRobot(directory);
        }
        else
        {
            UserMessageManager.Dispatch("Robot directory not found!", 5);
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
            Application.LoadLevel("Scene");
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

    /// <summary>
    /// Call this function whenever the user enters a new state (ex. selecting a new robot, using ruler function, orenting robot)
    /// </summary>
    public void EndOtherProcesses()
    {
        changeFieldPanel.SetActive(false);
        changeRobotPanel.SetActive(false);
        exitPanel.SetActive(false);
        CloseOrientWindow();
        main.IsResetting = false;   

        dpm.EndProcesses();
        toolkit.EndProcesses();
    }
    #endregion
    #region camera button functions
    //Camera Functions
    public void SwitchCameraFreeroam()
    {
        camera.SwitchCameraState(0);
    }

    public void SwitchCameraOrbit()
    {
        camera.SwitchCameraState(1);
    }

    public void SwitchCameraDriverStation()
    {
        camera.SwitchCameraState(2);
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

    /// <summary>
    /// Pop reset instructions when main is in reset spawnpoint mode
    /// </summary>
    private void UpdateSpawnpointWindow()
    {
        if (main.IsResetting)
        {
            spawnpointWindow.SetActive(true);
        }
        else
        {
            spawnpointWindow.SetActive(false);
        }
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


    public void CloseFreeroamWindow()
    {
        freeroamCameraWindow.SetActive(false);
        freeroamWindowClosed = true;
    }

    /// <summary>
    /// Activate driver station panel if the main camera is in driver station state
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
}
