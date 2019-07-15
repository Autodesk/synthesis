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
        GameObject spawnpointPanel;

        GameObject changeRobotPanel;
        GameObject robotListPanel;
        GameObject changeFieldPanel;

        GameObject mixAndMatchPanel;
        GameObject changePanel;
        GameObject addPanel;
        GameObject driverStationPanel;

        GameObject inputManagerPanel;
        GameObject checkSavePanel;
        GameObject unitConversionSwitch;

        GameObject hotKeyButton;
        GameObject hotKeyPanel;
        GameObject settingsPanel;

        GameObject exitPanel;
        GameObject loadingPanel;

        GameObject orientWindow;
        GameObject resetDropdown;

        GameObject tabs;
        GameObject emulationTab;

        private bool freeroamWindowClosed = false;
        private bool overviewWindowClosed = false;
        private bool oppositeSide = false;
        public static bool inputPanelOn = false;
        public static bool changeAnalytics = true;

        private StateMachine tabStateMachine;
        string currentTab;

        public static string updater;

        public Sprite normalButton; // these sprites are attached to the SimUI script
        public Sprite highlightButton; // in the Scene simulator

        GameObject helpMenu;
        GameObject overlay;

        private static SimUI instance = null;

        private void Start()
        {
            instance = this;
        }

        public static SimUI getSimUI() { return instance; }

        public StateMachine getTabStateMachine() { return tabStateMachine; }

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
            }
            HighlightTabs();
            if (State.isEmulationDownloaded) emulationTab.SetActive(true);
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
            resetUI = Auxiliary.FindGameObject("ResetRobotSpawnpointUI");

            freeroamCameraWindow = Auxiliary.FindObject(canvas, "FreeroamPanel");
            overviewCameraWindow = Auxiliary.FindObject(canvas, "OverviewPanel");
            spawnpointPanel = Auxiliary.FindObject(canvas, "SpawnpointPanel");
            //multiplayerPanel = Auxiliary.FindObject(canvas, "MultiplayerPanel");
            driverStationPanel = Auxiliary.FindObject(canvas, "DriverStationPanel");
            changeRobotPanel = Auxiliary.FindObject(canvas, "ChangeRobotPanel");
            robotListPanel = Auxiliary.FindObject(changeRobotPanel, "RobotListPanel");
            changeFieldPanel = Auxiliary.FindObject(canvas, "ChangeFieldPanel");
            inputManagerPanel = Auxiliary.FindObject(canvas, "InputManagerPanel");
            checkSavePanel = Auxiliary.FindObject(canvas, "CheckSavePanel");
            unitConversionSwitch = Auxiliary.FindObject(canvas, "UnitConversionSwitch");

            hotKeyPanel = Auxiliary.FindObject(canvas, "HotKeyPanel");
            hotKeyButton = Auxiliary.FindObject(canvas, "DisplayHotKeyButton");

            orientWindow = Auxiliary.FindObject(resetUI, "OrientWindow");
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
            settingsPanel = Auxiliary.FindObject(canvas, "SettingsPanel");
            emulationTab = Auxiliary.FindObject(tabs, "EmulationTab");
            tabStateMachine = tabs.GetComponent<StateMachine>();

            CheckControlPanel();

            LinkToolbars();
            tabStateMachine.ChangeState(new MainToolbarState());
            currentTab = "HomeTab";

            UICallbackManager.RegisterButtonCallbacks(tabStateMachine, canvas);
            UICallbackManager.RegisterDropdownCallbacks(tabStateMachine, canvas);

            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
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
            UnityEngine.Debug.Log("Logged Tab Timing");

            switch (currentTab)
            {
                case "HomeTab":
                    AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.HomeTab,
                        AnalyticsLedger.TimingVarible.Customizing,
                        AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
                    UnityEngine.Debug.Log("WOW we can log home tab");
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
        public void OnMainTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.HomeTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.HomeTab,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.HomeTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (helpMenu.activeSelf) CloseHelpMenu("MainToolbar");
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
                "",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.DPMTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (FieldDataHandler.gamepieces.Count > 0)
            {
                if (helpMenu.activeSelf) CloseHelpMenu("DPMToolbar");
                currentTab = "DriverPracticeTab";
                tabStateMachine.ChangeState(new DPMToolbarState());
            }
            else UserMessageManager.Dispatch("No Gamepieces Available In Field. Driver Practice Disabled.", 3);
        }

        public void OnScoringTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.ScoringTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ScoringTab,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.ScoringTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (FieldDataHandler.gamepieces.Count > 0)
            {
                if (helpMenu.activeSelf) CloseHelpMenu("ScoringToolbar");
                currentTab = "ScoringTab";
                tabStateMachine.ChangeState(new ScoringToolbarState());
            }
            else UserMessageManager.Dispatch("No Gamepieces Available In Field. Scoring Disabled.", 3);
        }

        public void OnSensorTab()
        {
            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.SensorTab,
                AnalyticsLedger.TimingVarible.Customizing,
                AnalyticsLedger.TimingLabel.MainSimulator); // log any timing events from switching tabs
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.SensorTab,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.SensorTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (helpMenu.activeSelf) CloseHelpMenu("SensorToolbar");
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
                "",
                AnalyticsLedger.getMilliseconds().ToString()); // log the button was clicked
            AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.EmulationTab,
                AnalyticsLedger.TimingVarible.Customizing); // start timer for current tab

            if (helpMenu.activeSelf) CloseHelpMenu("EmulationToolbar");
            currentTab = "EmulationTab";
            EmulationToolbarState.s = new Serialization();
            tabStateMachine.ChangeState(new EmulationToolbarState());
        }

        public void OnSettingsTab()
        {
            if (!settingsPanel.activeSelf)
            {
                tabStateMachine.PushState(new SettingsState());
            } else
            {
                tabStateMachine.PopState();
            }

            /*if (settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
            }
            else
            {
                EndOtherProcesses();
                //settingsPanel.SetActive(true);
                tabStateMachine.ChangeState(new OptionsTabState());
            }*/
        }

        private void CloseHelpMenu(string currentID = " ")
        {
            string toolbarID = Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text;
            if (toolbarID.Equals(currentID)) return;
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in Auxiliary.FindObject(toolbarID).transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
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

        /// <summary>
        /// Performs a sprite swap for the active tab.
        /// </summary>
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
            string directory = PlayerPrefs.GetString("RobotDirectory") + Path.DirectorySeparatorChar + panel.GetComponent<ChangeRobotScrollable>().selectedEntry;
            if (Directory.Exists(directory))
            {
                panel.SetActive(false);
                changeRobotPanel.SetActive(false);
                PlayerPrefs.SetString("simSelectedReplay", string.Empty);
                PlayerPrefs.SetString("simSelectedRobot", directory);
                PlayerPrefs.SetString("simSelectedRobotName", panel.GetComponent<ChangeRobotScrollable>().selectedEntry);
                PlayerPrefs.SetInt("hasManipulator", 0); //0 is false, 1 is true
                PlayerPrefs.Save();

                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ChangeRobot,
                    AnalyticsLedger.EventAction.Clicked,
                    "change",
                    AnalyticsLedger.getMilliseconds().ToString());

                robotCameraManager.DetachCamerasFromRobot(State.ActiveRobot);
                sensorManager.RemoveSensorsFromRobot(State.ActiveRobot);

                State.ChangeRobot(directory, false);
                RobotTypeManager.IsMixAndMatch = false;
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
                changeRobotPanel.transform.Find("PathLabel").GetComponent<Text>().text = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "synthesis" + Path.DirectorySeparatorChar + "Fields"));
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
                loadingPanel.SetActive(true);
                PlayerPrefs.SetString("simSelectedReplay", string.Empty);
                PlayerPrefs.SetString("simSelectedField", directory);
                PlayerPrefs.SetString("simSelectedFieldName", panel.GetComponent<ChangeFieldScrollable>().selectedEntry);
                PlayerPrefs.Save();

                AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.MainSimulator,
                    AnalyticsLedger.TimingVarible.Playing,
                    AnalyticsLedger.TimingLabel.ResetField);

                //FieldDataHandler.Load();
                //DPMDataHandler.Load();
                //Controls.Init();
                //Controls.Load();
                SceneManager.LoadScene("Scene");

                AnalyticsManager.GlobalInstance.StartTime(AnalyticsLedger.TimingLabel.ChangeField,
                    AnalyticsLedger.TimingVarible.Playing); // start timer for current field
            }
            else
            {
                UserMessageManager.Dispatch("Field directory not found!", 5);
            }
        }

        /// <summary>
        /// These toggle, change, and add functions are tethered in Unity
        /// </summary>
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
                changeFieldPanel.transform.Find("PathLabel").GetComponent<Text>().text = PlayerPrefs.GetString("FieldDirectory", (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                    + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "synthesis" + Path.DirectorySeparatorChar + "Fields"));
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
        #region control panel and analytics functions
        /// <summary>
        /// Toggle the control panel ON/OFF based on the boolean passed.
        /// </summary>
        /// <param name="show"></param>
        public void ShowControlPanel(bool alreadySaved)
        {
            if (!inputManagerPanel.activeSelf)
            {
                DynamicCamera.ControlEnabled = false;
                InputControl.freeze = true;
                EndOtherProcesses();
                inputManagerPanel.SetActive(true);
                inputPanelOn = true;

                Controls.Load();
                GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
            }
            else
            {
                DynamicCamera.ControlEnabled = true;
                InputControl.freeze = false;
                inputManagerPanel.SetActive(false);
                inputPanelOn = false;
                ToggleHotKeys(false);

                if (!alreadySaved && Controls.CheckIfSaved())
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
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ScoreHelp,
                AnalyticsLedger.EventAction.Clicked,
                "",
                AnalyticsLedger.getMilliseconds().ToString());
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
            if (helpMenu.activeSelf) CloseHelpMenu();
            EndOtherProcesses();
            switch (option)
            {
                case "open":
                    exitPanel.SetActive(true);
                    break;
                case "exit":
                    LogTabTiming();
                    if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
                    break;
                case "cancel":
                    exitPanel.SetActive(false);
                    break;
            }

            // log any timing events and log that the button was clicked
            
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.HomeTab,
                AnalyticsLedger.EventAction.Clicked,
                "",
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
            inputManagerPanel.SetActive(false);
            ToggleHotKeys(false);

            CancelOrientation();

            if (settingsPanel.activeSelf)
            {
                tabStateMachine.PopState();
            }

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