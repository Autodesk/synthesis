using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.Sensors;
using Synthesis.Camera;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// Menu toolbar
    /// </summary>
    public class MenuToolbarState : State
    {
        GameObject canvas;

        GameObject menuPanel;

        GameObject robotControlPanel;
        GameObject globalControlPanel;
        GameObject settingsPanel;
        GameObject viewReplaysPanel;
        GameObject helpPanel;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            
            menuPanel = Auxiliary.FindObject(canvas, "MenuPanel");

            robotControlPanel = Auxiliary.FindObject(canvas, "RobotControlPanel");
            globalControlPanel = Auxiliary.FindObject(canvas, "GlobalControlPanel");
            settingsPanel = Auxiliary.FindObject(canvas, "SettingsPanel");
            viewReplaysPanel = Auxiliary.FindObject(canvas, "LoadReplayPanel");
            helpPanel = Auxiliary.FindObject(canvas, "HelpPanel");
            
            robotControlPanel.SetActive(true);
        }

        private void CloseAll()
        {
            robotControlPanel.SetActive(false);
            globalControlPanel.SetActive(false);
            settingsPanel.SetActive(false);
            viewReplaysPanel.SetActive(false);
            helpPanel.SetActive(false);
        }

        private void SwitchMenu(GameObject newMenu)
        {
            CloseAll();
            newMenu.SetActive(true);
        }

        /// <summary>
        /// Opens the robot controls panel
        /// </summary>
        public void OnRobotControlsButtonClicked()
        {
            SwitchMenu(robotControlPanel);
        }

        /// <summary>
        /// Opens the global controls panel
        /// </summary>
        public void OnGlobalControlsButtonClicked()
        {
            SwitchMenu(globalControlPanel);
        }

        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            SwitchMenu(settingsPanel);
        }

        /// <summary>
        /// Opens the view replays panel
        /// </summary>
        public void OnViewReplaysButtonClicked()
        {
            SwitchMenu(viewReplaysPanel);
        }

        /// <summary>
        /// Opens the help panel
        /// </summary>
        public void OnHelpButtonClicked()
        {
            SwitchMenu(helpPanel);
        }

        public override void End()
        {
            CloseAll();
        }

    }
}