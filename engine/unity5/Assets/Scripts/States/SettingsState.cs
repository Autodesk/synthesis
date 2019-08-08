﻿using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsState : State {

    private GameObject canvas;
    private GameObject me;

    private static float initX = float.MaxValue, initY = float.MaxValue;

    private Dropdown resDD, scrDD, qualDD;

    private Text resolutionT, screenT, qualityT, analyticsT;
    private int collect = 1;

    private List<string> resolutions;
    private List<string> screens;

    private int selectedScreenMode = 0;
    private int selectedQuality = 0;
    private string selectedResolution = "0x0";

    public override void Start()
    {
        canvas = Auxiliary.FindGameObject("Canvas");
        me = Auxiliary.FindObject(canvas, "SettingsPanel");
        me.SetActive(true);
        resolutionT = Auxiliary.FindObject(me, "ResLabel").GetComponent<Text>();
        screenT = Auxiliary.FindObject(me, "ScreenLabel").GetComponent<Text>();
        qualityT = Auxiliary.FindObject(me, "QualLabel").GetComponent<Text>();
        analyticsT = Auxiliary.FindObject(me, "AnalyticsModeText").GetComponent<Text>();

        resDD = Auxiliary.FindObject(me, "ResolutionButton").GetComponent<Dropdown>();
        scrDD = Auxiliary.FindObject(me, "ScreenModeButton").GetComponent<Dropdown>();
        qualDD = Auxiliary.FindObject(me, "QualitySettings").GetComponent<Dropdown>();
        List<Dropdown.OptionData> resOps = new List<Dropdown.OptionData>();

        SimUI.getSimUI().OnResolutionSelection += OnResSelChanged;
        SimUI.getSimUI().OnScreenmodeSelection += OnScrSelChanged;
        SimUI.getSimUI().OnQualitySelection += OnQuaSelChanged;

        // RESOLUTIONS
        resolutions = new List<string>();

        foreach (Resolution a in Screen.resolutions) {
            bool g = false;
            if (resolutions.Count > 0) {
                resolutions.ForEach((x) => {
                    if (x.Equals(a.width + "x" + a.height)) {
                        g = true;
                    }
                });
            }
            if (!g) {
                resolutions.Add(a.width + "x" + a.height);
            }
        }

        foreach (string a in resolutions) {
            resOps.Add(new Dropdown.OptionData(a));
        }

        resDD.options = resOps;

        // SCREENMODES
        screens = new List<string>();
        screens.Add("Windowed");
        screens.Add("Fullscreen");

        List<Dropdown.OptionData> scrOps = new List<Dropdown.OptionData>();
        screens.ForEach((x) => scrOps.Add(new Dropdown.OptionData(x)));
        scrDD.options = scrOps;

        // QUALITIES
        List<Dropdown.OptionData> qualOps = new List<Dropdown.OptionData>();
        (new List<string>(QualitySettings.names)).ForEach((x) => qualOps.Add(new Dropdown.OptionData(x)));
        qualDD.options = qualOps;

        selectedScreenMode = Screen.fullScreen ? 1 : 0;
        selectedResolution = PlayerPrefs.GetString("resolution", Screen.currentResolution.width + "x" + Screen.currentResolution.height);
        selectedQuality = QualitySettings.GetQualityLevel();
        collect = PlayerPrefs.GetInt("gatherData", 1);

        int resID = 0;
        for (int i = 0; i < resDD.options.Count; i++) {
            if (resDD.options[i].text.Equals(selectedResolution)) {
                resID = i;
                break;
            }
        }
        resDD.SetValueWithoutNotify(resID);
        scrDD.SetValueWithoutNotify(selectedScreenMode);
        qualDD.SetValueWithoutNotify(selectedQuality);

        resolutionT.text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
        screenT.text = screens[PlayerPrefs.GetInt("fullscreen", 0)];
        qualityT.text = QualitySettings.names[PlayerPrefs.GetInt("qualityLevel", QualitySettings.GetQualityLevel())];
        analyticsT.text = collect == 1 ? "Yes" : "No";

        if (initX == float.MaxValue)
        {
            initX = 0;
            initY = me.GetComponent<RectTransform>().anchoredPosition.y;
        } else
        {
            me.GetComponent<RectTransform>().anchoredPosition = new Vector2(initX, initY);
        }
    }

    public override void LateUpdate() {
        resolutionT.text = selectedResolution;
        screenT.text = scrDD.options[selectedScreenMode].text;
        qualityT.text = QualitySettings.names[selectedQuality];
        analyticsT.text = collect == 1 ? "Yes" : "No";
    }

    public void OnToggleAnalyticsClicked()
    {
        collect = collect == 1 ? 0 : 1;
        analyticsT.text = collect == 1 ? "Yes" : "No";
    }

    /*public void OnScreenModeButtonClicked()
    {
        screenIndex = (screenIndex + 1) % screenModes.Length;
        screenT.text = screenModes[screenIndex];
    }

    public void OnResolutionButtonClicked()
    {
        resolutionIndex = (resolutionIndex + 1) % resolutions.Length;
        resolutionT.text = resolutions[resolutionIndex];
    }

    public void OnQualitySettingsClicked()
    {
        qualityIndex = (qualityIndex + 1) % QualitySettings.names.Length;
        qualityT.text = QualitySettings.names[qualityIndex];
    }*/

    public void OnApplySettingsClicked()
    {
        PlayerPrefs.SetString("resolution", selectedResolution);
        PlayerPrefs.SetInt("fullscreen", selectedScreenMode);
        PlayerPrefs.SetInt("qualityLevel", selectedQuality);
        PlayerPrefs.SetInt("gatherData", collect);
        string[] split = selectedResolution.Split('x');
        int xRes = int.Parse(split[0]), yRes = int.Parse(split[1]);
        Screen.SetResolution(xRes, yRes, PlayerPrefs.GetInt("fullscreen") != 0);
        QualitySettings.SetQualityLevel(selectedQuality);
        AnalyticsManager.GlobalInstance.DumpData = PlayerPrefs.GetInt("gatherData", 1) == 1;

        OnCloseSettingsPanelClicked();
    }

    public void OnScrSelChanged(int a) {

        selectedScreenMode = a;

        SimUI.getSimUI().OnScreenmodeSelection += OnScrSelChanged;
    }

    public void OnResSelChanged(int b) {
        string entry = resDD.options[b].text;
        selectedResolution = entry;

        SimUI.getSimUI().OnResolutionSelection += OnResSelChanged;
    }

    public void OnQuaSelChanged(int b) {
        string entry = qualDD.options[b].text;
        int a;
        selectedQuality = (a = (new List<string>(QualitySettings.names)).IndexOf(entry)) == -1 ? 0 : a;

        Debug.Log("Is your card... " + selectedQuality);

        SimUI.getSimUI().OnQualitySelection += OnQuaSelChanged;
    }

    public void OnCloseSettingsPanelClicked()
    {
        SimUI.getSimUI().OnSettingsTab();
    }

    public override void End()
    {
        me.SetActive(false);
    }

}
