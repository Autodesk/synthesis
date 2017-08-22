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

    private SimUI simUI;

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

        simUI = StateMachine.Instance.gameObject.GetComponent<SimUI>();
    }

    public void ToggleMaMPanel()
    {
        if (mixAndMatchPanel.activeSelf)
        {
            mixAndMatchPanel.SetActive(false);
        }
        else
        {
            //simUI.EndOtherProcesses();
            mixAndMatchPanel.SetActive(true);
        }
    }
}
