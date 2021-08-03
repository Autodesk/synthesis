using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Synthesis.PreferenceManager;
public static class Preference{
    public const string RESOLUTION = "Resolution";//Dropdown: Different resolution settings
    public static List<string> ResolutionList = new List<string>{"1920x1080"};    
    public const string SCREEN_MODE = "Screen Mode";//Dropdown: Fullscreen or Windowed
    public static List<string> ScreenModeList = new List<string>{"Fullscreen","Windowed"}; //OLD SYNTHESIS SUCKS AT THIS   
    public const string QUALITY_SETTINGS = " Quality Settings";//Dropdown: Low Medium High
    public static List<string> QualitySettingsList = new List<string>{"High","Medium","Low"};    
    public const string ALLOW_DATA_GATHERING = "Allow Data Gathering";//Toggle
    public const string MEASUREMENTS = "Measurements";//Dropdown: Metric or Imperial
    public static List<string> MeasurementsList = new List<string>{"Imperial","Metric"};   
    public const string ZOOM_SENSITIVITY = "Zoom Sensitivity";
    public const string YAW_SENSITIVITY = "Yaw Sensitivity";
    public const string PITCH_SENSITIVITY = "Pitch Sensitivity";

    //sliders for camera controller

    //sliders


    public static void LoadSettings(){
            PreferenceManager.Load();
            if(PreferenceManager.GetPreference(Preference.RESOLUTION)==null){//checks if preferences are initialized with default values
                setDefaultPreferences();
            }

            //implement settings by going through each, loading, and setting
    }
    public static void setDefaultPreferences(){
        PreferenceManager.SetPreference(Preference.RESOLUTION,(int)0);
        PreferenceManager.SetPreference(Preference.SCREEN_MODE,(int)0);
        PreferenceManager.SetPreference(Preference.QUALITY_SETTINGS,(int)0);
        PreferenceManager.SetPreference(Preference.ALLOW_DATA_GATHERING,(bool)true);
        PreferenceManager.SetPreference(Preference.MEASUREMENTS,(int)0);
        PreferenceManager.SetPreference(Preference.ZOOM_SENSITIVITY,(int)5);
        PreferenceManager.SetPreference(Preference.YAW_SENSITIVITY,(int)10);
        PreferenceManager.SetPreference(Preference.PITCH_SENSITIVITY,(int)3);
        PreferenceManager.Save();        
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

    /*
    At the start: Load Preference Manager
    Script that converts preference manager into the proper implemented settings

    When Panel is loaded:
    Show Settings goes through each preference and gets them and their state, creates the parts

    When save button is pressed: save changes, then call the load function to apply them

    When close is pressed with unsaved changes: issue warning saying you have unsaved changes    


    */


    void Start()
    {
        Preference.LoadSettings();
        showSettings();
    }

    private void showSettings()
    {        
        createTitle("Screen Settings");
        createDropdown(Preference.RESOLUTION, Preference.ResolutionList, Convert.ToInt32(PreferenceManager.GetPreference(Preference.RESOLUTION)));
        createDropdown(Preference.SCREEN_MODE,Preference.ScreenModeList,Convert.ToInt32(PreferenceManager.GetPreference(Preference.SCREEN_MODE)));
        createDropdown(Preference.QUALITY_SETTINGS,Preference.QualitySettingsList,Convert.ToInt32(PreferenceManager.GetPreference(Preference.QUALITY_SETTINGS)));
        
        createTitle("Camera Settings");
        createSlider(Preference.ZOOM_SENSITIVITY,1,15,Convert.ToInt32(PreferenceManager.GetPreference(Preference.ZOOM_SENSITIVITY)));
        createSlider(Preference.YAW_SENSITIVITY,1,15,Convert.ToInt32(PreferenceManager.GetPreference(Preference.YAW_SENSITIVITY)));
        createSlider(Preference.PITCH_SENSITIVITY,1,15,Convert.ToInt32(PreferenceManager.GetPreference(Preference.PITCH_SENSITIVITY)));

        createTitle("Preferences");        
        createToggle(Preference.ALLOW_DATA_GATHERING,(bool)PreferenceManager.GetPreference(Preference.ALLOW_DATA_GATHERING));
        createDropdown(Preference.MEASUREMENTS, Preference.MeasurementsList, Convert.ToInt32(PreferenceManager.GetPreference(Preference.MEASUREMENTS)));
    }

    //Depricate it: Have settings saved within each object when changed. Then just call preference manager save on save.
    public void saveSettings()//writes settings back into preference manager when save button is clicked
    {
        foreach(GameObject g in settingsList)
        {
            //MODIFY THIS: Each Input field has different outputs
            SettingsInput si = g.GetComponent<SettingsInput>();
            string name = si._name.text; //key for preference manager
            
            switch (si.getType())//sets each value based on type. Needs to be changed when writing to preference manager
            {
                case InputType.TOGGLE:
                    PreferenceManager.SetPreference(name,si._toggle.isOn);
                    break;
                case InputType.DROPDOWN:
                    PreferenceManager.SetPreference(name,si._dropdown.value);
                    break;
                case InputType.KEYBIND:
                    PreferenceManager.SetPreference(name,si._key.text);
                    break;
                case InputType.SLIDER:
                    PreferenceManager.SetPreference(name,si._slider.value);
                    break;
            }
        }
        PreferenceManager.Save();
    }

    private void createDropdown(string title, List<string> dropdownList, int value)
    {
        GameObject g = Instantiate(dropdownInput, list.transform);
        g.GetComponent<SettingsInput>().Init(title, dropdownList,value);
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
       g.GetComponentInChildren<TextMeshProUGUI>().text=title;
    }
    private void createSlider(string title, int lowVal, int highVal, int value)
    {
        GameObject g = Instantiate(sliderInput, list.transform);
        g.GetComponent<SettingsInput>().Init(title, lowVal,highVal,value);
        settingsList.Add(g);
    }
}

