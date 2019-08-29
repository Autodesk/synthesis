﻿using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using Synthesis.States;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UserSettings : MonoBehaviour
{
    private GameObject canvas;

    private GameObject settingsPanel;

    private Button loadReplayButton;
    private GameObject loadReplayPanel;

    private static float initX = float.MaxValue, initY = float.MaxValue;

    private Dropdown resDD, scrDD, qualDD;

    private Text resolutionT, screenT, qualityT;
    private bool collectAnalytics;

    private static Color ENABLED_COLOR = new Color(247 / 255f, 162 / 255f, 24 / 255f, 1f);
    private static Color DISABLED_COLOR = Color.white;// new Color(60 / 255f, 69 / 255f, 83 / 255f, 1f);
    private Slider analyticsSlider; // Used to display state only
    private Image analyticsSliderHandle;

    private List<string> resolutions;
    private List<string> screens;

    private FullScreenMode selectedScreenMode = FullScreenMode.FullScreenWindow;
    private int selectedQuality = 0;
    private string selectedResolution = "0x0";

    public GameObject unitConversionSwitch;

    public void Start()
    {
        canvas = Auxiliary.FindGameObject("Canvas");

        settingsPanel = Auxiliary.FindObject(canvas, "SettingsPanel");
        settingsPanel.SetActive(true);
        resolutionT = Auxiliary.FindObject(settingsPanel, "ResLabel").GetComponent<Text>();
        screenT = Auxiliary.FindObject(settingsPanel, "ScreenLabel").GetComponent<Text>();
        qualityT = Auxiliary.FindObject(settingsPanel, "QualLabel").GetComponent<Text>();

        resDD = Auxiliary.FindObject(settingsPanel, "ResolutionButton").GetComponent<Dropdown>();
        scrDD = Auxiliary.FindObject(settingsPanel, "ScreenModeButton").GetComponent<Dropdown>();
        qualDD = Auxiliary.FindObject(settingsPanel, "QualityButton").GetComponent<Dropdown>();
        List<Dropdown.OptionData> resOps = new List<Dropdown.OptionData>();

        analyticsSlider = Auxiliary.FindObject(settingsPanel, "AnalyticsSlider").GetComponent<Slider>();
        analyticsSliderHandle = Auxiliary.FindObject(settingsPanel, "AnalyticsSliderHandle").GetComponent<Image>();

        #region screen resolutions
        // RESOLUTIONS
        resolutions = new List<string>();

        foreach (Resolution a in Screen.resolutions)
        {
            bool g = false;
            if (resolutions.Count > 0)
            {
                resolutions.ForEach((x) => {
                    if (x.Equals(a.width + "x" + a.height))
                    {
                        g = true;
                    }
                });
            }
            if (!g)
            {
                resolutions.Add(a.width + "x" + a.height);
            }
        }

        foreach (string a in resolutions)
        {
            resOps.Add(new Dropdown.OptionData(a));
        }

        resDD.options = resOps;
        #endregion
        #region screenmodes
        // SCREENMODES
        screens = new List<string> { // Order matters
            "Fullscreen",
            "Borderless Windowed",
            "Maximized Window",
            "Windowed"
        };

        List<Dropdown.OptionData> scrOps = new List<Dropdown.OptionData>();
        screens.ForEach((x) => scrOps.Add(new Dropdown.OptionData(x)));
        scrDD.options = scrOps;
        #endregion
        #region screen qualities
        // QUALITIES
        List<Dropdown.OptionData> qualOps = new List<Dropdown.OptionData>();
        (new List<string>(QualitySettings.names)).ForEach((x) => qualOps.Add(new Dropdown.OptionData(x)));
        qualDD.options = qualOps;

        if ((int)Screen.fullScreenMode != -1) {
            selectedScreenMode = Screen.fullScreenMode;
        }
        selectedResolution = PlayerPrefs.GetString("resolution", Screen.currentResolution.width + "x" + Screen.currentResolution.height);
        selectedQuality = QualitySettings.GetQualityLevel();
        collectAnalytics = PlayerPrefs.GetInt("gatherData", 1) == 1;

        int resID = 0;
        for (int i = 0; i < resDD.options.Count; i++)
        {
            if (resDD.options[i].text.Equals(selectedResolution))
            {
                resID = i;
                break;
            }
        }
        resDD.SetValueWithoutNotify(resID);
        scrDD.SetValueWithoutNotify((int)selectedScreenMode);
        qualDD.SetValueWithoutNotify(selectedQuality);

        resolutionT.text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
        screenT.text = screens[PlayerPrefs.GetInt("fullscreen", 0)];
        qualityT.text = QualitySettings.names[PlayerPrefs.GetInt("qualityLevel", QualitySettings.GetQualityLevel())];
        analyticsSlider.value = collectAnalytics ? 1 : 0;
        analyticsSliderHandle.color = collectAnalytics ? ENABLED_COLOR : DISABLED_COLOR;
        #endregion
        #region units
        // UNITS
        unitConversionSwitch = Auxiliary.FindObject(settingsPanel, "UnitConversionSwitch");
        unitConversionSwitch.GetComponent<Slider>().value = PlayerPrefs.GetString("Measure").Equals("Metric") ? 0 : 1;
        #endregion
    }

    public void OnEnable()
    {
        this.Start();
    }

    public void LateUpdate()
    {
        resolutionT.text = selectedResolution;
        screenT.text = scrDD.options[(int)selectedScreenMode].text;
        qualityT.text = QualitySettings.names[selectedQuality];
        analyticsSlider.value = collectAnalytics ? 1 : 0;
        analyticsSliderHandle.color = collectAnalytics ? ENABLED_COLOR : DISABLED_COLOR;
    }

    public void ToggleAnalytics()
    {
        collectAnalytics = !collectAnalytics;
    }

    public void ApplySettings()
    {
        PlayerPrefs.SetString("resolution", selectedResolution);
        PlayerPrefs.SetInt("fullscreen", (int)selectedScreenMode);
        PlayerPrefs.SetInt("qualityLevel", selectedQuality);
        PlayerPrefs.SetInt("gatherData", collectAnalytics ? 1 : 0);
        PlayerPrefs.SetString("Measure", (unitConversionSwitch.GetComponent<Slider>().value == 0) ? "Metric" : "Imperial");

        string[] split = selectedResolution.Split('x');
        int xRes = int.Parse(split[0]), yRes = int.Parse(split[1]);
        Screen.SetResolution(xRes, yRes, selectedScreenMode);
        QualitySettings.SetQualityLevel(selectedQuality);
        StateMachine.SceneGlobal.FindState<MainState>().IsMetric = PlayerPrefs.GetString("Measure").Equals("Metric");
        AnalyticsManager.GlobalInstance.DumpData = PlayerPrefs.GetInt("gatherData", 1) == 1;

        UserMessageManager.Dispatch("Settings applied", 5);

        //OnCloseSettingsPanelClicked();
    }

    public void OnScrSelChanged(int a)
    {
        selectedScreenMode = (FullScreenMode)a;
    }

    public void OnResSelChanged(int b)
    {
        selectedResolution = resDD.options[b].text;
    }

    public void OnQuaSelChanged(int b)
    {
        selectedQuality = Math.Max(Array.IndexOf(QualitySettings.names, qualDD.options[b].text), 0);
    }

    public void OnCloseSettingsPanelClicked()
    {
        MenuUI.instance.SwitchSettings();
    }

    public void End()
    {
        settingsPanel.SetActive(false);
    }
}