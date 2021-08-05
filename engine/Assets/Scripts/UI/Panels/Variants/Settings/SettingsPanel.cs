using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Synthesis.PreferenceManager;
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
    public static bool useImperial = true;
    public const string ZOOM_SENSITIVITY = "Zoom Sensitivity";
    public const string YAW_SENSITIVITY = "Yaw Sensitivity";
    public const string PITCH_SENSITIVITY = "Pitch Sensitivity";

    public static void LoadSettings()
    {

        //populate resolution list        
        ResolutionList = new string[Screen.resolutions.Length];
        for (int i = 0; i < ResolutionList.Length; i++)
        {
            ResolutionList[i] = Screen.resolutions[i].width + "x" + Screen.resolutions[i].height;
        }

        PreferenceManager.Load();
        if (PreferenceManager.GetPreference(Preference.RESOLUTION) == null)
        {//checks if preferences are initialized with default values
            setDefaultPreferences();
        }

        FullScreenMode fsMode = FullScreenMode.FullScreenWindow;
        //set screen mode
        switch (Convert.ToInt32(PreferenceManager.GetPreference(Preference.SCREEN_MODE)))
        {
            case 0://Full Screen
                ResolutionList = new string[]{ResolutionList[ResolutionList.Length-1]};
                Set(Preference.MAX_RES,true);
                Save();
                break;
            case 1:
                ResolutionList = new string[]{ResolutionList[ResolutionList.Length-1]};
                fsMode = FullScreenMode.MaximizedWindow;
                Set(Preference.MAX_RES,true);
                Save();
                break;
            case 2:
                fsMode = FullScreenMode.Windowed;
                break;
                
        }

        if(GetBool(Preference.MAX_RES)){            
                Set(Preference.RESOLUTION, ResolutionList.Length-1);
                PreferenceManager.Save();
                SetRes(Screen.resolutions.Length-1,fsMode);
        }
        else{            
                int resolutionIndex = GetInt(Preference.RESOLUTION);

                if(resolutionIndex<ResolutionList.Length){
                    SetRes(resolutionIndex,fsMode);
                }
                else{
                    Set(Preference.RESOLUTION, ResolutionList.Length-1);
                    Set(Preference.MAX_RES,true);
                    PreferenceManager.Save();
                    SetRes(ResolutionList.Length-1,fsMode);
                }
        }

        //Quality Settings
        QualitySettings.SetQualityLevel(Convert.ToInt32(PreferenceManager.GetPreference(Preference.QUALITY_SETTINGS)), true);

        //Analytics
        AnalyticsManager.useAnalytics = (bool)PreferenceManager.GetPreference(Preference.ALLOW_DATA_GATHERING);

        //imperial or metric
        useImperial = (bool)PreferenceManager.GetPreference(Preference.MEASUREMENTS); //can convert back to dropdown


        //Camera
        CameraController c = Camera.main.GetComponent<CameraController>();
        c.ZoomSensitivity = Convert.ToSingle(PreferenceManager.GetPreference(Preference.ZOOM_SENSITIVITY)) / 10;//scaled down by 10
        c.PitchSensitivity = Convert.ToInt32(PreferenceManager.GetPreference(Preference.PITCH_SENSITIVITY));
        c.YawSensitivity = Convert.ToInt32(PreferenceManager.GetPreference(Preference.YAW_SENSITIVITY));
    }
    public static void setDefaultPreferences()
    {
        Set(Preference.RESOLUTION, (int)0);
        Set(Preference.MAX_RES,(bool)true);
        Set(Preference.SCREEN_MODE, (int)0);
        Set(Preference.QUALITY_SETTINGS, (int)0);
        Set(Preference.ALLOW_DATA_GATHERING, (bool)true);
        Set(Preference.MEASUREMENTS, (bool)true);
        Set(Preference.ZOOM_SENSITIVITY, (int)5);
        Set(Preference.YAW_SENSITIVITY, (int)10);
        Set(Preference.PITCH_SENSITIVITY, (int)3);
        PreferenceManager.Save();
    }
    private static void Set(string s, object o)
    {//better readability
        PreferenceManager.SetPreference(s, o);
    }
    //Get Int
    private static int GetInt(string s){
        return Convert.ToInt32(PreferenceManager.GetPreference(s));
    }
    //Get Float
    private static float GetFloat(string s){
        return Convert.ToSingle(PreferenceManager.GetPreference(s));
    }
    //Get Bool
    private static bool GetBool(string s){
        return (bool)(PreferenceManager.GetPreference(s));
    }
    private static void Save(){
        PreferenceManager.Save();
    }
    //screen resolution set
    private static void SetRes(int i, FullScreenMode f){
            Screen.SetResolution(
                Screen.resolutions[i].width,
                Screen.resolutions[i].height,f);
    }
    

}

public class SettingsPanel : MonoBehaviour
{

    [SerializeField]
    public GameObject list;
    [SerializeField]
    public GameObject keybindInput;
    [SerializeField]
    public GameObject dropdownInput;
    [SerializeField]
    public GameObject toggleInput;
    [SerializeField]
    public GameObject titleText;
    [SerializeField]
    public GameObject sliderInput;

    List<GameObject> settingsList = new List<GameObject>();

    public enum InputType
    {
        TOGGLE,
        DROPDOWN,
        KEYBIND,
        SLIDER
    }


    void Start()
    {
        showSettings();
    }

    private void showSettings()
    {
        createTitle("Screen Settings");
        createDropdown(Preference.RESOLUTION, Preference.ResolutionList, Convert.ToInt32(PreferenceManager.GetPreference(Preference.RESOLUTION)));
        createDropdown(Preference.SCREEN_MODE, Preference.ScreenModeList, Convert.ToInt32(PreferenceManager.GetPreference(Preference.SCREEN_MODE)));
        createDropdown(Preference.QUALITY_SETTINGS, Preference.QualitySettingsList, Convert.ToInt32(PreferenceManager.GetPreference(Preference.QUALITY_SETTINGS)));

        createTitle("Camera Settings");
        createSlider(Preference.ZOOM_SENSITIVITY, 1, 15, Convert.ToInt32(PreferenceManager.GetPreference(Preference.ZOOM_SENSITIVITY)));
        createSlider(Preference.YAW_SENSITIVITY, 1, 15, Convert.ToInt32(PreferenceManager.GetPreference(Preference.YAW_SENSITIVITY)));
        createSlider(Preference.PITCH_SENSITIVITY, 1, 15, Convert.ToInt32(PreferenceManager.GetPreference(Preference.PITCH_SENSITIVITY)));

        createTitle("Preferences");
        createToggle(Preference.ALLOW_DATA_GATHERING, (bool)PreferenceManager.GetPreference(Preference.ALLOW_DATA_GATHERING));
        createToggle(Preference.MEASUREMENTS, (bool)PreferenceManager.GetPreference(Preference.MEASUREMENTS));
    }

    public void saveSettings()//writes settings back into preference manager when save button is clicked
    {
        foreach (GameObject g in settingsList)
        {
            //MODIFY THIS: Each Input field has different outputs
            SettingsInput si = g.GetComponent<SettingsInput>();
            string name = si._name.text; //key for preference manager
            
            switch (si.getType())//sets each value based on type. Needs to be changed when writing to preference manager
            {
                case InputType.TOGGLE:
                    PreferenceManager.SetPreference(name, si._toggle.isOn);
                    break;
                case InputType.DROPDOWN:
                    PreferenceManager.SetPreference(name, si._dropdown.value);
                    break;
                case InputType.KEYBIND:
                    PreferenceManager.SetPreference(name, si._key.text);
                    break;
                case InputType.SLIDER:
                    PreferenceManager.SetPreference(name, si._slider.value);
                    break;
            }
            
            if(name == Preference.RESOLUTION){
                if(Convert.ToInt32(PreferenceManager.GetPreference(Preference.RESOLUTION))==Preference.ResolutionList.Length-1)
                    PreferenceManager.SetPreference(Preference.MAX_RES,true);
                else
                    PreferenceManager.SetPreference(Preference.MAX_RES,false);
            }
        }
        PreferenceManager.Save();
        Preference.LoadSettings();
    }

    public void resetSettings()
    {
        Preference.setDefaultPreferences();
        Preference.LoadSettings();
    }

    private void createDropdown(string title, string[] dropdownList, int value)
    {
        GameObject g = Instantiate(dropdownInput, list.transform);
        g.GetComponent<SettingsInput>().Init(title, dropdownList, value);
        settingsList.Add(g);
    }

    private void createToggle(string title, bool state)
    {
        GameObject g = Instantiate(toggleInput, list.transform);
        g.GetComponent<SettingsInput>().Init(title, state);
        settingsList.Add(g);
    }

    private void createKeybind(string control, string key)
    {
        GameObject g = Instantiate(keybindInput, list.transform);
        g.GetComponent<SettingsInput>().Init(control, key);
        settingsList.Add(g);
    }
    private void createTitle(string title)
    {
        GameObject g = Instantiate(titleText, list.transform);
        g.GetComponentInChildren<TextMeshProUGUI>().text = title;
    }
    private void createSlider(string title, int lowVal, int highVal, int value)
    {
        GameObject g = Instantiate(sliderInput, list.transform);
        g.GetComponent<SettingsInput>().Init(title, lowVal, highVal, value);
        settingsList.Add(g);
    }



}

