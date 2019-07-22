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

        DynamicCamera camera;
        MainState State;

        GameObject menuPanel;

        GameObject controlPanel;
        GameObject settingsPanel;
        GameObject changeFieldPanel;
        GameObject viewReplaysPanel;
        
        GameObject toolbar;
        GameObject overlay;
        GameObject tabs;

        Text helpBodyText;

        public GameObject currentMenuOpen;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            
            tabs = Auxiliary.FindObject(canvas, "Tabs");
            menuPanel = Auxiliary.FindObject(canvas, "MenuPanel");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            controlPanel = Auxiliary.FindObject(canvas, "ControlPanel");
            settingsPanel = Auxiliary.FindObject(canvas, "SettingsPanel");
            changeFieldPanel = Auxiliary.FindObject(canvas, "ChangeFieldPanel");
            viewReplaysPanel = Auxiliary.FindObject(canvas, "SelectReplayPanel");
            
            // To access instatiate classes within a state, use the StateMachine.SceneGlobal

            State = StateMachine.SceneGlobal.CurrentState as MainState;

            currentMenuOpen = controlPanel;
        }

        public void SwitchMenu(GameObject newMenu)
        {
            currentMenuOpen.SetActive(false);
            newMenu.SetActive(true);
            currentMenuOpen = newMenu;
        }

        /// <summary>
        /// Opens the robot controls panel
        /// </summary>
        public void OnControlPanelButtonClicked()
        {
            SwitchMenu(controlPanel);
        }

        /// <summary>
        /// Opens the settings panel
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            SwitchMenu(settingsPanel);
        }

        /// <summary>
        /// Opens the change field panel
        /// </summary>
        public void OnChangeFieldButtonClicked()
        {
            SwitchMenu(changeFieldPanel);

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.ChangeField,
                AnalyticsLedger.EventAction.Clicked,
                "change",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        /// <summary>
        /// Opens the view replays panel
        /// </summary>
        public void OnViewReplaysButtonClicked()
        {
            SwitchMenu(viewReplaysPanel);
        }

    }
}