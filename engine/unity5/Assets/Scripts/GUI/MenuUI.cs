﻿using UnityEngine;
using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;

namespace Synthesis.GUI
{
    /// <summary>
    /// MenuUI serves as an interface between the Unity button UI and the various functions within the settings menu.
    /// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
    /// </summary>
    public class MenuUI : LinkedMonoBehaviour<MainState>
    {
        GameObject canvas;

        GameObject robotControlPanel;
        GameObject globalControlPanel;
        GameObject settingsPanel;
        GameObject viewReplaysPanel;
        GameObject helpPanel;

        SettingsState settings;
        LoadReplayState loadReplay;

        public static MenuUI instance;

        public delegate void EntryChanged(int a);

        public event EntryChanged OnResolutionSelection, OnScreenmodeSelection, OnQualitySelection;

        // Robot controls
        // Global Controls
        // Settings
        // View Replays
        // Help

        public void Start()
        {
            instance = this;

            canvas = GameObject.Find("Canvas");

            robotControlPanel = Auxiliary.FindObject(canvas, "RobotControlPanel");
            globalControlPanel = Auxiliary.FindObject(canvas, "GlobalControlPanel");
            settingsPanel = Auxiliary.FindObject(canvas, "SettingsPanel");
            viewReplaysPanel = Auxiliary.FindObject(canvas, "LoadReplayPanel");
            helpPanel = Auxiliary.FindObject(canvas, "HelpPanel");

            settings = settingsPanel.GetComponent<SettingsState>();
            loadReplay = viewReplaysPanel.GetComponent<LoadReplayState>();
        }

        public void LateUpdate()
        {
            if (settingsPanel.activeSelf)
            {
                settings.LateUpdate();
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
        }

        #region robot controls

        public void SwitchRobotControls()
        {
            EndOtherProcesses();
            robotControlPanel.SetActive(true);
        }

        #endregion
        #region global controls

        public void SwitchGlobalControls()
        {
            EndOtherProcesses();
            globalControlPanel.SetActive(true);
        }

        #endregion
        #region settings

        public void SwitchSettings()
        {
            EndOtherProcesses();
            if (!settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(true);
                settings.Start();
            } else
            {
                settingsPanel.SetActive(false);
                settings.End();
            }
        }

        public void ResolutionSelectionChanged(int a)
        {
            OnResolutionSelection(a);
            Debug.Log("a" + a);
        }

        public void ScreenmodeSelectionChanged(int a)
        {
            OnScreenmodeSelection(a);
        }

        public void QualitySelectionChanged(int a)
        {
            OnQualitySelection(a);
        }

        public void SettingsToggleAnalytics()
        {
            settings.ToggleAnalytics();
        }

        public void SettingsApply()
        {
            settings.ApplySettings();
        }

        #endregion
        #region view replays

        public void SwitchViewReplays()
        {
            EndOtherProcesses();
            if (!viewReplaysPanel.activeSelf)
            {
                viewReplaysPanel.SetActive(true);
                loadReplay.Start();
            } else
            {
                viewReplaysPanel.SetActive(false);
                loadReplay.End();
            }
        }

        public void DeleteReplayButton()
        {
            loadReplay.DeleteReplay();
        }

        public void LaunchReplayButton()
        {
            loadReplay.LaunchReplay();
        }

        #endregion
        #region help

        public void SwitchHelp()
        {
            EndOtherProcesses();
            helpPanel.SetActive(true);
        }

        #endregion

        /// <summary>
        /// Call this function whenever the user enters a new state
        /// </summary>
        public void EndOtherProcesses()
        {
            robotControlPanel.SetActive(false);
            globalControlPanel.SetActive(false);
            settingsPanel.SetActive(false);
            viewReplaysPanel.SetActive(false);
            helpPanel.SetActive(false);
        }

    }
}