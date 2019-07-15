using Synthesis.Input;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    class EmulationDriverStation : MonoBehaviour
    {
        public static EmulationDriverStation Instance { get; private set; }

        public EmulationService.RobotInputs.Types.RobotMode.Types.Mode state;
        public EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID allianceStation;

        public bool isRobotEnabled = false;
        public bool isRunCode = false;

        GameObject canvas;
        InputField gameSpecificMessage;
        GameObject emuDriverStationPanel;
        GameObject javaEmulationNotSupportedPopUp; // TODO remove this once support is added
        GameObject runButton;
        UnityEngine.UI.Text VMConnectionStatusMessage;

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
            gameSpecificMessage = Auxiliary.FindObject(canvas, "InputField").GetComponent<InputField>();
            emuDriverStationPanel = Auxiliary.FindObject(canvas, "EmulationDriverStation");
            javaEmulationNotSupportedPopUp = Auxiliary.FindObject(canvas, "JavaEmulationNotSupportedPopUp");
            runButton = Auxiliary.FindObject(canvas, "StartRobotCodeButton");
            VMConnectionStatusMessage = Auxiliary.FindObject(canvas, "VMConnectionStatus").GetComponentInChildren<Text>();

            StartCoroutine(UpdateVMConnectionStatus());
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
        /// Displays dialogue that Java emulation is not currently supproted (WPILib v2019)
        /// </summary>
        public void ShowJavaNotSupportedPopUp()
        {
            javaEmulationNotSupportedPopUp.SetActive(true);
        }

        /// <summary>
        /// Close dialogue that displays Java emulation is not currently supproted
        /// </summary>
        public void CloseJavaNotSupportedPopUp()
        {
            javaEmulationNotSupportedPopUp.SetActive(false);
        }

        /// <summary>
        /// Indicator for VM connection status
        /// </summary>
        public System.Collections.IEnumerator UpdateVMConnectionStatus()
        {
            while (true)
            {
                if (SSHClient.IsVMConnected())
                {
                    VMConnectionStatusMessage.text = "Connected";
                    yield return new WaitForSeconds(15.0f); // s
                }
                else
                {
                    VMConnectionStatusMessage.text = "Connecting";
                    yield return new WaitForSeconds(3.0f); // s
                }
            }
        }

        /// <summary>
        /// Toggle button for run/stop code toolbar button
        /// </summary>
        public void ToggleRobotCodeButton()
        {
            if(!SSHClient.IsVMConnected())
            {
                return;
            }
            if (!isRunCode) // Start robot code
            {
                runButton.GetComponentInChildren<Text>().text = "Stop Code";
                GameObject.Find("CodeImage").GetComponentInChildren<Image>().sprite = StopCode;
                isRunCode = true;
                SSHClient.StartRobotCode();
            }
            else // Stop robot code
            {
                runButton.GetComponentInChildren<Text>().text = "Run Code";
                GameObject.Find("CodeImage").GetComponentInChildren<Image>().sprite = StartCode;
                isRunCode = false;
                SSHClient.StopRobotCode();
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
                case "auto":
                    state = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Autonomous;
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = HighlightColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = DefaultColor;
                    break;
                case "test":
                    state = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Test;
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = HighlightColor;
                    break;
                case "teleop":
                default:
                    state = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Teleop;
                    GameObject.Find("TeleOp").GetComponent<Image>().sprite = HighlightColor;
                    GameObject.Find("Auto").GetComponent<Image>().sprite = DefaultColor;
                    GameObject.Find("Test").GetComponent<Image>().sprite = DefaultColor;
                    break;
            }
        }

        public void RobotEnabled()
        {
            isRobotEnabled = true;
            GameObject.Find("Enable").GetComponent<Image>().sprite = EnableColor;
            GameObject.Find("Disable").GetComponent<Image>().sprite = DefaultColor;
        }

        public void RobotDisabled()
        {
            isRobotEnabled = false;
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
                case 1:
                    allianceStation = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red2;
                    break;
                case 2:
                    allianceStation = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red3;
                    break;
                case 3:
                    allianceStation = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Blue1;
                    break;
                case 4:
                    allianceStation = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Blue2;
                    break;
                case 5:
                    allianceStation = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Blue3;
                    break;
                case 0:
                default:
                    allianceStation = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red1;
                    break;
            }
            Debug.Log(allianceStation);
        }

        /// <summary>
        /// A game specific message specified by the user
        /// </summary>
        public void GameData()
        {
            gameSpecificMessage = Auxiliary.FindObject(canvas, "InputField").GetComponent<InputField>();
            gameSpecificMessage.onValueChanged.AddListener(delegate { Debug.Log(gameSpecificMessage.text.ToString()); });
        }

        public string GetGameSpecificMessage()
        {
            return gameSpecificMessage.text;
        }
    }
}
