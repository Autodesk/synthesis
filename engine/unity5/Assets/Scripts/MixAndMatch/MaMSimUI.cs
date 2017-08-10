using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Assets.Scripts.FSM;
using System.IO;

public class MaMSimUI : MonoBehaviour {
    MainState main;
    GameObject canvas;


    GameObject mixAndMatchPanel;

    GameObject wheelPanel;
    GameObject driveBasePanel;
    GameObject manipulatorPanel;

    GameObject changeRobotPanel;
    GameObject addRobotPanel;

    GameObject driverStationPanel;

    GameObject inputManagerPanel;
    GameObject unitConversionButton;

    public bool swapWindowOn = false; //if the swap window is active
    public bool wheelPanelOn = false; //if the wheel panel is active
    public bool driveBasePanelOn = false; //if the drive base panel is active
    public bool manipulatorPanelOn = false; //if the manipulator panel is active

    GameObject exitPanel;

    Text cameraNodeText;

    GameObject loadingPanel;



    /// <summary>
    /// Retreives the Main State instance which controls everything in the simulator.
    /// </summary>
    void Start()
    {
        FindElements();
    }

    private void Update()
    {
        //if (main == null)
        //{
        //    main = transform.GetComponent<StateMachine>().CurrentState as MainState;
        //}
        //else
        //{
        //    UpdateWindows();

        //    if (Input.GetKeyDown(KeyCode.Escape))
        //    {
        //        if (StateMachine.Instance.CurrentState.GetType().Equals(typeof(MainState)))
        //        {
        //            if (!exitPanel.activeSelf) MainMenuExit("open");
        //            else MainMenuExit("cancel");
        //        }
        //    }

        //}

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

        mixAndMatchPanel = AuxFunctions.FindObject(canvas, "MixAndMatchPanel");
        wheelPanel = AuxFunctions.FindObject(canvas, "WheelPanel");
        driveBasePanel = AuxFunctions.FindObject(canvas, "DriveBasePanel");
        manipulatorPanel = AuxFunctions.FindObject(canvas, "ManipulatorPanel");

        addRobotPanel = AuxFunctions.FindObject("MultiplayerPanel");


        changeRobotPanel = AuxFunctions.FindObject(canvas, "ChangeRobotPanel");


        inputManagerPanel = AuxFunctions.FindObject(canvas, "InputManagerPanel");

        exitPanel = AuxFunctions.FindObject(canvas, "ExitPanel");
        loadingPanel = AuxFunctions.FindObject(canvas, "LoadingPanel");
        
    }



    //private void UpdateWindows()
    //{
    //    if (main != null)
    //        UpdateFreeroamWindow();
    //    UpdateSpawnpointWindow();
    //    UpdateDriverStationPanel();
    //}


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

    public void ToggleMaMPanel()
    {
        if (mixAndMatchPanel.activeSelf)
        {
            mixAndMatchPanel.SetActive(false);
        }
        else
        {
            //EndOtherProcesses();
            mixAndMatchPanel.SetActive(true);
        }
    }
}
#endregion
