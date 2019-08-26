using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Synthesis.FSM;
using System.IO;
using UnityEngine.SceneManagement;
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
using System;
using System.Diagnostics;

namespace Synthesis.GUI
{
    /// <summary>
    /// SimUI serves as an interface between the Unity button UI and the various functions within the simulator.
    /// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
    /// </summary>
    public class SimUI : LinkedMonoBehaviour<MainState>
    {
        public const float WARNING_TIME = 5; // seconds

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
        GameObject resetUI;

        GameObject freeroamCameraWindow;
        GameObject overviewCameraWindow;

        GameObject changeRobotPanel;
        GameObject robotListPanel;
        GameObject changeFieldPanel;

        GameObject mixAndMatchPanel;
        GameObject changePanel;
        GameObject addPanel;
        GameObject driverStationPanel;

        GameObject exitPanel;
        GameObject loadingPanel;

        GameObject resetRobotUI;
        GameObject resetDropdown;

        GameObject tabs;
        GameObject emulationTab;

        private bool freeroamWindowClosed = false;
        private bool overviewWindowClosed = false;
        private bool oppositeSide = false;

        private StateMachine tabStateMachine;
        string currentTab;

        public static string updater;

        public Sprite normalButton; // these sprites are attached to the SimUI script
        public Sprite highlightButton; // in the Scene simulator
        private Sprite hoverHighlight;

        private static SimUI instance = null;

        private string[] lastJoystickNmaes = new string[Player.PLAYER_COUNT];

        private void Start()
        {
            instance = this;

            UpdateJoystickStates(false);
        }

        public static SimUI getSimUI() { return instance; }

        public StateMachine getTabStateMachine() { return tabStateMachine; }

        private void Update()
        {
            if (InputControl.GetButtonDown(new KeyMapping("Hide Menu", KeyCode.H, Input.Enums.KeyModifier.Ctrl), true))
            {
                tabs.SetActive(!tabs.activeSelf);
                tabStateMachine.CurrentState.ToggleHidden();
            }
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

                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
                {
                    if (!exitPanel.activeSelf)
                    {
                        if (GameObject.Find("Dropdown List")) GameObject.Destroy(GameObject.Find("Dropdown List"));
                        MainMenuExit("open");
                    }
                    else MainMenuExit("cancel");
                }
            }
            UpdateJoystickStates();
            HighlightTabs();
        }

        private void OnGUI()
        {
            UserMessageManager.Render();
        }

        /// <summary>
        /// Track joystick connections and disconnections
        /// </summary>
        /// <param name="warn">Whether to dispatch warnings on change or not</param>
        private void UpdateJoystickStates(bool warn = true)
        {
            var newJoystickNames = UnityEngine.Input.GetJoystickNames();
            for (var i = 0; i < lastJoystickNmaes.Length; i++)
            {
                string newState = (i < newJoystickNames.Length && !string.IsNullOrEmpty(newJoystickNames[i])) ? newJoystickNames[i] : null;
                if (warn && newState != lastJoystickNmaes[i])
                {
                    if (newState != null)
                    {
                        UserMessageManager.Dispatch("Connected - Joystick " + (i + 1) + " " + newState, 6);
                    }
                    else
                    {
                        UserMessageManager.Dispatch("Disconnected - Joystick " + (i + 1) + " " + lastJoystickNmaes[i], 6);
                    }
                }
                lastJoystickNmaes[i] = newState;
            }
        }

        /// <summary>
        /// Finds all the necessary UI elements that need to be updated/modified
        /// </summary>
        private void FindElements()
        {
            canvas = GameObject.Find("Canvas");
            resetUI = Auxiliary.FindGameObject("ResetRobotSpawnpointUI");

            freeroamCameraWindow = Auxiliary.FindObject(canvas, "FreeroamPanel");
            overviewCameraWindow = Auxiliary.FindObject(canvas, "OverviewPanel");
            //multiplayerPanel = Auxiliary.FindObject(canvas, "MultiplayerPanel");
            driverStationPanel = Auxiliary.FindObject(canvas, "DriverStationPanel");
            changeRobotPanel = Auxiliary.FindObject(canvas, "ChangeRobotPanel");
            robotListPanel = Auxiliary.FindObject(changeRobotPanel, "RobotListPanel");
            changeFieldPanel = Auxiliary.FindObject(canvas, "ChangeFieldPanel");

            resetRobotUI = Auxiliary.FindObject(resetUI, "ResetRobotSpawnpointUI");
            resetDropdown = GameObject.Find("Reset Robot Dropdown");

            exitPanel = Auxiliary.FindObject(canvas, "ExitPanel");
            loadingPanel = Auxiliary.FindObject(canvas, "LoadingPanel");
            sensorManager = GameObject.Find("SensorManager").GetComponent<SensorManager>();
            robotCameraManager = GameObject.Find("RobotCameraList").GetComponent<RobotCameraManager>();
            robotCameraGUI = GetComponent<RobotCameraGUI>();
            mixAndMatchPanel = Auxiliary.FindObject(canvas, "MixAndMatchPanel");

            changePanel = Auxiliary.FindObject(canvas, "ChangePanel");
            addPanel = Auxiliary.FindObject(canvas, "AddPanel");

            // tab and toolbar system components
            tabs = Auxiliary.FindGameObject("Tabs");
            emulationTab = Auxiliary.FindObject(tabs, "EmulationTab");
            tabStateMachine = tabs.GetComponent<StateMachine>();

            LinkToolbars();
            tabStateMachine.ChangeState(new MainToolbarState());
            currentTab = "HomeTab";

            UICallbackManager.RegisterButtonCallbacks(tabStateMachine, canvas);
            UICallbackManager.RegisterDropdownCallbacks(tabStateMachine, canvas);
        }

        private void UpdateWindows()
        {
            if (State != null)
            {
                UpdateFreeroamWindow();
                UpdateOverviewWindow();
            }
            UpdateSpawnpointWindow();
            UpdateDriverStationPanel();
        }

        public void CloseUpdatePrompt() {
            GameObject.Find("UpdatePrompt").SetActive(false);
        }

        public void UpdateYes() {
            Process.Start("http://synthesis.autodesk.com");
            Process.Start(updater);
            Application.Quit();
        }
        
        private void LogTabTiming()
        {
            switch (currentTab)
            {
                case "MenuTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MenuTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
                case "HomeTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.HomeTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
                case "DriverPracticeTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.DPMTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
                case "ScoringTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.ScoringTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
                case "SensorTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.SensorTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
                case "EmulationTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.EmulationTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
                default:
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.Tab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    break;
            }
        }

        #region tab buttons
        /// <summary>
        /// Each tab in the simulator is tethered by an OnClick() scripted function (below). Once the tab is clicked,
        /// each tab will activate a new toolbar state in where all of the toolbar functions will be managed by their
        /// specific states. 
        /// </summary>
        public void OnMenuTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MenuTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MenuTab,
                AnalyticsLedger.EventAction.Clicked,
                "Tab",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.HomeTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            currentTab = "MenuTab";
            tabStateMachine.PushState(new MenuToolbarState(), true);
        }

        public void OnMainTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.HomeTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.HomeTab,
                AnalyticsLedger.EventAction.Clicked,
                "Tab",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.HomeTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            currentTab = "HomeTab";
            tabStateMachine.ChangeState(new MainToolbarState());
        }

        public void OnDPMTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.DPMTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.DPMTab,
                AnalyticsLedger.EventAction.Clicked,
                "Tab",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.DPMTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (FieldDataHandler.gamepieces.Count > 0)
            {
                currentTab = "DriverPracticeTab";
                tabStateMachine.ChangeState(new DPMToolbarState());
            }
            else UserMessageManager.Dispatch("No Gamepieces Available In Field.", WARNING_TIME);
        }

        public void OnScoringTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.ScoringTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ScoringTab,
                AnalyticsLedger.EventAction.Clicked,
                "Tab",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.ScoringTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (FieldDataHandler.gamepieces.Count > 0)
            {
                currentTab = "ScoringTab";
                tabStateMachine.ChangeState(new ScoringToolbarState());
            }
            else UserMessageManager.Dispatch("No Gamepieces Available In Field. Scoring Disabled.", WARNING_TIME);
        }

        public void OnSensorTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.SensorTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.SensorTab,
                AnalyticsLedger.EventAction.Clicked,
                "Tab",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.SensorTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab
            
            currentTab = "SensorTab";
            tabStateMachine.ChangeState(new SensorToolbarState());
        }

        public void OnEmulationTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.EmulationTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                AnalyticsLedger.EventAction.Clicked,
                "Tab",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.EmulationTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab
            
            currentTab = "EmulationTab";
            tabStateMachine.ChangeState(new EmulationToolbarState());
        }

        public void ShowError(string msg)
        {
            GameObject errorScreen = Auxiliary.FindGameObject("ErrorScreen");
            errorScreen.transform.Find("ErrorText").GetComponent<Text>().text = msg;
            errorScreen.SetActive(true);
        }

        public void CloseErrorScreen()
        {
            Auxiliary.FindGameObject("ErrorScreen").SetActive(false);
        }

        public void ClosePointImpulse() {
            Auxiliary.FindGameObject("PointImpulsePanel").SetActive(false);
        }

        /// <summary>
        /// Performs a sprite swap for the active tab.
        /// </summary>
        private void HighlightTabs()
        {
            foreach (Transform t in tabs.transform)
            {
                if (t.gameObject.name.Equals(currentTab))
                {
                    // Showing it was choosen
                    t.gameObject.GetComponent<Image>().sprite = highlightButton;

                    // Disabling orange hover color
                    SpriteState s = new SpriteState();
                    s.highlightedSprite = null;
                    t.gameObject.GetComponent<Button>().spriteState = s;
                }
                else {
                    try {
                        t.gameObject.GetComponent<Image>().sprite = normalButton;
                        SpriteState s = new SpriteState();
                        s.highlightedSprite = hoverHighlight;
                        t.gameObject.GetComponent<Button>().spriteState = s;
                    } catch (Exception e) { }
                }
            }
        }
        #endregion
        #region change robot/field functions
        public void ChangeRobot()
        {
            GameObject panel = GameObject.Find("RobotListPanel");
            string directory = PlayerPrefs.GetString("RobotDirectory") + Path.DirectorySeparatorChar + panel.GetComponent<ChangeRobotScrollable>().selectedEntry;
            if (Directory.Exists(directory))
            {
                panel.SetActive(false);
                changeRobotPanel.SetActive(false);
                InputControl.EnableSimControls();
                PlayerPrefs.SetString("simSelectedReplay", string.Empty);
                PlayerPrefs.SetString("simSelectedRobot", directory);
                PlayerPrefs.SetString("simSelectedRobotName", panel.GetComponent<ChangeRobotScrollable>().selectedEntry);
                PlayerPrefs.SetInt("hasManipulator", 0); //0 is false, 1 is true
                PlayerPrefs.Save();

                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ChangeRobot,
                    AnalyticsLedger.EventAction.Clicked,
                    "Robot - Exported",
                    AnalyticsLedger.getMilliseconds().ToString());

                robotCameraManager.DetachCamerasFromRobot(State.ActiveRobot);
                sensorManager.RemoveSensorsFromRobot(State.ActiveRobot);

                State.ChangeRobot(directory, false);
                RobotTypeManager.IsMixAndMatch = false;
            }
            else
            {
                UserMessageManager.Dispatch("Robot directory not found!", WARNING_TIME);
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

            if (!State.ChangeRobot(robotDirectory, true)) {
                AppModel.ErrorToMenu("ROBOT_SELECT|Failed to load Mix & Match robot");
            }

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
                InputControl.EnableSimControls();
            }
            else
            {
                EndOtherProcesses();
                changeRobotPanel.SetActive(true);
                robotListPanel.SetActive(true);
                InputControl.DisableSimControls();
                Auxiliary.FindObject(changeRobotPanel, "PathLabel").GetComponent<Text>().text = PlayerPrefs.GetString("RobotDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Robots"));
            }
        }

        public void ChangeRobotDirectory()
        {
            StateMachine.SceneGlobal.PushState(new BrowseRobotState());
        }

        public void ChangeField()
        {
            GameObject panel = GameObject.Find("FieldListPanel");
            string directory = PlayerPrefs.GetString("FieldDirectory") + Path.DirectorySeparatorChar + panel.GetComponent<ChangeFieldScrollable>().selectedEntry;
            if (Directory.Exists(directory))
            {
                panel.SetActive(false);
                changeFieldPanel.SetActive(false);
                InputControl.EnableSimControls();
                loadingPanel.SetActive(true);
                PlayerPrefs.SetString("simSelectedReplay", string.Empty);
                PlayerPrefs.SetString("simSelectedField", directory);
                PlayerPrefs.SetString("simSelectedFieldName", panel.GetComponent<ChangeFieldScrollable>().selectedEntry);
                PlayerPrefs.Save();

                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                    AnalyticsLedger.TimingVarible.Playing,
                    AnalyticsLedger.TimingLabel.ResetField);
                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ChangeField,
                    AnalyticsLedger.EventAction.Changed,
                    panel.GetComponent<ChangeFieldScrollable>().selectedEntry.ToString(),
                    AnalyticsLedger.getMilliseconds().ToString());

                //FieldDataHandler.Load();
                //DPMDataHandler.Load();
                //Controls.Load();
                SceneManager.LoadScene("Scene");

                AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.ChangeField,
                    AnalyticsLedger.TimingVarible.Playing); // start timer for current field
            }
            else
            {
                UserMessageManager.Dispatch("Field directory not found!", WARNING_TIME);
            }
        }

        /// <summary>
        /// Reset to the empty grid
        /// </summary>
        public void LoadEmptyGrid()
        {
            MainState.timesLoaded = 0;
            
            changeFieldPanel.SetActive(false);
            InputControl.EnableSimControls();
            loadingPanel.SetActive(true);
            FieldDataHandler.Load("");

            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                AnalyticsLedger.TimingVarible.Playing,
                AnalyticsLedger.TimingLabel.ResetField);

            SceneManager.LoadScene("Scene");

            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.ChangeField,
                AnalyticsLedger.TimingVarible.Playing); // start timer for current field
        }

        /// <summary>
        /// These toggle, change, and add functions are tethered in Unity
        /// </summary>
        public void ToggleChangeFieldPanel()
        {
            if (changeFieldPanel.activeSelf)
            {
                changeFieldPanel.SetActive(false);
                InputControl.EnableSimControls();
            }
            else
            {
                EndOtherProcesses();
                changeFieldPanel.SetActive(true);
                InputControl.DisableSimControls();
                Auxiliary.FindObject(changeFieldPanel, "PathLabel").GetComponent<Text>().text = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields"));
            }
        }

        public void ChangeFieldDirectory()
        {
            StateMachine.SceneGlobal.PushState(new BrowseFieldState());
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
                if (IsMaMInstalled()) {
                    addPanel.SetActive(true);
                } else {
                    ToggleChangeRobotPanel();
                }
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
        /// Change camera tool tips
        /// </summary>
        public void CameraToolTips()
        {
            if (camera.ActiveState.GetType().Equals(typeof(DynamicCamera.DriverStationState)))
                camera.GetComponent<Text>().text = "Driver Station";
            else if (camera.ActiveState.GetType().Equals(typeof(DynamicCamera.FreeroamState)))
                camera.GetComponent<Text>().text = "Freeroam";
            else if (camera.ActiveState.GetType().Equals(typeof(DynamicCamera.OrbitState)))
                camera.GetComponent<Text>().text = "Orbit Robot";
            else if (camera.ActiveState.GetType().Equals(typeof(DynamicCamera.OverviewState)))
                camera.GetComponent<Text>().text = "Overview";
        }

        /// <summary>
        /// Pop freeroam instructions when using freeroam camera, won't show up again if the user closes it
        /// </summary>
        private void UpdateFreeroamWindow()
        {
            if (camera.ActiveState.GetType().Equals(typeof(DynamicCamera.FreeroamState)) && !freeroamWindowClosed)
            {
                if (!freeroamWindowClosed)
                {
                    freeroamCameraWindow.SetActive(true);
                }

            }
            else if (!camera.ActiveState.GetType().Equals(typeof(DynamicCamera.FreeroamState)))
            {
                freeroamCameraWindow.SetActive(false);
            }
        }

        private void UpdateOverviewWindow()
        {
            if (camera.ActiveState.GetType().Equals(typeof(DynamicCamera.OverviewState)) && !overviewWindowClosed)
            {
                if (!overviewWindowClosed)
                {
                    overviewCameraWindow.SetActive(true);
                }

            }
            else if (!camera.ActiveState.GetType().Equals(typeof(DynamicCamera.OverviewState)))
            {
                overviewCameraWindow.SetActive(false);
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
        /// Close overview camera tooltip
        /// </summary>
        public void CloseOverviewWindow()
        {
            overviewCameraWindow.SetActive(false);
            overviewWindowClosed = true;
        }

        /// <summary>
        /// Activate driver station tool tips if the main camera is in driver station state
        /// </summary>
        private void UpdateDriverStationPanel()
        {
            driverStationPanel.SetActive(camera.ActiveState.GetType().Equals(typeof(DynamicCamera.DriverStationState)));
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
        #region reset functions
        /// <summary>
        /// Pop reset instructions when main is in reset spawnpoint mode, enable orient robot at the same time
        /// </summary>
        private void UpdateSpawnpointWindow()
        {
            // TODO: Replace with resetUI (see PR #450)
            if (State.ActiveRobot.IsResetting)
            {
                resetRobotUI.SetActive(true);
            }
            else
            {
                resetRobotUI.SetActive(false);
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
                    MainState.timesLoaded--;

                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                        AnalyticsLedger.TimingVarible.Playing,
                        AnalyticsLedger.TimingLabel.ResetField);

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
            EndOtherProcesses();
            switch (option)
            {
                case "open":
                    exitPanel.SetActive(true);
                    break;
                case "exit":
                    LogTabTiming();
                    AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ExitTab,
                        AnalyticsLedger.EventAction.Exit,
                        "Exit",
                        AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked

                    if (!Application.isEditor) Application.Quit();
                    break;
                case "cancel":
                    exitPanel.SetActive(false);
                    break;
            }

            // log any timing events and log that the button was clicked
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ExitTab,
                AnalyticsLedger.EventAction.Exit,
                "Exit",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.HomeTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab
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

            CancelOrientation();

            toolkit.EndProcesses();
            multiplayer.EndProcesses();
            sensorManagerGUI.EndProcesses();
            robotCameraGUI.EndProcesses();
        }

        /// <summary>
        /// Links the specific toolbars to their specified states
        /// </summary>
        private void LinkToolbars()
        {
            LinkToolbar<MenuToolbarState>("MenuPanel");
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

        public static bool IsMaMInstalled() {
            return Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar
                + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "MixAndMatch" + Path.DirectorySeparatorChar
                + "DriveBases");
        }
    }
}
