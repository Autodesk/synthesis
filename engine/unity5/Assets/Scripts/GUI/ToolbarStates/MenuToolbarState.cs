using Synthesis.FSM;
using Synthesis.States;
using Synthesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using Synthesis.Sensors;
using Synthesis.Camera;
using Synthesis.GUI;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// Menu toolbar
    /// </summary>
    public class MenuToolbarState : State
    {
        public enum TabState
        {
            RobotControls,
            GlobalControls,
            Settings,
            ViewReplays,
            Help
        }

        GameObject canvas;
        GameObject sideBar;

        private MenuUI menuUI;
        private TabState tabState;

        private Image robotControlsButtonImage;
        private Image globalControlsButtonImage;
        private Image settingsButtonImage;
        private Image viewReplaysButtonImage;
        private Image helpButtonImage;

        private Sprite selectedButtonImage;
        private Sprite unselectedButtonImage;

        public override void Awake()
        {
            canvas = GameObject.Find("Canvas");
            sideBar = Auxiliary.FindObject(Auxiliary.FindObject(canvas, "MenuPanel"), "SideBar");

            menuUI = StateMachine.SceneGlobal.GetComponent<MenuUI>();

            robotControlsButtonImage = Auxiliary.FindObject(sideBar, "RobotControlsButton").GetComponent<Image>();
            globalControlsButtonImage = Auxiliary.FindObject(sideBar, "GlobalControlsButton").GetComponent<Image>();
            settingsButtonImage = Auxiliary.FindObject(sideBar, "SettingsButton").GetComponent<Image>();
            viewReplaysButtonImage = Auxiliary.FindObject(sideBar, "ViewReplaysButton").GetComponent<Image>();
            helpButtonImage = Auxiliary.FindObject(sideBar, "HelpButton").GetComponent<Image>();

            selectedButtonImage = Resources.Load<Sprite>("Images/New Textures/greenButton");
            unselectedButtonImage = Resources.Load<Sprite>("Images/New Textures/TopbarHighlight");
        }

        public override void Start()
        {
            base.Start();

            OnRobotControlsButtonClicked();
        }

        public override void Update()
        {
            base.Update();

            robotControlsButtonImage.sprite = (tabState == TabState.RobotControls) ? selectedButtonImage : unselectedButtonImage;
            globalControlsButtonImage.sprite = (tabState == TabState.GlobalControls) ? selectedButtonImage : unselectedButtonImage;
            settingsButtonImage.sprite = (tabState == TabState.Settings) ? selectedButtonImage : unselectedButtonImage;
            viewReplaysButtonImage.sprite = (tabState == TabState.ViewReplays) ? selectedButtonImage : unselectedButtonImage;
            helpButtonImage.sprite = (tabState == TabState.Help) ? selectedButtonImage : unselectedButtonImage; 
        }

        /// <summary>
        /// Opens the robot controls panel
        /// </summary>
        public void OnRobotControlsButtonClicked()
        {
            MenuUI.instance.CheckUnsavedControls(() =>
            {
                tabState = TabState.RobotControls;
                menuUI.SwitchRobotControls();
            });
        }

        /// <summary>
        /// Opens the global controls panel
        /// </summary>
        public void OnGlobalControlsButtonClicked()
        {
            MenuUI.instance.CheckUnsavedControls(() =>
            {
                tabState = TabState.GlobalControls;
                menuUI.SwitchGlobalControls();
            });
        }

        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            MenuUI.instance.CheckUnsavedControls(() =>
            {
                tabState = TabState.Settings;
                menuUI.SwitchSettings();
            });
        }

        /// <summary>
        /// Opens the view replays panel
        /// </summary>
        public void OnViewReplaysButtonClicked()
        {
            MenuUI.instance.CheckUnsavedControls(() =>
            {
                tabState = TabState.ViewReplays;
                menuUI.SwitchViewReplays();
            });
        }

        /// <summary>
        /// Opens the help panel
        /// </summary>
        public void OnHelpButtonClicked()
        {
            MenuUI.instance.CheckUnsavedControls(() =>
            {
                tabState = TabState.Help;
                menuUI.SwitchHelp();
            });
        }

        public override void End()
        {
            menuUI.EndOtherProcesses();
            if (!Synthesis.Input.Controls.HasBeenSaved())
            {
                Synthesis.Input.Controls.Load();
                UserMessageManager.Dispatch("Control changed discarded", 4);
            }
        }
    }
}