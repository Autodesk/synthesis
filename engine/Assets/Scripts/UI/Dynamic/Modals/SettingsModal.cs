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
            //SetDefaultPreferences();

            // (var left, var right) = base.MainContent.SplitLeftRight(250, 100);
            Title.SetText("Settings");
            Description.SetText("Select one of the settings in order to change simulation settings");

            AcceptButton.AddOnClickedEvent(b => {
                SaveSettings();
                DynamicUIManager.CloseActiveModal();
            });
            CancelButton.StepIntoLabel(b => { b.SetText("Close"); }).AddOnClickedEvent(b => DynamicUIManager.CloseActiveModal());
            
            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).ApplyTemplate(VerticalLayout).SetText("Screen Settings");

            /*if (customRes)
            {
                ResolutionList = ResolutionList.Concat(new string[] { "Custom" }).ToArray();
            }*/
            var resolutionDropdown = MainContent.CreateLabeledDropdown().ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Resolution"))
                .StepIntoDropdown(
                    d => d.SetOptions(ResolutionList)
                    .AddOnValueChangedEvent((d, i, o) => {
                        Set(RESOLUTION, i);

                        if (Get<int>(RESOLUTION) == ResolutionList.Length - 1)
                            Set(MAX_RES, true);
                        else
                            Set(MAX_RES, false);
                    })
                    .SetValue(Get<int>(RESOLUTION)));

            var screenModeDropdown = MainContent.CreateLabeledDropdown().ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Screen Mode"))
                .StepIntoDropdown(
                    d => d.SetOptions(ScreenModeList)
                    .AddOnValueChangedEvent((d, i, o) => { 
                        Set(SCREEN_MODE, i);
                        if (Get<int>(SCREEN_MODE) == 1)
                        {  //2 is windowed mode                                    
                            PopulateResolutionList();
                            SetMaxResolution();
                        }
                        else
                        {
                            ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                            SetMaxResolution();
                        }
                    })
                    .SetValue(Get<int>(SCREEN_MODE)));

            var qualitySettingsDropdown = MainContent.CreateLabeledDropdown().ApplyTemplate(VerticalLayout)
                .StepIntoLabel(l => l.SetText("Quality Settings"))
                .StepIntoDropdown(
                    d => d.SetOptions(QualitySettingsList)
                    .AddOnValueChangedEvent((d, i, o) => Set(QUALITY_SETTINGS,i))
                    .SetValue(Get<int>(QUALITY_SETTINGS)));


            MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).ApplyTemplate(VerticalLayout).SetText("Camera Settings");
            var zoomSensitivity = MainContent.CreateSlider(label: "Zoom Sensitivity", minValue: 1f, maxValue: 15f, currentValue: Get<float>(ZOOM_SENSITIVITY))
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent((s, v) => Set(ZOOM_SENSITIVITY, v))
                .SetValue(Get<float>(ZOOM_SENSITIVITY));
            var yawSensitivity = MainContent.CreateSlider(label: "Yaw Sensitivity", minValue: 1f, maxValue: 15f, currentValue: Get<float>(YAW_SENSITIVITY))
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent((s, v) => Set(YAW_SENSITIVITY, v))
                .SetValue(Get<float>(YAW_SENSITIVITY));
            var pitchSensitivity = MainContent.CreateSlider(label: "Pitch Sensitivity", minValue: 1f, maxValue: 15f, currentValue: Get<float>(PITCH_SENSITIVITY))
                .ApplyTemplate(VerticalLayout).AddOnValueChangedEvent((s, v) => Set(PITCH_SENSITIVITY, v))
                .SetValue(Get<float>(PITCH_SENSITIVITY));

            /*MainContent.CreateLabel().ApplyTemplate(Label.BigLabelTemplate).ApplyTemplate(VerticalLayout).SetText("Preferences");
            var reportAnalyticsToggle = MainContent.CreateToggle().ApplyTemplate(VerticalLayout).ApplyTemplate(Toggle.VerticalLayoutTemplate);
            reportAnalyticsToggle.AddOnStateChangedEvent(
                (t, s) => {
                Set(ALLOW_DATA_GATHERING, s);
                }
            );*/

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


        private List<GameObject> _settingsList = new List<GameObject>();

        private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private Color enabledColor = new Color(0.1294118f, 0.5882353f, 0.9529412f, 1f);
        public GameObject saveButton;

        public const string RESOLUTION = "Resolution";//Dropdown: Different resolution settings
        public static string[] ResolutionList;
        public static string MAX_RES = "Maximum Resolution";//saved bool for if the player selected for the maximum resolution
        public const string SCREEN_MODE = "Screen Mode";//Dropdown: Fullscreen or Windowed
        public static readonly string[] ScreenModeList = new string[] { "Fullscreen", "Windowed" };
        public const string QUALITY_SETTINGS = "Quality Settings";//Dropdown: Low Medium High
        public static string[] QualitySettingsList;
        public const string ALLOW_DATA_GATHERING = "Allow Data Gathering";//Toggle
        public const string MEASUREMENTS = "Use Imperial Measurements";//toggle for imperial. if unchecked, uses metric. 
        public static bool useImperial = true;//for other scripts to know when to use imperial or metric
        public const string ZOOM_SENSITIVITY = "Zoom Sensitivity";//camera settings
        public const string YAW_SENSITIVITY = "Yaw Sensitivity";
        public const string PITCH_SENSITIVITY = "Pitch Sensitivity";

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(int hWnd, int nCmdShow);

        private static bool customRes = false;

        public enum InputType
        {
            Toggle,
            Dropdown,
            Keybind,
            Slider
        }

        bool setup = false;

        public void SaveSettings()
        {//writes settings back into preference manager when save button is clicked
            Save();
            LoadSettings();
            RepopulatePanel();

            var update = new AnalyticsEvent(category: "Settings", action: "Saved", label: $"Saved Settings");
            AnalyticsManager.LogEvent(update);
            AnalyticsManager.PostData();
        }
        /*
        public override void Close()
        {
            Save();

            var update = new AnalyticsEvent(category: "Settings", action: "Closed", label: $"Closed Settings");
            AnalyticsManager.LogEvent(update);
            AnalyticsManager.PostData();
        }*/

        public void ResetSettings()
        {
            SetDefaultPreferences();
            LoadSettings();
            RepopulatePanel();

            var update = new AnalyticsEvent(category: "Settings", action: "Reset", label: $"Reset Settings");
            AnalyticsManager.LogEvent(update);
            AnalyticsManager.PostData();
        }
        private void SetMaxResolution()
        {
            Set(MAX_RES, true);
            Set(RESOLUTION, ResolutionList.Length - 1);
            //_settingsList[0].GetComponent<SettingsInput>().Init(RESOLUTION, ResolutionList, Get<int>(RESOLUTION));
        }
        private void RepopulatePanel()
        {
            DynamicUIManager.CreateModal<SettingsModal>();
        }

        public static void LoadSettings()
        {
            PopulateResolutionList();


            QualitySettingsList = QualitySettings.names;

            PreferenceManager.PreferenceManager.Load();
            //checks if preferences are initialized with default values
            if (!PreferenceManager.PreferenceManager.ContainsPreference(RESOLUTION))
            {
                SetDefaultPreferences();
            }

            //set screen mode
            FullScreenMode fsMode = FullScreenMode.FullScreenWindow;
            switch (Get<int>(SCREEN_MODE))
            {
                case 0://Full Screen
                    ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                    Set(MAX_RES, true);
                    Save();
                    break;
                case 1:
                    fsMode = FullScreenMode.Windowed;
                    break;

            }

                if (Get<bool>(MAX_RES))
                {
                    Set(RESOLUTION, ResolutionList.Length - 1);
                    Save();
                    SetRes(ResolutionList.Length - 1, fsMode);
                }
                else
                {//if user doesn't want maximum resolution, use index
                    int resolutionIndex = Get<int>(RESOLUTION);

                    if (resolutionIndex < ResolutionList.Length)
                    {//check if wanted resolution is above the availible resolutions (in case screen resolution changed)
                        SetRes(resolutionIndex, fsMode);
                    }
                    else
                    {
                        Set(RESOLUTION, ResolutionList.Length - 1);
                        Set(MAX_RES, true);
                        Save();
                        SetRes(ResolutionList.Length - 1, fsMode);
                    }
                }
            
            //Quality Settings
            QualitySettings.SetQualityLevel(Get<int>(QUALITY_SETTINGS), true);

            //Analytics
            AnalyticsManager.useAnalytics = Get<bool>(ALLOW_DATA_GATHERING);

            //imperial or metric
            useImperial = Get<bool>(MEASUREMENTS);

            //Camera
            CameraController.ZoomSensitivity = Get<float>(ZOOM_SENSITIVITY) / 10;//scaled down by 10
            CameraController.PitchSensitivity = Get<int>(PITCH_SENSITIVITY);
            CameraController.YawSensitivity = Get<int>(YAW_SENSITIVITY);

            MaximizeScreen();
        }
        public static void SetDefaultPreferences()
        {
            Set(RESOLUTION, (int)0);
            Set(MAX_RES, (bool)true);
            Set(SCREEN_MODE, (int)0);//fullscreen
            Set(QUALITY_SETTINGS, (int)3);//high quality
            Set(ALLOW_DATA_GATHERING, (bool)true);
            Set(MEASUREMENTS, (bool)true);
            Set(ZOOM_SENSITIVITY, (int)5);
            Set(YAW_SENSITIVITY, (int)10);
            Set(PITCH_SENSITIVITY, (int)3);
            Save();
        }

        //populate resolution list with availible resolutions  
        public static void PopulateResolutionList()
        {
            List<string> resolutions = new List<string>();
            foreach (Resolution r in Screen.resolutions)
            {
                if (!resolutions.Contains(r.width + "x" + r.height))
                    resolutions.Add(r.width + "x" + r.height);
            }
            ResolutionList = resolutions.ToArray();
        }

        private static bool CustomScreen()
        {
            bool customScreen = true;
            foreach (Resolution r in Screen.resolutions)
            {
                if (Screen.width == r.width && Screen.height == r.height)
                {
                    customScreen = false;
                    break;
                }
            }
            return customScreen;
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
            if (Get<bool>(MAX_RES) && !Screen.fullScreen && !Application.isEditor)
                ShowWindowAsync(GetActiveWindow().ToInt32(), 3);
        }

        //screen resolution set
        private static void SetRes(int i, FullScreenMode f)
        {
            if (ResolutionList[i] == "Custom")
                return;
            string[] r = ResolutionList[i].Split('x');
            Screen.SetResolution(
            Convert.ToInt32(r[0]),
            Convert.ToInt32(r[1]), f);
        }
        
    }
}
