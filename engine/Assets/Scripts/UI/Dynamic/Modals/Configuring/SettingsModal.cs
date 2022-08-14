using System.Collections;
using System.Collections.Generic;
using Synthesis.UI.Dynamic;
using UnityEngine;
using TMPro;
using System;
using System.Runtime.InteropServices;
using System.Linq;

namespace Synthesis.UI.Dynamic {
    public class SettingsModal : ModalDynamic {
        public SettingsModal() : base(new Vector2(500, 500)) { }
        
        public Func<UIComponent, UIComponent> VerticalLayout = (u) => {
            var offset = (-u.Parent!.RectOfChildren(u).yMin) + 7.5f;
            u.SetTopStretch<UIComponent>(anchoredY: offset, leftPadding: 15f);
            return u;
        };
        public override void Create() {
            
            Title.SetText("Settings");
            Description.SetText("Select one of the settings in order to change simulation settings");

            AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
                SaveSettings();
                DynamicUIManager.CloseActiveModal();
            });
            CancelButton.StepIntoLabel(b => { b.SetText("Close"); }).AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal());
            
            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).ApplyTemplate(VerticalLayout).SetText("Screen Settings");

            var screenModeDropdown = MainContent.CreateLabeledDropdown().ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Screen Mode"))
                .StepIntoDropdown(
                    d => d.SetOptions(ScreenModeList)
                    .AddOnValueChangedEvent((d, i, o) => { 
                        Set(SCREEN_MODE, i);
                    })
                    .SetValue(Get<int>(SCREEN_MODE)));

            var qualitySettingsDropdown = MainContent.CreateLabeledDropdown().ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Quality Settings"))
                .StepIntoDropdown(
                    d => d.SetOptions(QualitySettingsList)
                    .AddOnValueChangedEvent((d, i, o) => Set(QUALITY_SETTINGS,i))
                    .SetValue(Get<int>(QUALITY_SETTINGS)));


            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).ApplyTemplate(VerticalLayout).SetText("Camera Settings");
            var zoomSensitivity = MainContent.CreateSlider(label: "Zoom Sensitivity", minValue: 1f, maxValue: 15f, currentValue: Get<float>(CameraController.ZOOM_SENSITIVITY_PREF))
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent((s, v) => Set(CameraController.ZOOM_SENSITIVITY_PREF, v))
                .SetValue(Get<float>(CameraController.ZOOM_SENSITIVITY_PREF));
            var yawSensitivity = MainContent.CreateSlider(label: "Yaw Sensitivity", minValue: 1f, maxValue: 15f, currentValue: Get<float>(CameraController.YAW_SENSITIVITY_PREF))
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent((s, v) => Set(CameraController.YAW_SENSITIVITY_PREF, v))
                .SetValue(Get<float>(CameraController.YAW_SENSITIVITY_PREF));
            var pitchSensitivity = MainContent.CreateSlider(label: "Pitch Sensitivity", minValue: 1f, maxValue: 15f, currentValue: Get<float>(CameraController.PITCH_SENSITIVITY_PREF))
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent((s, v) => Set(CameraController.PITCH_SENSITIVITY_PREF, v))
                .SetValue(Get<float>(CameraController.PITCH_SENSITIVITY_PREF));

            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).ApplyTemplate(VerticalLayout).SetText("Preferences");
            var reportAnalyticsToggle = MainContent.CreateToggle().ApplyTemplate(VerticalLayout).AddOnStateChangedEvent(
                (t, s) => {
                Set(ALLOW_DATA_GATHERING, s);
                }
            ).SetState(Get<bool>(ALLOW_DATA_GATHERING)).TitleLabel.SetText("Report Analytics");
            var measurementsToggle = MainContent.CreateToggle().ApplyTemplate(VerticalLayout).AddOnStateChangedEvent(
                (t, s) =>
                {
                    Set(MEASUREMENTS, s);
                }
            ).SetState(Get<bool>(MEASUREMENTS)).TitleLabel.SetText("Use Metric");
            

        }

        public override void Update() { }

        public override void Delete() { }


        [SerializeField]
        public GameObject list;
        [SerializeField]
        public GameObject keybindInputPrefab;
        [SerializeField]
        public GameObject dropdownInputPrefab;
        [SerializeField]
        public GameObject toggleInputPrefab;
        [SerializeField]
        public GameObject titleTextPrefab;
        [SerializeField]
        public GameObject sliderInputPrefab;


        public const string SCREEN_MODE = "Screen Mode";//Dropdown: Fullscreen or Windowed
        public static readonly string[] ScreenModeList = new string[] { "Fullscreen", "Windowed" };
        public const string QUALITY_SETTINGS = "Quality Settings";//Dropdown: Low Medium High
        public static string[] QualitySettingsList;
        public const string ALLOW_DATA_GATHERING = "Allow Data Gathering";//Toggle
        public const string MEASUREMENTS = "Use Imperial Measurements";//toggle for imperial. if unchecked, uses metric. 
        public static bool useImperial = true;//for other scripts to know when to use imperial or metric

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(int hWnd, int nCmdShow);

        public void SaveSettings()
        {//writes settings back into preference manager when save button is clicked
            Save();
            LoadSettings();
            RepopulatePanel();

            var update = new AnalyticsEvent(category: "Settings", action: "Saved", label: $"Saved Settings");
            AnalyticsManager.LogEvent(update);
            AnalyticsManager.PostData();
        }

        public void ResetSettings()
        {
            SetDefaultPreferences();
            LoadSettings();
            RepopulatePanel();

            var update = new AnalyticsEvent(category: "Settings", action: "Reset", label: $"Reset Settings");
            AnalyticsManager.LogEvent(update);
            AnalyticsManager.PostData();
        }
        private void RepopulatePanel()
        {
            DynamicUIManager.CreateModal<SettingsModal>();
        }
        public static void LoadSettings()
        {
            QualitySettingsList = QualitySettings.names;

            PreferenceManager.PreferenceManager.Load();
            //checks if preferences are initialized with default values
            if (!PreferenceManager.PreferenceManager.ContainsPreference(SCREEN_MODE))
            {
                SetDefaultPreferences();
            }

            //set screen mode
            switch (Get<int>(SCREEN_MODE))
            {
                case 0://Full Screen
                    if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
                    {
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                        MaximizeScreen();
                        
                    }
                    break;
                case 1:
                    if (Screen.fullScreenMode != FullScreenMode.Windowed)
                    {
                        Screen.fullScreenMode = FullScreenMode.Windowed;

                    }
                    break;

            }

            //Quality Settings
            QualitySettings.SetQualityLevel(Get<int>(QUALITY_SETTINGS), true);

            //Analytics
            AnalyticsManager.UseAnalytics = Get<bool>(ALLOW_DATA_GATHERING);

            //imperial or metric
            useImperial = Get<bool>(MEASUREMENTS);

            //Camera
            CameraController.ZoomSensitivity = Get<float>(CameraController.ZOOM_SENSITIVITY_PREF) / 10;//scaled down by 10
            CameraController.PitchSensitivity = Get<float>(CameraController.PITCH_SENSITIVITY_PREF);
            CameraController.YawSensitivity = Get<float>(CameraController.YAW_SENSITIVITY_PREF);
        }
        public static void SetDefaultPreferences()
        {
            Set(SCREEN_MODE, (int)0);//fullscreen
            Set(QUALITY_SETTINGS, (int)3);//high quality
            Set(ALLOW_DATA_GATHERING, (bool)true);
            Set(MEASUREMENTS, (bool)true);
            Set(CameraController.ZOOM_SENSITIVITY_PREF, (int)5);
            Set(CameraController.YAW_SENSITIVITY_PREF, (int)10);
            Set(CameraController.PITCH_SENSITIVITY_PREF, (int)3);
            Save();
        }

        //Sets Preference for better readability
        private static void Set(string s, object o) {
            PreferenceManager.PreferenceManager.SetPreference(s, o);
        }
        private static T Get<T>(string s)
            => PreferenceManager.PreferenceManager.GetPreference<T>(s);

        private static void Save()
        {
            PreferenceManager.PreferenceManager.Save();
        }
        public static void MaximizeScreen()
        {
            //auto maximizes if its a window and the resolution is maximum. 
            if (!Screen.fullScreen && !Application.isEditor)
                ShowWindowAsync(GetActiveWindow().ToInt32(), 3);
        }

    }
}
