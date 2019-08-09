using UnityEngine;
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

        // Robot controls
        // Global Controls
        // Settings
        // View Replays
        // Help

        public void Start()
        {
            canvas = GameObject.Find("Canvas");

            robotControlPanel = Auxiliary.FindObject(canvas, "RobotControlPanel");
            globalControlPanel = Auxiliary.FindObject(canvas, "GlobalControlPanel");
            settingsPanel = Auxiliary.FindObject(canvas, "SettingsPanel");
            viewReplaysPanel = Auxiliary.FindObject(canvas, "LoadReplayPanel");
            helpPanel = Auxiliary.FindObject(canvas, "HelpPanel");
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
            settingsPanel.SetActive(true);
        }

        #endregion
        #region view replays

        public void SwitchViewReplays()
        {
            EndOtherProcesses();
            viewReplaysPanel.SetActive(true);
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