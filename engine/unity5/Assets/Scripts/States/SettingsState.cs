using Synthesis.FSM;
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

    private Text resolutionT, screenT, qualityT, analyticsT;

    private int resolutionIndex = 0, screenIndex = 0, qualityIndex = 0;
    private int collect = 1;

    private string[] resolutions =
    {
        "1024x768",
        "1280x720",
        "1280x768",
        "1280x1024",
        "1400x900",
        "1600x900",
        "1680x1050",
        "1920x1080"
    };

    private string[] screenModes =
    {
        "Fullscreen",
        "Windowed"
    };

    public override void Start()
    {
        canvas = Auxiliary.FindGameObject("Canvas");
        me = Auxiliary.FindObject(canvas, "SettingsPanel");
        me.SetActive(true);
        resolutionT = Auxiliary.FindObject(me, "ResolutionText").GetComponent<Text>();
        screenT = Auxiliary.FindObject(me, "ScreenModeText").GetComponent<Text>();
        qualityT = Auxiliary.FindObject(me, "QualitySettingsText").GetComponent<Text>();
        analyticsT = Auxiliary.FindObject(me, "AnalyticsModeText").GetComponent<Text>();

        screenIndex = PlayerPrefs.GetInt("fullscreen") == 0 ? 1 : 0;
        int resInd = (new List<string>(resolutions)).IndexOf(PlayerPrefs.GetString("resolution"));
        resolutionIndex = resInd == -1 ? 0 : resInd;
        qualityIndex = PlayerPrefs.GetInt("qualityLevel");
        collect = PlayerPrefs.GetInt("gatherData", 1);


        resolutionT.text = Screen.currentResolution.width + "x" + Screen.currentResolution.height;
        screenT.text = PlayerPrefs.GetInt("fullscreen") == 0 ? "Windowed" : "Fullscreen";
        qualityT.text = QualitySettings.names[PlayerPrefs.GetInt("qualityLevel")];
        analyticsT.text = collect == 1 ? "Yes" : "No";

        if (initX == float.MaxValue)
        {
            Debug.Log("X " + me.transform.position.x);
            initX = 0;
            initY = me.GetComponent<RectTransform>().anchoredPosition.y;
            Debug.Log("Init");
        } else
        {
            me.GetComponent<RectTransform>().anchoredPosition = new Vector2(initX, initY);
            Debug.Log("Reset Pos");
        }
    }

    public void OnToggleAnalyticsClicked()
    {
        collect = collect == 1 ? 0 : 1;
        analyticsT.text = collect == 1 ? "Yes" : "No";
    }

    public void OnScreenModeButtonClicked()
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
    }

    public void OnApplySettingsClicked()
    {
        PlayerPrefs.SetString("resolution", resolutions[resolutionIndex]);
        PlayerPrefs.SetInt("fullscreen", screenIndex == 0 ? 1 : 0);
        PlayerPrefs.SetInt("qualityLevel", qualityIndex);
        PlayerPrefs.SetInt("gatherData", collect);
        string[] split = resolutions[resolutionIndex].Split('x');
        int xRes = int.Parse(split[0]), yRes = int.Parse(split[1]);
        Screen.SetResolution(xRes, yRes, PlayerPrefs.GetInt("fullscreen") != 0);
        QualitySettings.SetQualityLevel(qualityIndex);
        AnalyticsManager.GlobalInstance.DumpData = PlayerPrefs.GetInt("gatherData", 1) == 1;

        OnCloseSettingsPanelClicked();
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
