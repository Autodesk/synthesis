using Synthesis.Input;
using Synthesis.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.GUI
{
    class EmulationDriverStation : MonoBehaviour
    {
        public static EmulationDriverStation Instance { get; private set; }
        private bool VMConnectionCoroutineRunning = false;

        private bool lastGameSpecificMessageFocused = false;

        GameObject canvas;
        InputField gameSpecificMessage;
        GameObject emuDriverStationPanel;

        GameObject killEmulatorPanel;
        Text VMConnectionStatusMessage;
        Image VMConnectionStatusImage;

        Image enableRobotImage;
        Image disableRobotImage;

        Image VMConnectionImage;
        Image robotCodeUploadedImage;
        Image robotCodeRunningImage;
        Image robotCodeConnectedImage;

        private const int AUTONOMOUS_LENGTH = 15 * 1000; // ms

        // Sprites for emulation coloring details
        // Tethered in Unity > Simulator > Attached to the EmulationDriverStation script
        public Sprite HighlightColor;
        public Sprite DefaultColor;
        public Sprite EnableColor;
        public Sprite DisableColor;
        public Sprite EmulatorNotInstalled;
        public Sprite StartEmulator;
        public Sprite EmulatorConnection;
        public Sprite StartCode;
        public Sprite StopCode;

        public Sprite StatusGood;
        public Sprite StatusBad;

        private bool runPracticeMode = false;

        public void Start()
        {
            canvas = GameObject.Find("Canvas");
            emuDriverStationPanel = Auxiliary.FindObject(canvas, "EmulationDriverStation");
            killEmulatorPanel = Auxiliary.FindObject(canvas, "KillEmulatorPanel");

            gameSpecificMessage = Auxiliary.FindObject(emuDriverStationPanel, "InputField").GetComponent<InputField>();

            VMConnectionStatusMessage = Auxiliary.FindObject(canvas, "VMConnectionStatus").GetComponentInChildren<Text>();
            VMConnectionStatusImage = Auxiliary.FindObject(canvas, "VMConnectionStatusImage").GetComponentInChildren<Image>();

            enableRobotImage = Auxiliary.FindObject(emuDriverStationPanel, "Enable").GetComponentInChildren<Image>();
            disableRobotImage = Auxiliary.FindObject(emuDriverStationPanel, "Disable").GetComponentInChildren<Image>();

            VMConnectionImage = Auxiliary.FindObject(emuDriverStationPanel, "VMConnectionImage").GetComponentInChildren<Image>();
            robotCodeUploadedImage = Auxiliary.FindObject(emuDriverStationPanel, "RobotCodeUploadedImage").GetComponentInChildren<Image>();
            robotCodeRunningImage = Auxiliary.FindObject(emuDriverStationPanel, "RobotCodeRunningImage").GetComponentInChildren<Image>();
            robotCodeConnectedImage = Auxiliary.FindObject(emuDriverStationPanel, "RobotCodeConnectedImage").GetComponentInChildren<Image>();

            RobotState("teleop");
            RobotDisabled();
            BeginTrackingVMConnectionStatus();
        }

        public void Awake()
        {
            Instance = this;
        }

        public void DisableDrag()
        {
            DynamicCamera.MovementEnabled = false;
        }

        public void EnableDrag()
        {
            DynamicCamera.MovementEnabled = true;
        }

        public void Update()
        {
            if (lastGameSpecificMessageFocused != gameSpecificMessage.isFocused)
            {
                InputControl.freeze = gameSpecificMessage.isFocused;
                lastGameSpecificMessageFocused = gameSpecificMessage.isFocused;
            }
            VMConnectionImage.sprite = EmulatorManager.IsVMConnected() ? StatusGood : StatusBad;
            robotCodeUploadedImage.sprite = EmulatorManager.IsFRCUserProgramPresent() ? StatusGood : StatusBad;
            robotCodeRunningImage.sprite = EmulatorManager.IsRunningRobotCode() ? StatusGood : StatusBad;
            robotCodeConnectedImage.sprite = EmulatorNetworkConnection.Instance.IsConnected() ? StatusGood : StatusBad;
        }

        /// <summary>
        /// Opens the emulation driver station
        /// </summary>
        public void ToggleDriverStation()
        {
            emuDriverStationPanel.SetActive(!emuDriverStationPanel.activeSelf);
        }

        public void SetActive(bool active)
        {
            emuDriverStationPanel.SetActive(active);
        }

        /// <summary>
        /// Indicator for VM connection status
        /// </summary>
        private System.Collections.IEnumerator UpdateVMConnectionStatus() // TODO move to emulation toolbar
        {
            while (true)
            {
                if (EmulatorManager.IsVMConnected())
                {
                    VMConnectionStatusImage.sprite = EmulatorConnection;
                    if (EmulatorNetworkConnection.Instance.IsConnected())
                    {
                        VMConnectionStatusMessage.text = "Connected";
                    }
                    else
                    {
                        VMConnectionStatusMessage.text = "Ready";
                        RobotDisabled();
                    }
                    if (!EmulatorManager.IsRunningRobotCodeRunner() && !EmulatorManager.IsTryingToRunRobotCode() && !EmulatorManager.IsRobotCodeRestarting())
                    {
                        EmulatorManager.RestartRobotCode();
                    }
                }
                else
                {
                    if (!EmulatorManager.IsVMInstalled())
                    {
                        VMConnectionStatusImage.sprite = EmulatorNotInstalled;
                        VMConnectionStatusMessage.text = "Not Installed";
                    }
                    else if (!EmulatorManager.IsVMRunning())
                    {
                        VMConnectionStatusImage.sprite = StartEmulator;
                        VMConnectionStatusMessage.text = "Start Emulator";
                    }
                    else
                    {
                        VMConnectionStatusImage.sprite = EmulatorConnection;
                        VMConnectionStatusMessage.text = "Starting";
                    }
                    RobotDisabled();
                }
                yield return new WaitForSeconds(1.0f); // s
            }
        }

        public void BeginTrackingVMConnectionStatus()
        {
            if (!VMConnectionCoroutineRunning){
                StartCoroutine(UpdateVMConnectionStatus());
                VMConnectionCoroutineRunning = true;
            }
        }

        public async void RestartRobotCode()
        {
            if (EmulationWarnings.CheckRequirement(EmulationWarnings.Requirement.UserProgramPresent) && EmulationWarnings.CheckRequirement(EmulationWarnings.Requirement.UserProgramNotRestarting))
            {
                bool success = await EmulatorManager.RestartRobotCode();
                if (!success)
                {
                    UserMessageManager.Dispatch("Failed to restart user program", EmulationWarnings.WARNING_DURATION);
                }
            }
        }

        private async void RunPracticeMode()
        {
            EmulatedRoboRIO.RobotInputs.RobotMode.Mode = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Autonomous;
            await Task.Delay(AUTONOMOUS_LENGTH);
            if (!EmulatedRoboRIO.RobotInputs.RobotMode.Enabled)
            {
                return;
            }
            EmulatedRoboRIO.RobotInputs.RobotMode.Mode = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Teleop;
        }

        /// <summary>
        /// Selected state for the driver station
        /// </summary>
        /// <param name="theState"></param>
        public void RobotState(string theState)
        {
            EmulationService.RobotInputs.Types.RobotMode.Types.Mode last_mode = EmulatedRoboRIO.RobotInputs.RobotMode.Mode;

            Auxiliary.FindObject(canvas, "TeleOp").GetComponentInChildren<Image>().sprite = DefaultColor;
            Auxiliary.FindObject(canvas, "Auto").GetComponentInChildren<Image>().sprite = DefaultColor;
            Auxiliary.FindObject(canvas, "Practice").GetComponentInChildren<Image>().sprite = DefaultColor;
            Auxiliary.FindObject(canvas, "Test").GetComponentInChildren<Image>().sprite = DefaultColor;

            bool lastRunPracticeMode = runPracticeMode;
            runPracticeMode = theState == "practice";
            switch (theState)
            {
                case "auto":
                    EmulatedRoboRIO.RobotInputs.RobotMode.Mode = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Autonomous;
                    Auxiliary.FindObject(canvas, "Auto").GetComponentInChildren<Image>().sprite = HighlightColor;
                    break;
                case "practice":
                    EmulatedRoboRIO.RobotInputs.RobotMode.Mode = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Autonomous;
                    Auxiliary.FindObject(canvas, "Practice").GetComponentInChildren<Image>().sprite = HighlightColor;
                    break;
                case "test":
                    EmulatedRoboRIO.RobotInputs.RobotMode.Mode = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Test;
                    Auxiliary.FindObject(canvas, "Test").GetComponentInChildren<Image>().sprite = HighlightColor;
                    break;
                case "teleop":
                default:
                    EmulatedRoboRIO.RobotInputs.RobotMode.Mode = EmulationService.RobotInputs.Types.RobotMode.Types.Mode.Teleop;
                    Auxiliary.FindObject(canvas, "TeleOp").GetComponentInChildren<Image>().sprite = HighlightColor;
                    break;
            }
            if(EmulatedRoboRIO.RobotInputs.RobotMode.Mode != last_mode || lastRunPracticeMode != runPracticeMode)
                RobotDisabled();
            else if (runPracticeMode && EmulatedRoboRIO.RobotInputs.RobotMode.Enabled)
                Task.Run(RunPracticeMode);
        }

        public void RobotEnabled()
        {
            if(EmulationWarnings.CheckRequirement((EmulationWarnings.Requirement.UseEmulation)) && EmulationWarnings.CheckRequirement((EmulationWarnings.Requirement.UserProgramConnected)))
            {
                if (!EmulatedRoboRIO.RobotInputs.RobotMode.Enabled && runPracticeMode)
                {
                    Task.Run(RunPracticeMode);
                }
                EmulatedRoboRIO.RobotInputs.RobotMode.Enabled = true;
                enableRobotImage.sprite = EnableColor;
                disableRobotImage.sprite = DefaultColor;
            }
        }

        public void RobotDisabled()
        {
            EmulatedRoboRIO.RobotInputs.RobotMode.Enabled = false;
            enableRobotImage.sprite = DefaultColor;
            disableRobotImage.sprite = DisableColor;
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
                    EmulatedRoboRIO.RobotInputs.MatchInfo.AllianceStationId = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red2;
                    break;
                case 2:
                    EmulatedRoboRIO.RobotInputs.MatchInfo.AllianceStationId = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red3;
                    break;
                case 3:
                    EmulatedRoboRIO.RobotInputs.MatchInfo.AllianceStationId = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Blue1;
                    break;
                case 4:
                    EmulatedRoboRIO.RobotInputs.MatchInfo.AllianceStationId = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Blue2;
                    break;
                case 5:
                    EmulatedRoboRIO.RobotInputs.MatchInfo.AllianceStationId = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Blue3;
                    break;
                case 0:
                default:
                    EmulatedRoboRIO.RobotInputs.MatchInfo.AllianceStationId = EmulationService.RobotInputs.Types.MatchInfo.Types.AllianceStationID.Red1;
                    break;
            }
        }

        public void FinishGameSpecificMessage()
        {
            EmulatedRoboRIO.RobotInputs.MatchInfo.GameSpecificMessage = gameSpecificMessage.text;
        }

        public string GetGameSpecificMessage()
        {
            return gameSpecificMessage.text;
        }

        public void SetKillEmulatorDialogActive(bool active)
        {
            killEmulatorPanel.SetActive(active);
        }

        public void KillEmulator()
        {
            EmulatorManager.KillEmulator();
        }
    }
}
