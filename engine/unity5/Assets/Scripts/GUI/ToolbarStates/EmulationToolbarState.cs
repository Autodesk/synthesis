using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Input;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// This state controls the emulation toolbar and interface to starting emulation robot code.
    /// </summary>
    public class EmulationToolbarState : State
    {
        EmulationDriverStation emulationDriverStation;

        GameObject canvas;
        GameObject tabs;
        GameObject emulationToolbar;

        GameObject helpMenu;
        GameObject overlay;
        Text helpBodyText;

        public override void Start()
        {
            emulationDriverStation = StateMachine.SceneGlobal.GetComponent<EmulationDriverStation>();

            canvas = GameObject.Find("Canvas");
            tabs = Auxiliary.FindObject(canvas, "Tabs");
            emulationToolbar = Auxiliary.FindObject(canvas, "EmulationToolbar");

            helpMenu = Auxiliary.FindObject(canvas, "Help");
            overlay = Auxiliary.FindObject(canvas, "Overlay");
            helpBodyText = Auxiliary.FindObject(canvas, "BodyText").GetComponent<Text>();

            Button helpButton = Auxiliary.FindObject(helpMenu, "CloseHelpButton").GetComponent<Button>();
            helpButton.onClick.RemoveAllListeners();
            helpButton.onClick.AddListener(CloseHelpMenu);
        }

        /// <summary>
        /// Selects robot code and starts VM. 
        /// </summary>
        public void OnSelectRobotCodeButtonPressed()
        {
            SSHClient.SCPFileSender();
        }

        /// <summary>
        /// Opens the Synthesis Driver Station for emulation
        /// </summary>
        public void OnDriverStationButtonPressed()
        {
            emulationDriverStation.OpenDriverStation();
        }

        public void OnStartRobotCodeButtonPressed()
        {
            emulationDriverStation.ToggleRobotCodeButton();
            SSHClient.StartRobotCode();
            //Serialization.RestartThreads("10.140.148.66");
        }

        #region Help Button and Menu
        public void OnHelpButtonPressed()
        {
            helpMenu.SetActive(true);

            // Used to change the text of emulation help menu
            helpBodyText.GetComponent<Text>().text = "\n\nStart Code: Select your code file" +
                "\n\nDriver Station: Access the Synthesis FRC Driver Station to manipulate your robot" +
                "\n\nStart Code/ Stop Code: Run or disable code";

            Auxiliary.FindObject(helpMenu, "Type").GetComponent<Text>().text = "EmulationToolbar";
            overlay.SetActive(true);
            tabs.transform.Translate(new Vector3(300, 0, 0));
            foreach (Transform t in emulationToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(300, 0, 0));
                else t.gameObject.SetActive(false);
            }

            if (PlayerPrefs.GetInt("analytics") == 1)
            {
                Analytics.CustomEvent("Emulation Help Button Pressed", new Dictionary<string, object> //for analytics tracking
                {
                });
            }

        }

        internal static Serialization s;

        private void CloseHelpMenu()
        {
            helpMenu.SetActive(false);
            overlay.SetActive(false);
            tabs.transform.Translate(new Vector3(-300, 0, 0));
            foreach (Transform t in emulationToolbar.transform)
            {
                if (t.gameObject.name != "HelpButton") t.Translate(new Vector3(-300, 0, 0));
                else t.gameObject.SetActive(true);
            }
        }
        #endregion
    }
}