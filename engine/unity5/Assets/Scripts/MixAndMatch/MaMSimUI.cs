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
