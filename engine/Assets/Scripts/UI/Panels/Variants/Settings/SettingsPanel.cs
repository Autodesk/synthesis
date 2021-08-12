using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Synthesis.UI.Panels.Variant
{
    public class SettingsPanel : Panel
    {

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

        private Color disabledColor = new Color(0.9333333f, 0.9333333f, 0.9333333f, 1f);
        private Color enabledColor = new Color(0.1294118f, 0.5882353f, 0.9529412f, 1f);
        public GameObject saveButton;
        public TextMeshProUGUI saveText;

        public const string RESOLUTION = "Resolution";//Dropdown: Different resolution settings
        public static string[] ResolutionList;
        public static string MAX_RES = "Maximum Resolution";//saved bool for if the player selected for the maximum resolution
        public const string SCREEN_MODE = "Screen Mode";//Dropdown: Fullscreen or Windowed
        public static readonly string[] ScreenModeList = new string[] { "Fullscreen", "Maximized Window", "Windowed" }; //OLD SYNTHESIS SUCKS AT THIS   
        public const string QUALITY_SETTINGS = "Quality Settings";//Dropdown: Low Medium High
        public static string[] QualitySettingsList;
        public const string ALLOW_DATA_GATHERING = "Allow Data Gathering";//Toggle
        public const string MEASUREMENTS = "Use Imperial Measurements";//toggle for imperial. if unchecked, uses metric. 
        public static bool useImperial = true;//for other scripts to know when to use imperial or metric
        public const string ZOOM_SENSITIVITY = "Zoom Sensitivity";//camera settings
        public const string YAW_SENSITIVITY = "Yaw Sensitivity";
        public const string PITCH_SENSITIVITY = "Pitch Sensitivity";

        public enum InputType
        {
            Toggle,
            Dropdown,
            Keybind,
            Slider
        }

        void Start()
        {
            DisplaySettings();
        }


        public void disableSaveButton()
        {
            saveButton.GetComponent<Image>().color = disabledColor;
            saveText.color = new Color(0.2352941f, 0.2352941f, 0.2352941f, 1f);
            saveButton.GetComponent<Button>().interactable = false;
        }
        public void enableSaveButton()
        {
            saveButton.GetComponent<Button>().interactable = true;
            saveText.color = Color.white;
            saveButton.GetComponent<Image>().color = enabledColor;
        }
        bool setup = false;
        private void DisplaySettings()
        {
            setup = true;
            CreateTitle("Screen Settings");
            CreateDropdown(RESOLUTION, ResolutionList, GetInt(RESOLUTION));
            CreateDropdown(SCREEN_MODE, ScreenModeList, GetInt(SCREEN_MODE));
            CreateDropdown(QUALITY_SETTINGS, QualitySettingsList, GetInt(QUALITY_SETTINGS));

            CreateTitle("Camera Settings");
            CreateSlider(ZOOM_SENSITIVITY, 1, 15, GetInt(ZOOM_SENSITIVITY));
            CreateSlider(YAW_SENSITIVITY, 1, 15, GetInt(YAW_SENSITIVITY));
            CreateSlider(PITCH_SENSITIVITY, 1, 15, GetInt(PITCH_SENSITIVITY));

            CreateTitle("Preferences");
            CreateToggle(ALLOW_DATA_GATHERING, GetBool(ALLOW_DATA_GATHERING));
            CreateToggle(MEASUREMENTS, GetBool(MEASUREMENTS));

            disableSaveButton();
            setup = false;
        }



        public void SaveSettings()
        {//writes settings back into preference manager when save button is clicked
            Save();
            LoadSettings();
            disableSaveButton();
        }
        public void onValueChanged(SettingsInput si)
        {
            if (!setup)
            { //prevents this from being called on setup
                string name = si.Title; //key for preference manager

                switch (si.Type)
                {
                    case InputType.Toggle:
                        Set(name, si.toggle.isOn);
                        break;
                    case InputType.Dropdown:
                        Set(name, si.dropdown.value);
                        break;
                    case InputType.Keybind:
                        Set(name, si.key.text);
                        break;
                    case InputType.Slider:
                        Set(name, si.slider.value);
                        break;
                }

                //for dynamic resolution setting: checks if player wants the maximum resolution
                if (name == RESOLUTION)
                {
                    if (GetInt(RESOLUTION) == ResolutionList.Length - 1)
                        Set(MAX_RES, true);
                    else
                        Set(MAX_RES, false);
                }
                else if (name == SCREEN_MODE)
                {//modifies availible resolutions when screen mode is changed
                    if (GetInt(SCREEN_MODE) == 2)
                    {  //2 is windowed mode                                    
                        PopulateResolutionList();
                        SetMaxResolution();
                    }
                    else
                    {
                        ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                        SetMaxResolution();
                    }
                }

                enableSaveButton();
            }
        }

        public override void Close()
        {
            Save();
            base.Close();
        }

        public void ResetSettings()
        {
            SetDefaultPreferences();
            LoadSettings();
            RepopulatePanel();
        }
        private void SetMaxResolution()
        {
            Set(MAX_RES, true);
            Set(RESOLUTION, ResolutionList.Length - 1);
            _settingsList[0].GetComponent<SettingsInput>().Init(RESOLUTION, ResolutionList, GetInt(RESOLUTION));
        }
        private void RepopulatePanel()
        {
            //clear
            _settingsList = new List<GameObject>();
            foreach (Transform s in list.GetComponentInChildren<Transform>())
            {
                Destroy(s.gameObject);
            }
            //reload
            DisplaySettings();
        }

        private void CreateDropdown(string title, string[] dropdownList, int value)
        {
            GameObject g = Instantiate(dropdownInputPrefab, list.transform);
            g.GetComponent<SettingsInput>().Init(title, dropdownList, value);
            _settingsList.Add(g);
        }

        private void CreateToggle(string title, bool state)
        {
            GameObject g = Instantiate(toggleInputPrefab, list.transform);
            g.GetComponent<SettingsInput>().Init(title, state);
            _settingsList.Add(g);
        }

        private void CreateKeybind(string control, string key)
        {
            GameObject g = Instantiate(keybindInputPrefab, list.transform);
            g.GetComponent<SettingsInput>().Init(control, key);
            _settingsList.Add(g);
        }

        private void CreateTitle(string title)
        {
            GameObject g = Instantiate(titleTextPrefab, list.transform);
            g.GetComponentInChildren<TextMeshProUGUI>().text = title;
        }

        private void CreateSlider(string title, int lowVal, int highVal, int value)
        {
            GameObject g = Instantiate(sliderInputPrefab, list.transform);
            g.GetComponent<SettingsInput>().Init(title, lowVal, highVal, value);
            _settingsList.Add(g);
        }


        public static void LoadSettings()
        {
            PopulateResolutionList();
            QualitySettingsList = QualitySettings.names;

            PreferenceManager.PreferenceManager.Load();
            if (Get(RESOLUTION) == null)
            {//checks if preferences are initialized with default values
                SetDefaultPreferences();
            }

            //set screen mode
            FullScreenMode fsMode = FullScreenMode.FullScreenWindow;
            switch (GetInt(SCREEN_MODE))
            {
                case 0://Full Screen
                    ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                    Set(MAX_RES, true);
                    Save();
                    break;
                case 1:
                    ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                    fsMode = FullScreenMode.MaximizedWindow;
                    Set(MAX_RES, true);
                    Save();
                    break;
                case 2:
                    fsMode = FullScreenMode.Windowed;
                    break;

            }

            //Checks if the user wants maximum resolution
            if (GetBool(MAX_RES))
            {
                Set(RESOLUTION, ResolutionList.Length - 1);
                Save();
                SetRes(ResolutionList.Length - 1, fsMode);
            }
            else
            {//if user doesn't want maximum resolution, use index
                int resolutionIndex = GetInt(RESOLUTION);

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
            QualitySettings.SetQualityLevel(GetInt(QUALITY_SETTINGS), true);

            //Analytics
            AnalyticsManager.useAnalytics = GetBool(ALLOW_DATA_GATHERING);

            //imperial or metric
            useImperial = GetBool(MEASUREMENTS);

            //Camera
            CameraController c = Camera.main.GetComponent<CameraController>();
            c.ZoomSensitivity = GetFloat(ZOOM_SENSITIVITY) / 10;//scaled down by 10
            c.PitchSensitivity = GetInt(PITCH_SENSITIVITY);
            c.YawSensitivity = GetInt(YAW_SENSITIVITY);
        }
        public static void SetDefaultPreferences()
        {
            Set(RESOLUTION, (int)0);
            Set(MAX_RES, (bool)true);
            Set(SCREEN_MODE, (int)0);
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
            foreach(Resolution r in Screen.resolutions){
                if(!resolutions.Contains(r.width+"x"+r.height))
                resolutions.Add(r.width+"x"+r.height);
            }
            ResolutionList = resolutions.ToArray();
        }

        //Sets Preference for better readability
        private static void Set(string s, object o)
        {
            PreferenceManager.PreferenceManager.SetPreference(s, o);
        }
        private static object Get(string s)
        {
            return PreferenceManager.PreferenceManager.GetPreference(s);
        }
        //Get Int
        private static int GetInt(string s)
        {
            return Convert.ToInt32(PreferenceManager.PreferenceManager.GetPreference(s));
        }
        //Get Float
        private static float GetFloat(string s)
        {
            return Convert.ToSingle(PreferenceManager.PreferenceManager.GetPreference(s));
        }
        //Get Bool
        private static bool GetBool(string s)
        {
            return (bool)(PreferenceManager.PreferenceManager.GetPreference(s));
        }
        private static void Save()
        {
            PreferenceManager.PreferenceManager.Save();
        }

        //screen resolution set
        private static void SetRes(int i, FullScreenMode f)
        {
            Debug.Log(ResolutionList.Length);
            Debug.Log(i);
            Debug.Log(ResolutionList[i]);
            string[] r = ResolutionList[i].Split('x');
            Screen.SetResolution(
                Convert.ToInt32(r[0]),
                Convert.ToInt32(r[1]), f);
        }
    }

}
