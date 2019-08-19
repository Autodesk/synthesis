using UnityEngine;
using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using Synthesis.Input;
using System;

namespace Synthesis.GUI
{
    /// <summary>
    /// MenuUI serves as an interface between the Unity button UI and the various functions within the settings menu.
    /// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
    /// </summary>
    public class MenuUI : LinkedMonoBehaviour<MainState>
    {
        GameObject canvas;
        GameObject menuPanelInner;

        // main panels
        GameObject robotControlPanel;
        GameObject globalControlPanel;
        GameObject settingsPanel;
        GameObject viewReplaysPanel;
        GameObject helpPanel;

        // additional panels within the main panels
        GameObject checkSavePanel;

        UserSettings settings;
        LoadReplay loadReplay;
        public static MenuUI instance;

        // controls
        Action ProcessControlsCallback; // Function called after user saves or discards changes to controls

        // Functions for screen and graphics OnClick changes
        public delegate void EntryChanged(int a);
        public event EntryChanged OnResolutionSelection, OnScreenmodeSelection, OnQualitySelection;
        private Rect lastSetPixelRect;

        protected override void Awake()
        {
            base.Awake();

            instance = this;
            FindElements();
        }

        public void Update()
        {
            DynamicCamera.ControlEnabled = !robotControlPanel.activeSelf || !globalControlPanel.activeSelf;
            InputControl.freeze = robotControlPanel.activeSelf || globalControlPanel.activeSelf;

            Resize();
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
            menuPanelInner = Auxiliary.FindObject(Auxiliary.FindObject(canvas, "MenuPanel"), "Panel");

            robotControlPanel = Auxiliary.FindObject(menuPanelInner, "RobotControlPanel");
            globalControlPanel = Auxiliary.FindObject(menuPanelInner, "GlobalControlPanel");
            settingsPanel = Auxiliary.FindObject(menuPanelInner, "SettingsPanel");
            viewReplaysPanel = Auxiliary.FindObject(menuPanelInner, "LoadReplayPanel");
            helpPanel = Auxiliary.FindObject(menuPanelInner, "HelpPanel");

            checkSavePanel = Auxiliary.FindObject(canvas, "CheckSavePanel");

            settings = settingsPanel.GetComponent<UserSettings>();
            loadReplay = viewReplaysPanel.GetComponent<LoadReplay>();
        }

        #region robot controls

        public void SwitchRobotControls()
        {
            EndOtherProcesses();

            if (!robotControlPanel.activeSelf)
            {
                Controls.Load();
                robotControlPanel.SetActive(true);
                Auxiliary.FindObject(Auxiliary.FindObject(Auxiliary.FindObject(robotControlPanel, "SettingsMode"), "ScrollRect"), "Content").GetComponent<CreateButton>().CreateButtons();
            }
            else
            {
                CheckUnsavedControls(() =>
                {
                    robotControlPanel.SetActive(false);
                });
            }
        }

        public void CheckUnsavedControls(Action callback)
        {
            ProcessControlsCallback = callback;
            if (!Controls.HasBeenSaved())
            {
                checkSavePanel.SetActive(true);
            }
            else if (ProcessControlsCallback != null)
            {
                ProcessControlsCallback.Invoke();
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
                    break;
                default:
                case "cancel":
                    return;
            }
            if (ProcessControlsCallback != null)
                ProcessControlsCallback.Invoke();
        }

        #endregion
        #region global controls

        public void SwitchGlobalControls()
        {
            EndOtherProcesses();
            if (!globalControlPanel.activeSelf)
            {
                Controls.Load();
                globalControlPanel.SetActive(true);
                Auxiliary.FindObject(Auxiliary.FindObject(globalControlPanel, "ScrollRect"), "Content").GetComponent<CreateButton>().CreateButtons();
            }
            else
            {
                CheckUnsavedControls(() =>
                {
                    globalControlPanel.SetActive(false);
                });
            }
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
            }
            else
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
            }
            else
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

        private void Resize(bool forceUpdate = false)
        {
            if (!lastSetPixelRect.Equals(UnityEngine.Camera.main.pixelRect) || forceUpdate)
            {

            }
        }
    }
}