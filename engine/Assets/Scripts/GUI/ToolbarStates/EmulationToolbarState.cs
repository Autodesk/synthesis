using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Threading.Tasks;
using System;
using Synthesis;

namespace Assets.Scripts.GUI
{
    /// <summary>
    /// This state controls the emulation toolbar and interface to starting emulation robot code.
    /// </summary>
    public class EmulationToolbarState : State
    {
        public static bool exiting = false;

        bool uploadFinished = false, uploadSuccess = false;
        float lastAdditionalDot = 0;
        int dotCount = 0;

        GameObject canvas;
        GameObject emulationToolbar;
        GameObject loadingPanel = null;

        private Text useEmulationButtonText;
        private Image useEmulationButtonImage;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            emulationToolbar = Auxiliary.FindObject(canvas, "EmulationToolbar");
            loadingPanel = Auxiliary.FindObject(canvas, "LoadingPanel");

            useEmulationButtonText = Auxiliary.FindObject(canvas, "UseEmulationButton").GetComponentInChildren<Text>();
            useEmulationButtonImage = Auxiliary.FindObject(canvas, "UseEmulationImage").GetComponentInChildren<Image>();

            try
            {
                EmulatorManager.programType = (UserProgram.Type)Enum.Parse(typeof(UserProgram.Type), PlayerPrefs.GetString("UserProgramType"), false);
            }
            catch (Exception) { }

            EmulatorManager.StartUpdatingStatus();
            EmulationDriverStation.Instance.BeginTrackingVMConnectionStatus();
        }

        public override void Update()
        {
            if (loadingPanel.activeSelf)
            {
                Text t = loadingPanel.transform.Find("Text").GetComponent<Text>();
                if (!EmulatorManager.IsVMConnected())
                {
                    uploadFinished = true;
                    uploadSuccess = false;
                }
                if (uploadFinished)
                {
                    if (!uploadSuccess)
                    {
                        UserMessageManager.Dispatch("Failed to upload new user program", EmulationWarnings.WARNING_DURATION);
                    }
                    t.text = "Loading...";
                    loadingPanel.SetActive(false);
                    uploadFinished = false;
                }
                else
                {
                    if (Time.unscaledTime >= lastAdditionalDot + 0.75)
                    {
                        dotCount = (dotCount + 1) % 4;
                        t.text = "Loading";
                        for (int i = 0; i < dotCount; i++)
                        {
                            t.text += ".";
                        }
                        lastAdditionalDot = Time.unscaledTime;
                    }
                }
            }
        }

        public void OnUseEmulationButtonClicked()
        {
            EmulatorManager.UseEmulation = !EmulatorManager.UseEmulation;
            if (EmulatorManager.UseEmulation)
            {
                useEmulationButtonImage.sprite = EmulationDriverStation.Instance.StopCode;
                useEmulationButtonImage.color = Color.red;
                useEmulationButtonText.text = "Stop Emulation";

                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                    AnalyticsLedger.EventAction.Clicked,
                    "Emulation Stop",
                    AnalyticsLedger.getMilliseconds().ToString());
            }
            else
            {
                useEmulationButtonImage.sprite = EmulationDriverStation.Instance.StartCode;
                useEmulationButtonImage.color = Color.green;
                useEmulationButtonText.text = "Use Emulation";
                EmulationDriverStation.Instance.RobotDisabled();

                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                    AnalyticsLedger.EventAction.Clicked,
                    "Emulation In-Use",
                    AnalyticsLedger.getMilliseconds().ToString());
            }
        }

        /// <summary>
        /// Selects robot code and starts VM. 
        /// </summary>
        public void OnSelectRobotCodeButtonClicked()
        {
            if (EmulationWarnings.CheckRequirement(EmulationWarnings.Requirement.VMConnected))
            {
                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                    AnalyticsLedger.EventAction.Clicked,
                    "Emulation Select Code",
                    AnalyticsLedger.getMilliseconds().ToString());
                LoadCode();
            }
        }

        public async void LoadCode()
        {
            string[] selectedFiles = SFB.StandaloneFileBrowser.OpenFilePanel("Robot Code Executable", "C:\\", "", false);
            if (selectedFiles.Length != 1)
            {
                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                    AnalyticsLedger.EventAction.CodeType,
                    "No Code",
                    AnalyticsLedger.getMilliseconds().ToString());
            }
            else
            {
                UserProgram userProgram = new UserProgram(selectedFiles[0]);
                if (userProgram.ProgramType == UserProgram.Type.INVALID)
                {
                    UserMessageManager.Dispatch("Invalid user program type", EmulationWarnings.WARNING_DURATION);
                }
                else if (userProgram.Size > EmulatorManager.MAX_FILE_SIZE)
                {
                    UserMessageManager.Dispatch(Math.Round((double)userProgram.Size / (1024 * 1024), 2) + " MB user program is too large (capacity is " + Math.Round((double)EmulatorManager.MAX_FILE_SIZE / (1024 * 1024), 2) + " MB)", EmulationWarnings.WARNING_DURATION);
                }
                else
                {
                    if (userProgram.Size > EmulatorManager.WARNING_FILE_SIZE)
                    {
                        UserMessageManager.Dispatch("Uploading large, " + Math.Round((double)userProgram.Size / (1024 * 1024), 2) + " MB user program. Upload may take a while.", EmulationWarnings.WARNING_DURATION);
                    }
                    PlayerPrefs.SetString("UserProgramType", userProgram.ProgramType.ToString());

                    AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                        AnalyticsLedger.EventAction.CodeType,
                        userProgram.ProgramType.ToString(),
                        AnalyticsLedger.getMilliseconds().ToString());

                    loadingPanel.SetActive(true);
                    uploadSuccess = await EmulatorManager.SCPFileSender(userProgram);
                    uploadFinished = true;
                }
            }
        }

        public void OnDestroy()
        {
            PlayerPrefs.SetString("UserProgramType", EmulatorManager.programType.ToString());
        }

        /// <summary>
        /// Opens the Synthesis Driver Station for emulation
        /// </summary>
        public void OnDriverStationButtonClicked()
        {
            EmulationDriverStation.Instance.ToggleDriverStation();

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                AnalyticsLedger.EventAction.Clicked,
                "Emulation Driver Station",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        public void OnRobotIOPanelButtonClicked()
        {
            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                AnalyticsLedger.EventAction.Clicked,
                "Emulation IO Panel",
                AnalyticsLedger.getMilliseconds().ToString());

            RobotIOPanel.Instance.Toggle();
        }

        public void OnVMConnectionStatusClicked()
        {
            if (EmulationWarnings.CheckRequirement((EmulationWarnings.Requirement.VMInstalled)) && !EmulatorManager.IsVMRunning() && !EmulatorManager.IsVMConnected())
            {
                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                    AnalyticsLedger.EventAction.Clicked,
                    "Emulation Start",
                    AnalyticsLedger.getMilliseconds().ToString());

                if (EmulatorManager.StartEmulator()) // If successful
                {
                    EmulationDriverStation.Instance.SetActive(true);
                }
                else
                {
                    UserMessageManager.Dispatch("Emulator failed to start.", EmulationWarnings.WARNING_DURATION);
                }
            }
            else if (EmulatorManager.IsVMRunning())
            {
                EmulationDriverStation.Instance.SetKillEmulatorDialogActive(true);
            }
        }

        public override void ToggleHidden()
        {
            emulationToolbar.SetActive(!emulationToolbar.activeSelf);
        }
    }
}
