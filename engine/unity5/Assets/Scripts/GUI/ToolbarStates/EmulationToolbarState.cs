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

        bool loaded = false;
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
            useEmulationButtonImage.color = Color.green;

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

                if (loaded)
                {
                    t.text = "Loading...";
                    loadingPanel.SetActive(false);
                    loaded = false;
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
                LoadCode();
                AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                    AnalyticsLedger.EventAction.Clicked,
                    "Emulation Select Code",
                    AnalyticsLedger.getMilliseconds().ToString());
            }
        }

        public async void LoadCode()
        {
            string[] selectedFiles = SFB.StandaloneFileBrowser.OpenFilePanel("Robot Code Executable", "C:\\", "", false);
            if (selectedFiles.Length != 1)
            {
                Debug.Log("No files selected for robot code upload");
            }
            else
            {
                UserProgram userProgram = new UserProgram(selectedFiles[0]);
                PlayerPrefs.SetString("UserProgramType", userProgram.ProgramType.ToString());

                loadingPanel.SetActive(true);
                Task Upload = Task.Factory.StartNew(async () =>
                {
                    await EmulatorManager.SCPFileSender(userProgram);
                    loaded = true;
                });

                await Upload;
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

            // TODO
            RobotIOPanel.Instance.Toggle();
        }

        public void OnVMConnectionStatusClicked()
        {
            if (EmulationWarnings.CheckRequirement((EmulationWarnings.Requirement.VMInstalled)) && !EmulatorManager.IsVMRunning() && !EmulatorManager.IsVMConnected())
            {
                if (!EmulatorManager.StartEmulator())
                    UserMessageManager.Dispatch("Emulator failed to start.", EmulationWarnings.WARNING_DURATION);
            }

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                AnalyticsLedger.EventAction.Clicked,
                "Emulation Start",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        public override void ToggleHidden()
        {
            emulationToolbar.SetActive(!emulationToolbar.activeSelf);
        }
    }
}
