using Synthesis.FSM;
using Synthesis.Input;
using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    class EmulationDriverStation : MonoBehaviour
    {
        public static EmulationDriverStation Instance { get; private set; }

        public enum DriveState
        {
            Auto,
            Teleop,
            Test,
        };

        public enum AllianceStation
        {
            Red1,
            Red2,
            Red3,
            Blue1,
            Blue2,
            Blue3,
        };

        public DriveState state;
        public AllianceStation allianceStation;

        public bool isRobotDisabled = false;
        public bool isRunCode = false;

        GameObject canvas;
        InputField gameDataInput;
        GameObject emuDriverStationPanel;
        GameObject runButton;

        // Sprites for emulation coloring details
        // Tethered in Unity > Simulator > Attached to the EmulationDriverStation script
        public Sprite HighlightColor;
        public Sprite DefaultColor;
        public Sprite EnableColor;
        public Sprite DisableColor;
        public Sprite StartCode;
        public Sprite StopCode;

        Image startImage;
        Image stopImage;

        public static string emulationDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Autodesk\Synthesis\Emulator\");

        private void Start()
        {
            canvas = GameObject.Find("Canvas");
            gameDataInput = Auxiliary.FindObject(canvas, "InputField").GetComponent<InputField>();
            emuDriverStationPanel = Auxiliary.FindObject(canvas, "EmulationDriverStation");
            runButton = Auxiliary.FindObject(canvas, "StartRobotCodeButton");
            GameData();
        }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Opens the emulation driver station
        /// </summary>
        public void OpenDriverStation()
        {
            if (emuDriverStationPanel.activeSelf == true)
            {
                emuDriverStationPanel.SetActive(false);
                InputControl.freeze = false;

                if (PlayerPrefs.GetInt("analytics") == 1)
                {
                    Analytics.CustomEvent("Opened Driver Station", new Dictionary<string, object> //for analytics tracking
                    {
                    });
                }

            }
            else
            {
                emuDriverStationPanel.SetActive(true);
                InputControl.freeze = true;
                RobotState("teleop");
                RobotDisabled();
            }
        }

        /// <summary>
        /// Toggle button for run/stop code toolbar button
        /// </summary>
        public void ToggleRobotCodeButton()
        {
            if (!isRunCode)
            {
                runButton.GetComponentInChildren<Text>().text = "Stop Code";
                GameObject.Find("CodeImage").GetComponentInChildren<Image>().sprite = StopCode;
                isRunCode = true;
            }
            else
            {
                runButton.GetComponentInChildren<Text>().text = "Run Code";
                GameObject.Find("CodeImage").GetComponentInChildren<Image>().sprite = StartCode;
                isRunCode = false;
            }
        }

        /// <summary>
        /// Selected state for the driver station
        /// </summary>
        /// <param name="theState"></param>
        public void RobotState(string theState)
        {
            switch (theState)
            {
                case "teleop":
                    state = DriveState.Teleop;
                    Debug.Log(state);
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = HighlightColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = DefaultColor;
                    break;
                case "auto":
                    state = DriveState.Auto;
                    Debug.Log(state);
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = HighlightColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = DefaultColor;
                    break;
                case "test":
                    state = DriveState.Test;
                    Debug.Log(state);
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = HighlightColor;
                    break;
                default:
                    state = DriveState.Teleop;
                    Debug.Log(state);
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = HighlightColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = DefaultColor;
                    break;
            }
        }

        public void RobotEnabled()
        {
            isRobotDisabled = false;
            Debug.Log(isRobotDisabled);
            GameObject.Find("Enable").GetComponent<Image>().sprite = EnableColor;
            GameObject.Find("Disable").GetComponent<Image>().sprite = DefaultColor;
        }

        public void RobotDisabled()
        {
            isRobotDisabled = true;
            Debug.Log(isRobotDisabled);
            GameObject.Find("Enable").GetComponent<Image>().sprite = DefaultColor;
            GameObject.Find("Disable").GetComponent<Image>().sprite = DisableColor;
        }

        /// <summary>
        /// Selected team alliance station
        /// </summary>
        /// <param name="teamStation"></param>
        public void TeamStation(int teamStation)
        {
            switch (teamStation)
            {
                case 0:
                    allianceStation = AllianceStation.Red1;
                    Debug.Log(allianceStation);
                    break;
                case 1:
                    allianceStation = AllianceStation.Red2;
                    Debug.Log(allianceStation);
                    break;
                case 2:
                    allianceStation = AllianceStation.Red3;
                    Debug.Log(allianceStation);
                    break;
                case 3:
                    allianceStation = AllianceStation.Blue1;
                    Debug.Log(allianceStation);
                    break;
                case 4:
                    allianceStation = AllianceStation.Blue2;
                    Debug.Log(allianceStation);
                    break;
                case 5:
                    allianceStation = AllianceStation.Blue3;
                    Debug.Log(allianceStation);
                    break;
                default:
                    allianceStation = AllianceStation.Red1;
                    break;
            }
        }

        /// <summary>
        /// A game specific message specified by the user
        /// </summary>
        public void GameData()
        {
            gameDataInput = Auxiliary.FindObject(canvas, "InputField").GetComponent<InputField>();
            gameDataInput.onValueChanged.AddListener(delegate { Debug.Log(gameDataInput.text.ToString()); });
        }
    }
}
