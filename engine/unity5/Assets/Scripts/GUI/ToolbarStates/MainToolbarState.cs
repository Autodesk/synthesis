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
using Synthesis.Field;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// The MainToolbarState controls the functions of each of the main toolbar functions such as 
    /// change robots/fields, reset, camera views, etc. 
    /// 
    /// Toolbar state composition
    /// -Button/Dropdowns for toolbar functions
    /// -Help menu
    /// </summary>
    public class MainToolbarState : State
    {
        GameObject canvas;

        DynamicCamera camera;
        MainState State;
        SensorManagerGUI sensorManagerGUI;
        RobotCameraGUI robotCameraGUI;
        Toolkit toolkit;
        LocalMultiplayer multiplayer;
        SimUI simUI;
        Panel panel;

        GameObject changeRobotPanel;
        GameObject robotListPanel;
        GameObject changePanel;
        GameObject addPanel;
        GameObject changeFieldPanel;
        GameObject resetDropdown;
        GameObject multiplayerPanel;
        GameObject stopwatchWindow;
        GameObject statsWindow;
        GameObject rulerWindow;
        GameObject inputManagerPanel;
        GameObject bindedKeyPanel;
        GameObject checkSavePanel;
        GameObject helpMenu;
        GameObject toolbar;
        GameObject overlay;
        GameObject tabs;

        Text helpBodyText;

        public bool dpmWindowOn = false; //if the driver practice mode window is active
        public static bool inputPanelOn = false;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

            tabs = Auxiliary.FindObject(canvas, "Tabs");
            toolbar = Auxiliary.FindObject(canvas, "MainToolbar");
            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            changeRobotPanel = Auxiliary.FindObject(canvas, "ChangeRobotPanel");
            robotListPanel = Auxiliary.FindObject(changeRobotPanel, "RobotListPanel");
            changePanel = Auxiliary.FindObject(canvas, "ChangePanel");
            addPanel = Auxiliary.FindObject(canvas, "AddPanel");
            changeFieldPanel = Auxiliary.FindObject(canvas, "ChangeFieldPanel");

            resetDropdown = GameObject.Find("ResetRobotDropdown");
            multiplayerPanel = Auxiliary.FindObject(canvas, "MultiplayerPanel");

            stopwatchWindow = Auxiliary.FindObject(canvas, "StopwatchPanel");
            statsWindow = Auxiliary.FindObject(canvas, "StatsPanel");
            rulerWindow = Auxiliary.FindObject(canvas, "RulerPanel");

            inputManagerPanel = Auxiliary.FindObject(canvas, "InputManagerPanel");
            bindedKeyPanel = Auxiliary.FindObject(canvas, "BindedKeyPanel");
            checkSavePanel = Auxiliary.FindObject(canvas, "CheckSavePanel");

            // To access instatiate classes within a state, use the StateMachine.SceneGlobal
            toolkit = StateMachine.SceneGlobal.GetComponent<Toolkit>();
            multiplayer = StateMachine.SceneGlobal.GetComponent<LocalMultiplayer>();
            simUI = StateMachine.SceneGlobal.GetComponent<SimUI>();
            robotCameraGUI = StateMachine.SceneGlobal.GetComponent<RobotCameraGUI>();
            sensorManagerGUI = StateMachine.SceneGlobal.GetComponent<SensorManagerGUI>();
            panel = StateMachine.SceneGlobal.GetComponent<Panel>();

            State = StateMachine.SceneGlobal.CurrentState as MainState;

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);
        }

        /// <summary>
        /// Change robot button callback. Note: Buttons register with "On...Pressed"
        /// </summary>
        public void OnChangeRobotButtonPressed()
        {
            panel.OpenOnly(changePanel);
            panel.Freeze(true);
        }

        /// <summary>
        /// Reset robot dropdown callback. Note: Dropdowns register with "On...Clicked"
        /// naming conventions.
        /// </summary>
        /// <param name="i"></param>
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
                    GameObject.Destroy(GameObject.Find("Dropdown List"));
                    EndOtherProcesses();
                    resetDropdown.GetComponent<Dropdown>().value = 0;
                    State.BeginRobotReset();
                    break;
                case 3:
                    Auxiliary.FindObject(canvas, "ResetRobotDropdown").SetActive(false);
                    Auxiliary.FindObject(canvas, "LoadingPanel").SetActive(true);
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

        /// <summary>
        /// Change field button callback
        /// </summary>
        public void OnChangeFieldButtonPressed()
        {
            panel.OpenOnly(changeFieldPanel);
            panel.Freeze(true);
        }

        /// <summary>
        /// Enters replay mode
        /// </summary>
        public void OnReplayModeButtonPressed()
        {
            State.EnterReplayState();
        }

        /// <summary>
        /// Toggles the multiplayer window
        /// </summary>
        public void OnMultiplayerButtonPressed()
        {
            panel.OpenOnly(multiplayerPanel);
            multiplayer.UpdateUI();
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
        /// Help button and menu text
        /// </summary>
        public void OnHelpButtonPressed()
        {
            helpMenu.SetActive(true);

            // To change the help menu texts within the simulator, most are configured by calling the
            // the following component. 
            helpBodyText.GetComponent<Text>().text = "\n\nTutorials: bxd.autodesk.com" +
                "\n\nHome Tab: Main simulator functions" +
                "\n\nDriver Practice Tab: Gamepiece setup and interaction" +
                "\n\nScoring Tab: Match play" +
                "\n\nSensors Tab: Robot camera and sensors";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "MainToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(300, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0));
                else t.gameObject.SetActive(false);
            }

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Main Toolbar Help Button Pressed", new Dictionary<string, object> //for analytics tracking
                {
                });
            }
        }

        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in toolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
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

            toolkit.EndProcesses();
            multiplayer.EndProcesses();
            sensorManagerGUI.EndProcesses();
            robotCameraGUI.EndProcesses();
        }
    }
}