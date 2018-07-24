using Synthesis.FSM;
using Synthesis.DriverPractice;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Synthesis.GUI;
using UnityEngine.Analytics;
using Synthesis.Input;
using Synthesis.Sensors;
using Synthesis.Camera;

namespace Assets.Scripts.GUI
{
    public class MainToolbarState : State
    {
        GameObject canvas;

        DynamicCamera camera;
        DriverPracticeMode dpm;
        MainState State;
        SensorManagerGUI sensorManagerGUI;
        RobotCameraGUI robotCameraGUI;
        Toolkit toolkit;
        LocalMultiplayer multiplayer;
        SimUI simUI;

        GameObject changeRobotPanel;
        GameObject robotListPanel;
        GameObject changePanel;
        GameObject addPanel;
        GameObject changeFieldPanel;
        GameObject resetDropdown;
        GameObject dpmPanel;
        GameObject toolkitPanel;
        GameObject multiplayerPanel;
        GameObject stopwatchWindow;
        GameObject statsWindow;
        GameObject rulerWindow;
        GameObject inputManagerPanel;
        GameObject bindedKeyPanel;
        GameObject checkSavePanel;

        public bool dpmWindowOn = false; //if the driver practice mode window is active
        public static bool inputPanelOn = false;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

            changeRobotPanel = Auxiliary.FindObject(canvas, "ChangeRobotPanel");
            robotListPanel = Auxiliary.FindObject(changeRobotPanel, "RobotListPanel");
            changePanel = Auxiliary.FindObject(canvas, "ChangePanel");
            addPanel = Auxiliary.FindObject(canvas, "AddPanel");
            changeFieldPanel = Auxiliary.FindObject(canvas, "ChangeFieldPanel");

            resetDropdown = GameObject.Find("ResetRobotDropdown");
            dpmPanel = Auxiliary.FindObject(canvas, "DPMPanel"); // going to be moved to its own state
            multiplayerPanel = Auxiliary.FindObject(canvas, "MultiplayerPanel");

            toolkitPanel = Auxiliary.FindObject(canvas, "ToolkitPanel");
            stopwatchWindow = Auxiliary.FindObject(canvas, "StopwatchPanel");
            statsWindow = Auxiliary.FindObject(canvas, "StatsPanel");
            rulerWindow = Auxiliary.FindObject(canvas, "RulerPanel");

            inputManagerPanel = Auxiliary.FindObject(canvas, "InputManagerPanel");
            bindedKeyPanel = Auxiliary.FindObject(canvas, "BindedKeyPanel");
            checkSavePanel = Auxiliary.FindObject(canvas, "CheckSavePanel");

            dpm = StateMachine.SceneGlobal.GetComponent<DriverPracticeMode>();
            toolkit = StateMachine.SceneGlobal.GetComponent<Toolkit>();
            multiplayer = StateMachine.SceneGlobal.GetComponent<LocalMultiplayer>();
            simUI = StateMachine.SceneGlobal.GetComponent<SimUI>();
            robotCameraGUI = StateMachine.SceneGlobal.GetComponent<RobotCameraGUI>();
            sensorManagerGUI = StateMachine.SceneGlobal.GetComponent<SensorManagerGUI>();

            State = StateMachine.SceneGlobal.CurrentState as MainState;
        }

        public void OnChangeRobotButtonPressed()
        {
            if (changePanel.activeSelf == true)
            {
                changePanel.SetActive(false);
            }
            else
            {
                changePanel.SetActive(true);
                addPanel.SetActive(false);
            }
        }

        public void OnResetRobotDropdownClicked(int i)
        {
            switch (i)
            {
                case 1:
                    State.BeginRobotReset();
                    State.EndRobotReset();
                    resetDropdown.GetComponent<Dropdown>().value = 0;
                    break;
                case 2:
                    EndOtherProcesses();
                    camera.SwitchCameraState(new DynamicCamera.OrbitState(camera));
                    DynamicCamera.ControlEnabled = true;
                    State.BeginRobotReset();
                    resetDropdown.GetComponent<Dropdown>().value = 0;
                    break;
                case 3:
                    Auxiliary.FindObject(GameObject.Find("ResetRobot"), "Dropdown List").SetActive(false);
                    Auxiliary.FindObject(GameObject.Find("Canvas"), "LoadingPanel").SetActive(true);
                    SceneManager.LoadScene("Scene");
                    resetDropdown.GetComponent<Dropdown>().value = 0;
                    break;
            }
        }

        /// <summary>
        /// Toggles between different dynamic camera states
        /// </summary>
        /// <param name="mode"></param>
        public void OnCameraDropdownClicked(int mode)
        {
            switch (mode)
            {
                case 1:
                    camera.SwitchCameraState(new DynamicCamera.DriverStationState(camera));
                    DynamicCamera.ControlEnabled = true;
                    break;
                case 2:
                    camera.SwitchCameraState(new DynamicCamera.OrbitState(camera));
                    DynamicCamera.ControlEnabled = true;
                    break;
                case 3:
                    camera.SwitchCameraState(new DynamicCamera.FreeroamState(camera));
                    DynamicCamera.ControlEnabled = true;
                    break;
                case 4:
                    camera.SwitchCameraState(new DynamicCamera.OverviewState(camera));
                    DynamicCamera.ControlEnabled = true;
                    break;
            }
        }

        // TODO: Add the reset robot dropdown and camera dropdown

        /// <summary>
        /// Toggles the Driver Practice Mode window
        /// </summary>
        public void OnDriverPracticeButtonPressed()
        {
            if (dpmWindowOn)
            {
                dpmWindowOn = false;
            }
            else
            {
                EndOtherProcesses();
                dpmWindowOn = true;
            }
            dpmPanel.SetActive(dpmWindowOn);
        }

        public void OnChangeFieldButtonPressed()
        {
            if (changeFieldPanel.activeSelf)
            {
                changeFieldPanel.SetActive(false);
                DynamicCamera.ControlEnabled = true;
            }
            else
            {
                EndOtherProcesses();
                changeFieldPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Enters replay mode
        /// </summary>
        public void OnReplayModeButtonPressed()
        {
            State.EnterReplayState();
        }

        /// <summary>
        /// Toggle the toolkit window on/off according to its current state
        /// </summary>
        public void OnToolkitButtonPressed()
        {
            toolkit.ToggleToolkitWindow(!toolkitPanel.activeSelf);
        }

        /// <summary>
        /// Toggles the multiplayer window
        /// </summary>
        public void OnMultiplayerButtonPressed()
        {
            if (multiplayerPanel.activeSelf)
            {
                multiplayerPanel.SetActive(false);
            }
            else
            {
                EndOtherProcesses();
                multiplayerPanel.SetActive(true);
                multiplayer.UpdateUI();
            }
        }

        /// <summary>
        /// Toggle the stopwatch window on/off according to its current state
        /// </summary>
        public void OnStopwatchPressed()
        {
            toolkit.ToggleStopwatchWindow(!stopwatchWindow.activeSelf);
        }

        /// <summary>
        /// Toggle the toolkit window on/off according to its current state
        /// </summary>
        public void OnStatsPressed()
        {
            toolkit.ToggleStatsWindow(!statsWindow.activeSelf);
        }

        /// <summary>
        /// Toggle the ruler window on/off according to its current state
        /// </summary>
        public void OnRulerPressed()
        {
            toolkit.ToggleRulerWindow(!rulerWindow.activeSelf);
        }

        /// <summary>
        /// Toggle the control panel ON/OFF based on its current state
        /// </summary>
        public void OnInfoButtonPressed()
        {
            simUI.ShowControlPanel(!inputManagerPanel.activeSelf);
        }

        /// <summary>
        /// Call this function whenever the user enters a new state (ex. selecting a new robot, using ruler function, orenting robot)
        /// </summary>
        public void EndOtherProcesses()
        {
            changeFieldPanel.SetActive(false);
            changeRobotPanel.SetActive(false);
            changePanel.SetActive(false);
            addPanel.SetActive(false);
            inputManagerPanel.SetActive(false);

            simUI.CancelOrientation();

            dpm.EndProcesses();
            toolkit.EndProcesses();
            multiplayer.EndProcesses();
            sensorManagerGUI.EndProcesses();
            robotCameraGUI.EndProcesses();
        }
    }
}