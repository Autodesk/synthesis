using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Synthesis.FSM;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;
using Synthesis.DriverPractice;
using Synthesis.GUI;
using Synthesis.GUI.Scrollables;
using Synthesis.Input;
using Synthesis.MixAndMatch;
using Synthesis.Camera;
using Synthesis.Sensors;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Robot;
using Assets.Scripts.GUI;
using Synthesis.Field;

namespace Synthesis.GUI
{
    /// <summary>
    /// SimUI serves as an interface between the Unity button UI and the various functions within the simulator.
    /// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
    /// </summary>
    public class SimUI : LinkedMonoBehaviour<MainState>
    {
        RobotBase Robot;

        new DynamicCamera camera;
        Toolkit toolkit;
        LocalMultiplayer multiplayer;
        SensorManagerGUI sensorManagerGUI;
        SensorManager sensorManager;
        RobotCameraManager robotCameraManager;
        RobotCameraGUI robotCameraGUI;
        GoalManager gm;

        GameObject canvas;

        GameObject freeroamCameraWindow;
        GameObject spawnpointPanel;

        GameObject changeRobotPanel;
        GameObject robotListPanel;
        GameObject changeFieldPanel;

        GameObject mixAndMatchPanel;
        GameObject changePanel;
        GameObject addPanel;
        GameObject driverStationSettingsPanel;
        GameObject driverStationPanel;

        GameObject inputManagerPanel;
        GameObject bindedKeyPanel;
        GameObject checkSavePanel;
        GameObject unitConversionSwitch;

        GameObject hotKeyButton;
        GameObject hotKeyPanel;
        GameObject analyticsPanel;

        GameObject toolbar;

        GameObject exitPanel;
        GameObject loadingPanel;

        GameObject orientWindow;
        GameObject resetDropdown;

        GameObject tabs;
        GameObject mainTab;
        GameObject dpmTab;
        GameObject scoringTab;
        GameObject sensorTab;

        private bool freeroamWindowClosed = false;
        private bool oppositeSide = false;
        public static bool inputPanelOn = false;
        public static bool changeAnalytics = true;

        private StateMachine tabStateMachine;
        string currentTab;
        public Sprite normalButton;
        public Sprite highlightButton;

        GameObject helpMenu;
        GameObject overlay;

        private void Update()
        {
            if (toolkit == null)
            {
                camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

                toolkit = GetComponent<Toolkit>();
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

                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && !InputControl.freeze)
                {
                    if (!exitPanel.activeSelf)
                    {
                        if (GameObject.Find("Dropdown List")) GameObject.Destroy(GameObject.Find("Dropdown List"));
                        MainMenuExit("open");
                    }
                    else MainMenuExit("cancel");
                }

                if (UnityEngine.Input.GetKey(KeyCode.LeftControl) && UnityEngine.Input.GetKeyDown(KeyCode.H))
                {
                    TogglePanel(toolbar);
                }

                if (KeyButton.Binded() && inputPanelOn)
                {
                    ShowBindedInfoPanel();
                }
            }
            HighlightTabs();
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

            freeroamCameraWindow = Auxiliary.FindObject(canvas, "FreeroamPanel");
            spawnpointPanel = Auxiliary.FindObject(canvas, "SpawnpointPanel");
            //multiplayerPanel = Auxiliary.FindObject(canvas, "MultiplayerPanel");
            driverStationPanel = Auxiliary.FindObject(canvas, "DriverStationPanel");
            changeRobotPanel = Auxiliary.FindObject(canvas, "ChangeRobotPanel");
            robotListPanel = Auxiliary.FindObject(changeRobotPanel, "RobotListPanel");
            changeFieldPanel = Auxiliary.FindObject(canvas, "ChangeFieldPanel");
            inputManagerPanel = Auxiliary.FindObject(canvas, "InputManagerPanel");
            bindedKeyPanel = Auxiliary.FindObject(canvas, "BindedKeyPanel");
            checkSavePanel = Auxiliary.FindObject(canvas, "CheckSavePanel");
            unitConversionSwitch = Auxiliary.FindObject(canvas, "UnitConversionSwitch");

            hotKeyPanel = Auxiliary.FindObject(canvas, "HotKeyPanel");
            hotKeyButton = Auxiliary.FindObject(canvas, "DisplayHotKeyButton");

            orientWindow = Auxiliary.FindObject(canvas, "OrientWindow");
            resetDropdown = GameObject.Find("Reset Robot Dropdown");

            exitPanel = Auxiliary.FindObject(canvas, "ExitPanel");
            loadingPanel = Auxiliary.FindObject(canvas, "LoadingPanel");
            analyticsPanel = Auxiliary.FindObject(canvas, "AnalyticsPanel");
            sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();
            robotCameraGUI = GetComponent<RobotCameraGUI>();
            mixAndMatchPanel = Auxiliary.FindObject(canvas, "MixAndMatchPanel");
            toolbar = Auxiliary.FindObject(canvas, "Toolbar");

            changePanel = Auxiliary.FindObject(canvas, "ChangePanel");
            addPanel = Auxiliary.FindObject(canvas, "AddPanel");
            //toolkitPanel = Auxiliary.FindObject(canvas, "ToolkitPanel");
            driverStationSettingsPanel = Auxiliary.FindObject(canvas, "DPMPanel");

            tabs = Auxiliary.FindGameObject("Tabs");
            mainTab = Auxiliary.FindObject(tabs, "HomeTab");
            dpmTab = Auxiliary.FindObject(tabs, "DriverPracticeTab");
            scoringTab = Auxiliary.FindObject(tabs, "ScoringTab");
            sensorTab = Auxiliary.FindObject(tabs, "SensorTab");

            tabStateMachine = tabs.GetComponent<StateMachine>();

            CheckControlPanel();

            LinkToolbars();
            tabStateMachine.ChangeState(new MainToolbarState());
            currentTab = "HomeTab";

            ButtonCallbackManager.RegisterButtonCallbacks(tabStateMachine, canvas);
            ButtonCallbackManager.RegisterDropdownCallbacks(tabStateMachine, canvas);

            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
        }

        private void UpdateWindows()
        {
            if (State != null)
                UpdateFreeroamWindow();
            UpdateSpawnpointWindow();
            UpdateDriverStationPanel();
        }

        #region tab buttons
        public void OnMainTab()
        {
            if (helpMenu.activeSelf) CloseHelpMenu("MainToolbar");
            currentTab = "HomeTab";
            tabStateMachine.ChangeState(new MainToolbarState());
        }

        public void OnDPMTab()
        {
            if (helpMenu.activeSelf) CloseHelpMenu("DPMToolbar");
            currentTab = "DriverPracticeTab";
            tabStateMachine.ChangeState(new DPMToolbarState());
        }

        public void OnScoringTab()
        {
            if (helpMenu.activeSelf) CloseHelpMenu("ScoringToolbar");
            currentTab = "ScoringTab";
            tabStateMachine.ChangeState(new ScoringToolbarState());
        }

        public void OnSensorTab()
        {
            if (helpMenu.activeSelf) CloseHelpMenu("SensorToolbar");
            currentTab = "SensorTab";
            tabStateMachine.ChangeState(new SensorToolbarState());
        }

        public void OnEmulationTab()
        {
            if (helpMenu.activeSelf) CloseHelpMenu("EmulationToolbar");
            currentTab = "EmulationTab";
            tabStateMachine.ChangeState(new EmulationToolbarState());
        }

        private void CloseHelpMenu(string currentID = " ")
        {
            string toolbarID = Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text;
            if (toolbarID.Equals(currentID)) return;
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-200, 0, 0));
            foreach (Transform t in Auxiliary.FindObject(toolbarID).transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-200, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }

        private void HighlightTabs()
        {
            foreach (Transform t in tabs.transform)
            {
                if (t.gameObject.name.Equals(currentTab))
                {
                    t.gameObject.GetComponent<Image>().sprite = highlightButton;
                }
                else t.gameObject.GetComponent<Image>().sprite = normalButton;
            }
        }
        #endregion
        #region change robot/field functions
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
                PlayerPrefs.SetInt("hasManipulator", 0); //0 is false, 1 is true
                PlayerPrefs.Save();

                if (PlayerPrefs.GetInt("analytics") == 1) //for analytics tracking
                {
                    Analytics.CustomEvent("Changed Robot", new Dictionary<string, object>
                    {
                    });
                }

                robotCameraManager.DetachCamerasFromRobot(State.ActiveRobot);
                sensorManager.RemoveSensorsFromRobot(State.ActiveRobot);

                DPMDataHandler.Load();
                State.ChangeRobot(directory, false);

            }
            else
            {
                UserMessageManager.Dispatch("Robot directory not found!", 5);
            }
        }

        /// <summary>
        /// Changes the drive base, destroys old manipulator and creates new manipulator, sets wheels
        /// </summary>
        public void MaMChangeRobot(string robotDirectory, string manipulatorDirectory)
        {
            MaMRobot mamRobot = State.ActiveRobot as MaMRobot;

            robotCameraManager.DetachCamerasFromRobot(State.ActiveRobot);
            sensorManager.RemoveSensorsFromRobot(State.ActiveRobot);

            //If the current robot has a manipulator, destroy the manipulator
            if (mamRobot != null && mamRobot.RobotHasManipulator)
                State.DeleteManipulatorNodes();

            State.ChangeRobot(robotDirectory, true);

            //If the new robot has a manipulator, load the manipulator
            if (RobotTypeManager.HasManipulator)
                State.LoadManipulator(manipulatorDirectory);
            else if (mamRobot != null)
                mamRobot.RobotHasManipulator = false;
        }

        public void ToggleChangeRobotPanel()
        {
            if (changeRobotPanel.activeSelf)
            {
                changeRobotPanel.SetActive(false);
                DynamicCamera.ControlEnabled = true;
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

                if (PlayerPrefs.GetInt("analytics") == 1) //for analytics tracking
                    Analytics.CustomEvent("Changed Field", new Dictionary<string, object>
                    {
                    });
                //FieldDataHandler.Load();
                //DPMDataHandler.Load();
                //Controls.Init();
                //Controls.Load();
                SceneManager.LoadScene("Scene");
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
                DynamicCamera.ControlEnabled = true;
            }
            else
            {
                EndOtherProcesses();
                changeFieldPanel.SetActive(true);
            }
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

        public void ToggleAddRobotPanel()
        {
            if (addPanel.activeSelf == true)
            {
                addPanel.SetActive(false);
            }
            else
            {
                addPanel.SetActive(true);
                changePanel.SetActive(false);
            }
        }

        public void ToggleChangePanel()
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

        #endregion
        #region camera button functions
        /// <summary>
        /// Toggles between different dynamic camera states
        /// </summary>
        /// <param name="mode"></param>
        public void SwitchCameraView(int mode)
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
        public void OrientLeft()
        {
            State.RotateRobot(new Vector3(Mathf.PI * 0.25f, 0f, 0f));
        }
        public void OrientRight()
        {
            State.RotateRobot(new Vector3(-Mathf.PI * 0.25f, 0f, 0f));
        }
        public void OrientForward()
        {
            State.RotateRobot(new Vector3(0f, 0f, Mathf.PI * 0.25f));
        }
        public void OrientBackward()
        {
            State.RotateRobot(new Vector3(0f, 0f, -Mathf.PI * 0.25f));
        }

        public void DefaultOrientation()
        {
            State.ResetRobotOrientation();
        }

        public void SaveOrientation()
        {
            State.SaveRobotOrientation();
        }

        public void CancelOrientation()
        {
            State.CancelRobotOrientation();
        }

        #endregion
        #region control panel and analytics functions
        /// <summary>
        /// Toggle the control panel ON/OFF based on the boolean passed.
        /// </summary>
        /// <param name="show"></param>
        public void ShowControlPanel(bool show)
        {
            if (show)
            {
                EndOtherProcesses();
                inputManagerPanel.SetActive(true);
                inputPanelOn = true;

                Controls.Load();
                GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
            }
            else
            {
                inputManagerPanel.SetActive(false);
                bindedKeyPanel.SetActive(false);
                inputPanelOn = false;
                ToggleHotKeys(false);

                if (Controls.CheckIfSaved())
                {
                    checkSavePanel.SetActive(true);
                }
            }
        }

        /// <summary>
        /// Toggle the control panel ON/OFF based on its current state
        /// </summary>
        public void ShowControlPanel()
        {
            ShowControlPanel(!inputManagerPanel.activeSelf);
        }

        public void SaveAndClose()
        {
            GameObject.Find("SettingsMode").GetComponent<SettingsMode>().OnSaveClick();
            inputManagerPanel.SetActive(false);
        }

        /// <summary>
        /// Pop up error-panel if user enters WASD for robot controls
        /// </summary>
        public void ShowBindedInfoPanel()
        {
            if (KeyButton.Binded())
            {
                bindedKeyPanel.SetActive(true);
            }
            else
            {
                bindedKeyPanel.SetActive(false);
                ToggleHotKeys(false);
            }
        }

        /// <summary>
        /// Checks the last state of the control panel. Defaults to OFF
        /// unless the user leaves it on.
        /// </summary>
        public void CheckControlPanel()
        {
            if (PlayerPrefs.GetInt("isInputManagerPanel", 1) == 0)
            {
                inputManagerPanel.SetActive(false);
            }
            else
            {
                inputManagerPanel.SetActive(true);
                PlayerPrefs.SetInt("isInputManagerPanel", 0);
            }
        }

        public void CheckForSavedControls(string option)
        {
            checkSavePanel.SetActive(false);

            switch (option)
            {
                case "yes":
                    Controls.Save();
                    break;
                case "no":
                    Controls.Load();
                    inputManagerPanel.SetActive(false);
                    break;
                case "cancel":
                    inputManagerPanel.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// Open tutorial link
        /// </summary>
        public void OpenTutorialLink()
        {
            Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
            if (PlayerPrefs.GetInt("analytics") == 1) //for analytics tracking
            {
                Analytics.CustomEvent("Clicked Tutorial Link", new Dictionary<string, object>
                {
                });
            }
        }
        /// <summary>
        /// Activates analytics panel
        /// </summary>
        public void ToggleAnalyticsPanel()
        {
            if (analyticsPanel.activeSelf)
            {
                analyticsPanel.SetActive(false);
            }
            else
            {
                EndOtherProcesses();
                analyticsPanel.SetActive(true);
                inputManagerPanel.SetActive(true);
            }
        }

        /// <summary>
        /// Toggle for analytics
        /// </summary>
        public void ToggleAnalytics(bool tAnalytics)
        {
            if (PlayerPrefs.GetInt("analytics") == 0)
            {
                PlayerPrefs.SetInt("analytics", 1);
            }
            else
            {
                PlayerPrefs.SetInt("analytics", 0);
            }
        }

        /// <summary>
        /// Toggles between meter and feet measurements
        /// </summary>
        public void ToggleUnitConversion()
        {
            if (canvas != null)
            {
                unitConversionSwitch = Auxiliary.FindObject(canvas, "UnitConversionSwitch");
                int i = (int)unitConversionSwitch.GetComponent<Slider>().value;
                State.IsMetric = (i == 1 ? true : false);
                PlayerPrefs.SetString("Measure", i == 1 ? "Metric" : "Imperial");
                //Debug.Log("Metric: " + main.IsMetric);
            }
        }

        /// <summary>
        /// Toggle the hot key tool tips on/off based on the boolean passed in
        /// </summary>
        /// <param name="show"></param>
        public void ToggleHotKeys(bool show)
        {
            hotKeyPanel.SetActive(show);
            if (show)
            {
                hotKeyButton.GetComponentInChildren<Text>().text = "Hide Hot Keys";
            }
            else
            {
                hotKeyButton.GetComponentInChildren<Text>().text = "Display Hot Keys";
            }
        }

        /// <summary>
        ///Toggle the hot key tool tips on/off based on its current state
        /// </summary>
        public void ToggleHotKeys()
        {
            ToggleHotKeys(!hotKeyPanel.activeSelf);
        }
        #endregion
        #region reset functions
        /// <summary>
        /// Pop reset instructions when main is in reset spawnpoint mode, enable orient robot at the same time
        /// </summary>
        private void UpdateSpawnpointWindow()
        {
            if (State.ActiveRobot.IsResetting)
            {
                spawnpointPanel.SetActive(true);
                orientWindow.SetActive(true);
            }
            else
            {
                spawnpointPanel.SetActive(false);
                orientWindow.SetActive(false);
            }
        }

        /// <summary>
        /// Allows for user to reset their robot to the default spawnpoint 
        /// </summary>
        public void RevertToDefaultSpawnPoint()
        {
            State.RevertSpawnpoint();
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
                    State.BeginRobotReset();
                    State.EndRobotReset();
                    resetDropdown.GetComponent<Dropdown>().value = 0;
                    break;
                case 2:
                    EndOtherProcesses();
                    DynamicCamera.ControlEnabled = true;
                    State.BeginRobotReset();
                    resetDropdown.GetComponent<Dropdown>().value = 0;
                    break;
                case 3:
                    Auxiliary.FindObject(GameObject.Find("Reset Robot Dropdown"), "Dropdown List").SetActive(false);
                    Auxiliary.FindObject(GameObject.Find("Canvas"), "LoadingPanel").SetActive(true);
                    SceneManager.LoadScene("Scene");
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
            if (helpMenu.activeSelf) CloseHelpMenu();
            EndOtherProcesses();
            switch (option)
            {
                case "open":
                    exitPanel.SetActive(true);
                    break;
                case "exit":
                    SceneManager.LoadScene("MainMenu");
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
            changePanel.SetActive(false);
            addPanel.SetActive(false);
            analyticsPanel.SetActive(false);
            inputManagerPanel.SetActive(false);
            ToggleHotKeys(false);

            CancelOrientation();

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
            State.EnterReplayState();
        }


        /// <summary>
        /// Links the specific toolbars to their specified states
        /// </summary>
        private void LinkToolbars()
        {
            LinkToolbar<MainToolbarState>("MainToolbar");
            LinkToolbar<DPMToolbarState>("DPMToolbar");
            LinkToolbar<ScoringToolbarState>("ScoringToolbar");
            LinkToolbar<SensorToolbarState>("SensorToolbar");
            LinkToolbar<EmulationToolbarState>("EmulationToolbar");
        }

        /// <summary>
        /// Links each gameobject "tab" to the specified state
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tabName"></param>
        /// <param name="strict"></param>
        private void LinkToolbar<T>(string tabName, bool strict = true) where T : State
        {
            GameObject tab = Auxiliary.FindGameObject(tabName);

            if (tab != null)
                tabStateMachine.Link<T>(tab, strict);
        }
    }
}