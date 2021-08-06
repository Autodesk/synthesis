using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Synthesis.PreferenceManager;
using Synthesis.UI.Panels;

public static class Preference
{
    public const string RESOLUTION = "Resolution";//Dropdown: Different resolution settings
    public static string[] ResolutionList;
    public static string MAX_RES = "Maximum Resolution";//saved bool for if the player selected for the maximum resolution
    public const string SCREEN_MODE = "Screen Mode";//Dropdown: Fullscreen or Windowed
    public static readonly string[] ScreenModeList = new string[] { "Fullscreen", "Maximized Window", "Windowed" }; //OLD SYNTHESIS SUCKS AT THIS   
    public const string QUALITY_SETTINGS = "Quality Settings";//Dropdown: Low Medium High
    public static readonly string[] QualitySettingsList = QualitySettings.names;
    public const string ALLOW_DATA_GATHERING = "Allow Data Gathering";//Toggle
    public const string MEASUREMENTS = "Use Imperial Measurements";//toggle for imperial. if unchecked, uses metric. 
    public static bool useImperial = true;//for other scripts to know when to use imperial or metric
    public const string ZOOM_SENSITIVITY = "Zoom Sensitivity";//camera settings
    public const string YAW_SENSITIVITY = "Yaw Sensitivity";
    public const string PITCH_SENSITIVITY = "Pitch Sensitivity";

    public static void LoadSettings()
    {
        //populate resolution list with availible resolutions     
        ResolutionList = new string[Screen.resolutions.Length];
        for (int i = 0; i < ResolutionList.Length; i++)
        {
            ResolutionList[i] = Screen.resolutions[i].width + "x" + Screen.resolutions[i].height;
        }

        PreferenceManager.Load();
        if (Get(Preference.RESOLUTION) == null)
        {//checks if preferences are initialized with default values
            setDefaultPreferences();
        }

        //set screen mode
        FullScreenMode fsMode = FullScreenMode.FullScreenWindow;
        switch (GetInt(Preference.SCREEN_MODE))
        {
            case 0://Full Screen
                ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                Set(Preference.MAX_RES, true);
                Save();
                break;
            case 1:
                ResolutionList = new string[] { ResolutionList[ResolutionList.Length - 1] };
                fsMode = FullScreenMode.MaximizedWindow;
                Set(Preference.MAX_RES, true);
                Save();
                break;
            case 2:
                fsMode = FullScreenMode.Windowed;
                break;

        }

        if (GetBool(Preference.MAX_RES))
        {
            Set(Preference.RESOLUTION, ResolutionList.Length - 1);
            Save();
            SetRes(Screen.resolutions.Length - 1, fsMode);
        }
        else
        {
            int resolutionIndex = GetInt(Preference.RESOLUTION);

            if (resolutionIndex < ResolutionList.Length)
            {
                SetRes(resolutionIndex, fsMode);
            }
            else
            {
                Set(Preference.RESOLUTION, ResolutionList.Length - 1);
                Set(Preference.MAX_RES, true);
                Save();
                SetRes(ResolutionList.Length - 1, fsMode);
            }
        }

        //Quality Settings
        QualitySettings.SetQualityLevel(GetInt(Preference.QUALITY_SETTINGS), true);

        //Analytics
        AnalyticsManager.useAnalytics = GetBool(Preference.ALLOW_DATA_GATHERING);

        //imperial or metric
        useImperial = GetBool(Preference.MEASUREMENTS);

        //Camera
        CameraController c = Camera.main.GetComponent<CameraController>();
        c.ZoomSensitivity = GetFloat(Preference.ZOOM_SENSITIVITY) / 10;//scaled down by 10
        c.PitchSensitivity = GetInt(Preference.PITCH_SENSITIVITY);
        c.YawSensitivity = GetInt(Preference.YAW_SENSITIVITY);
    }
    public static void setDefaultPreferences()
    {
        Set(Preference.RESOLUTION, (int)0);
        Set(Preference.MAX_RES, (bool)true);
        Set(Preference.SCREEN_MODE, (int)0);
        Set(Preference.QUALITY_SETTINGS, (int)3);//high quality
        Set(Preference.ALLOW_DATA_GATHERING, (bool)true);
        Set(Preference.MEASUREMENTS, (bool)true);
        Set(Preference.ZOOM_SENSITIVITY, (int)5);
        Set(Preference.YAW_SENSITIVITY, (int)10);
        Set(Preference.PITCH_SENSITIVITY, (int)3);
        Save();
    }
    
    //Sets Preference for better readability
    private static void Set(string s, object o)
    {
        PreferenceManager.SetPreference(s, o);
    }
    private static object Get(string s)
    {
        return PreferenceManager.GetPreference(s);
    }
    //Get Int
    private static int GetInt(string s)
    {
        return Convert.ToInt32(PreferenceManager.GetPreference(s));
    }
    //Get Float
    private static float GetFloat(string s)
    {
        return Convert.ToSingle(PreferenceManager.GetPreference(s));
    }
    //Get Bool
    private static bool GetBool(string s)
    {
        return (bool)(PreferenceManager.GetPreference(s));
    }
    private static void Save()
    {
        PreferenceManager.Save();
    }
    //screen resolution set
    private static void SetRes(int i, FullScreenMode f)
    {
        Screen.SetResolution(
            Screen.resolutions[i].width,
            Screen.resolutions[i].height, f);
    }
}

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

    public enum InputType {
        Toggle,
        Dropdown,
        Keybind,
        Slider
    }

    void Start() {
        DisplaySettings();
    }

    private void DisplaySettings() {
        CreateTitle("Screen Settings");
        CreateDropdown(Preference.RESOLUTION, Preference.ResolutionList, GetInt(Preference.RESOLUTION));
        CreateDropdown(Preference.SCREEN_MODE, Preference.ScreenModeList, GetInt(Preference.SCREEN_MODE));
        CreateDropdown(Preference.QUALITY_SETTINGS, Preference.QualitySettingsList, GetInt(Preference.QUALITY_SETTINGS));

        CreateTitle("Camera Settings");
        CreateSlider(Preference.ZOOM_SENSITIVITY, 1, 15, GetInt(Preference.ZOOM_SENSITIVITY));
        CreateSlider(Preference.YAW_SENSITIVITY, 1, 15, GetInt(Preference.YAW_SENSITIVITY));
        CreateSlider(Preference.PITCH_SENSITIVITY, 1, 15, GetInt(Preference.PITCH_SENSITIVITY));

        CreateTitle("Preferences");
        CreateToggle(Preference.ALLOW_DATA_GATHERING, GetBool(Preference.ALLOW_DATA_GATHERING));
        CreateToggle(Preference.MEASUREMENTS, GetBool(Preference.MEASUREMENTS));
    }

    public void SaveSettings() {//writes settings back into preference manager when save button is clicked
        foreach (GameObject g in _settingsList)
        {
            SettingsInput si = g.GetComponent<SettingsInput>();
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
            if (name == Preference.RESOLUTION)
            {
                if (GetInt(Preference.RESOLUTION) == Preference.ResolutionList.Length - 1)
                    Set(Preference.MAX_RES, true);
                else
                    Set(Preference.MAX_RES, false);
            }
        }
        Save();
        Load();
    }

    public override void Close() {
        Save();
        base.Close();
    }

    public void ResetSettings() {
        Preference.setDefaultPreferences();
        Load();
    }

    private void CreateDropdown(string title, string[] dropdownList, int value) {
        GameObject g = Instantiate(dropdownInputPrefab, list.transform);
        g.GetComponent<SettingsInput>().Init(title, dropdownList, value);
        _settingsList.Add(g);
    }

    private void CreateToggle(string title, bool state) {
        GameObject g = Instantiate(toggleInputPrefab, list.transform);
        g.GetComponent<SettingsInput>().Init(title, state);
        _settingsList.Add(g);
    }

    private void CreateKeybind(string control, string key) {
        GameObject g = Instantiate(keybindInputPrefab, list.transform);
        g.GetComponent<SettingsInput>().Init(control, key);
        _settingsList.Add(g);
    }
    
    private void CreateTitle(string title) {
        GameObject g = Instantiate(titleTextPrefab, list.transform);
        g.GetComponentInChildren<TextMeshProUGUI>().text = title;
    }
    
    private void CreateSlider(string title, int lowVal, int highVal, int value) {
        GameObject g = Instantiate(sliderInputPrefab, list.transform);
        g.GetComponent<SettingsInput>().Init(title, lowVal, highVal, value);
        _settingsList.Add(g);
    }

    //Sets Preference for better readability
    private void Set(string s, object o) {
        PreferenceManager.SetPreference(s, o);
    }
    //Get Int
    private int GetInt(string s)
        => Convert.ToInt32(PreferenceManager.GetPreference(s));
    private void Load() {
        Preference.LoadSettings();
    }
    //Get Float
    private float GetFloat(string s)
        => Convert.ToSingle(PreferenceManager.GetPreference(s));
    //Get Bool
    private bool GetBool(string s)
        => (bool)PreferenceManager.GetPreference(s);
    private void Save() {
        PreferenceManager.Save();
    }
}

