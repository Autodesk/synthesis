using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Analytics;
using UI.Dynamic.Modals.Configuring.ThemeEditor;

namespace Synthesis.UI.Dynamic {
    public class SettingsModal : ModalDynamic {
        public const string SCREEN_MODE      = "Screen Mode";      // Dropdown: Fullscreen or Windowed
        public const string QUALITY_SETTINGS = "Quality Settings"; // Dropdown: Low Medium High
        public const string MEASUREMENTS =
            "Use Imperial Measurements"; // toggle for imperial. if unchecked, uses metric.
        public const string RENDER_SCORE_ZONES = "Render Score Zones";

        private static string[] _screenModeList      = { "Fullscreen", "Windowed" };
        private static string[] _qualitySettingsList = { "Low", "Medium", "High", "Ultra" };

        private const float MODAL_WIDTH  = 400;
        private const float MODAL_HEIGHT = 562;

        private static int _screenModeIndex;
        private static int _qualitySettingsIndex;
        private static float _zoomSensitivity;
        private static float _yawSensitivity;
        private static float _pitchSensitivity;
        private static bool _useAnalytics;
        private static bool _useMetric;
        private static bool _renderScoreZones;

        public SettingsModal() : base(new Vector2(MODAL_WIDTH, MODAL_HEIGHT)) {}

        public override void Create() {
            Title.SetText("Settings");
            ModalIcon.SetSprite(SynthesisAssetCollection.GetSpriteByName("settings"));

            AcceptButton.StepIntoLabel(b => { b.SetText("Save"); }).AddOnClickedEvent(b => {
                SaveSettings();
                ApplySettings();
                DynamicUIManager.CloseActiveModal();
            });
            CancelButton.StepIntoLabel(b => { b.SetText("Close"); })
                .AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal());

            LoadSettings();

            MainContent.CreateLabel()
                .ApplyTemplate(Label.BigLabelTemplate)
                .ApplyTemplate(UIComponent.VerticalLayoutNoSpacing)
                .SetText("Screen Settings");

            var screenModeDropdown =
                MainContent.CreateLabeledDropdown()
                    .ApplyTemplate(UIComponent.VerticalLayout)
                    .StepIntoLabel(l => l.SetText("Screen Mode"))
                    .StepIntoDropdown(d => d.SetOptions(_screenModeList)
                                               .AddOnValueChangedEvent((d, i, o) => { _screenModeIndex = i; })
                                               .SetValue(Get<int>(SCREEN_MODE)));

            var qualitySettingsDropdown =
                MainContent.CreateLabeledDropdown()
                    .ApplyTemplate(UIComponent.VerticalLayout)
                    .StepIntoLabel(l => l.SetText("Quality Settings"))
                    .StepIntoDropdown(d => d.SetOptions(_qualitySettingsList)
                                               .AddOnValueChangedEvent((d, i, o) => _qualitySettingsIndex = i)
                                               .SetValue(Get<int>(QUALITY_SETTINGS)));

            var editThemeButton =
                MainContent.CreateButton("Theme Editor").ApplyTemplate<Button>(UIComponent.VerticalLayout).AddOnClickedEvent(b => {
                    SaveSettings();
                    ApplySettings();
                    DynamicUIManager.CreateModal<EditThemeModal>();
                });

            MainContent.CreateLabel()
                .ApplyTemplate(Label.BigLabelTemplate)
                .ApplyTemplate(UIComponent.VerticalLayoutBigSpacing)
                .SetText("Camera Settings");
            var zoomSensitivity = MainContent
                                      .CreateSlider(label: "Zoom Sensitivity", minValue: 1f, maxValue: 15f,
                                          currentValue: Get<float>(CameraController.ZOOM_SENSITIVITY_PREF))
                                      .ApplyTemplate(UIComponent.VerticalLayout)
                                      .AddOnValueChangedEvent((s, v) => _zoomSensitivity = v)
                                      .SetValue(Get<float>(CameraController.ZOOM_SENSITIVITY_PREF));
            var yawSensitivity = MainContent
                                     .CreateSlider(label: "Yaw Sensitivity", minValue: 1f, maxValue: 15f,
                                         currentValue: Get<float>(CameraController.YAW_SENSITIVITY_PREF))
                                     .ApplyTemplate(UIComponent.VerticalLayout)
                                     .AddOnValueChangedEvent((s, v) => _yawSensitivity = v)
                                     .SetValue(Get<float>(CameraController.YAW_SENSITIVITY_PREF));
            var pitchSensitivity = MainContent
                                       .CreateSlider(label: "Pitch Sensitivity", minValue: 1f, maxValue: 15f,
                                           currentValue: Get<float>(CameraController.PITCH_SENSITIVITY_PREF))
                                       .ApplyTemplate(UIComponent.VerticalLayout)
                                       .AddOnValueChangedEvent((s, v) => _pitchSensitivity = v)
                                       .SetValue(Get<float>(CameraController.PITCH_SENSITIVITY_PREF));

            MainContent.CreateLabel()
                .ApplyTemplate(Label.BigLabelTemplate)
                .ApplyTemplate(UIComponent.VerticalLayoutBigSpacing)
                .SetText("Preferences");
            var reportAnalyticsToggle = MainContent.CreateToggle()
                                            .ApplyTemplate(UIComponent.VerticalLayout)
                                            .AddOnStateChangedEvent((t, s) => _useAnalytics = s)
                                            .SetState(Get<bool>(AnalyticsManager.USE_ANALYTICS_PREF))
                                            .TitleLabel.SetText("Report Analytics");
            var measurementsToggle = MainContent.CreateToggle()
                                         .ApplyTemplate(UIComponent.VerticalLayout)
                                         .AddOnStateChangedEvent((t, s) => _useMetric = s)
                                         .SetState(Get<bool>(MEASUREMENTS))
                                         .TitleLabel.SetText("Use Metric");
            var renderScoreModesToggle = MainContent.CreateToggle()
                                             .ApplyTemplate(UIComponent.VerticalLayout)
                                             .AddOnStateChangedEvent((t, s) => _renderScoreZones = s)
                                             .SetState(Get<bool>(RENDER_SCORE_ZONES))
                                             .TitleLabel.SetText("Render Score Zones");
        }

        public override void Update() {}

        public override void Delete() {}

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(int hWnd, int nCmdShow);

        public static void SaveSettings() {
            Set(SCREEN_MODE, _screenModeIndex);
            Set(QUALITY_SETTINGS, _qualitySettingsIndex);
            Set(CameraController.ZOOM_SENSITIVITY_PREF, _zoomSensitivity);
            Set(CameraController.YAW_SENSITIVITY_PREF, _yawSensitivity);
            Set(CameraController.PITCH_SENSITIVITY_PREF, _pitchSensitivity);
            Set(AnalyticsManager.USE_ANALYTICS_PREF, _useAnalytics);
            Set(MEASUREMENTS, _useMetric);
            Set(RENDER_SCORE_ZONES, _renderScoreZones);

            Save();

            AnalyticsManager.LogCustomEvent(AnalyticsEvent.SettingsSaved);
        }

        public static void LoadSettings() {
            _screenModeIndex      = Get<int>(SCREEN_MODE);
            _qualitySettingsIndex = Get<int>(QUALITY_SETTINGS);
            _zoomSensitivity      = Get<float>(CameraController.ZOOM_SENSITIVITY_PREF);
            _yawSensitivity       = Get<float>(CameraController.YAW_SENSITIVITY_PREF);
            _pitchSensitivity     = Get<float>(CameraController.PITCH_SENSITIVITY_PREF);
            _useAnalytics         = Get<bool>(AnalyticsManager.USE_ANALYTICS_PREF);
            _useMetric            = Get<bool>(MEASUREMENTS);
        }

        public void ResetSettings() {
            SetDefaultPreferences();
            SaveSettings();
            ApplySettings();
            RepopulatePanel();

            AnalyticsManager.LogCustomEvent(AnalyticsEvent.SettingsReset);
        }

        private void RepopulatePanel() {
            DynamicUIManager.CreateModal<SettingsModal>();
        }

        public static void ApplySettings() {
            _qualitySettingsList = QualitySettings.names;

            PreferenceManager.PreferenceManager.Load();
            // checks if preferences are initialized with default values
            if (!PreferenceManager.PreferenceManager.ContainsPreference(SCREEN_MODE)) {
                SetDefaultPreferences();
            }

            // set screen mode
            switch (Get<int>(SCREEN_MODE)) {
                case 0: // Full Screen
                    if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow) {
                        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                        MaximizeScreen();
                    }
                    break;
                case 1:
                    if (Screen.fullScreenMode != FullScreenMode.Windowed)
                        Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
            }

            // Quality Settings
            QualitySettings.SetQualityLevel(Get<int>(QUALITY_SETTINGS), true);

            // Camera
            CameraController.ZoomSensitivity =
                Get<float>(CameraController.ZOOM_SENSITIVITY_PREF) / 10; // scaled down by 10
            CameraController.PitchSensitivity = Get<float>(CameraController.PITCH_SENSITIVITY_PREF);
            CameraController.YawSensitivity   = Get<float>(CameraController.YAW_SENSITIVITY_PREF);
        }

        public static void SetDefaultPreferences() {
            _screenModeIndex      = 0;
            _qualitySettingsIndex = 3;
            _zoomSensitivity      = 5f;
            _yawSensitivity       = 10f;
            _pitchSensitivity     = 3f;
            _useAnalytics         = true;
            _useMetric            = true;
            SaveSettings();
        }

        private static void Set(string s, object o) => PreferenceManager.PreferenceManager.SetPreference(s, o);

        private static T Get<T>(string s) => PreferenceManager.PreferenceManager.GetPreference<T>(s);

        private static void Save() => PreferenceManager.PreferenceManager.Save();

        public static void MaximizeScreen() {
            // auto maximizes if its a window and the resolution is maximum.
            if (!Screen.fullScreen && !Application.isEditor)
                ShowWindowAsync(GetActiveWindow().ToInt32(), 3);
        }
    }
}