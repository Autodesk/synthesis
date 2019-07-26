using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Threading.Tasks;
using static Synthesis.EmulatorManager;
using System;

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
        GameObject tabs;
        GameObject emulationToolbar;
        GameObject loadingPanel = null;

        public override void Start()
        {
            canvas = GameObject.Find("Canvas");
            tabs = Auxiliary.FindObject(canvas, "Tabs");
            emulationToolbar = Auxiliary.FindObject(canvas, "EmulationToolbar");
            loadingPanel = Auxiliary.FindObject(canvas, "LoadingPanel");
        }

        public void OnDestroy()
        {
            if (Synthesis.EmulatorManager.IsRunningRobotCode())
                EmulationDriverStation.Instance.StopRobotCode();
        }

        public override void FixedUpdate()
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

        /// <summary>
        /// Selects robot code and starts VM. 
        /// </summary>
        public void OnSelectRobotCodeButtonClicked()
        {
            if (Synthesis.EmulationWarnings.CheckRequirement((Synthesis.EmulationWarnings.Requirement.VMConnected)))
            {
                LoadCode();
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
                Synthesis.UserProgram userProgram = new Synthesis.UserProgram(selectedFiles[0]);
                loadingPanel.SetActive(true);
                Task Upload = Task.Factory.StartNew(() =>
                {
                    Synthesis.EmulatorManager.SCPFileSender(userProgram);
                    loaded = true;
                });

                await Upload;
                PlayerPrefs.SetString("UserProgramType", Enum.GetName(typeof(Synthesis.UserProgram.UserProgramType), programType));
            }

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                AnalyticsLedger.EventAction.Clicked,
                "Select Code",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        private void UserMessageManager(string v)
        {
            throw new NotImplementedException();
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

        public void OnStartRobotCodeButtonClicked()
        {
            EmulationDriverStation.Instance.ToggleRobotCodeButton();

            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.EmulationTab,
                AnalyticsLedger.EventAction.Clicked,
                "Run Code",
                AnalyticsLedger.getMilliseconds().ToString());
        }

        public void OnVMConnectionStatusClicked()
        {
            if (!Synthesis.EmulatorManager.IsVMRunning() && !Synthesis.EmulatorManager.IsVMConnected())
            {
                Synthesis.EmulatorManager.StartEmulator();
                EmulationDriverStation.Instance.BeginTrackingVMConnectionStatus();
            }
        }

        public override void ToggleHidden()
        {
            emulationToolbar.SetActive(!emulationToolbar.activeSelf);
        }

    }
}
