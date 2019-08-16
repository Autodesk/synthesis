using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Utils;
using Synthesis.States;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserSettings : MonoBehaviour
{
    private GameObject canvas;

    private GameObject settingsPanel;

    private Button loadReplayButton;
    private GameObject loadReplayPanel;

    private static float initX = float.MaxValue, initY = float.MaxValue;

    private Dropdown resDD, scrDD, qualDD;

    private Text resolutionT, screenT, qualityT, analyticsT;
    private int collect = 1;

    private List<string> resolutions;
    private List<string> screens;

    private int selectedScreenMode = 0;
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
        analyticsT = Auxiliary.FindObject(settingsPanel, "AnalyticsModeText").GetComponent<Text>();

        resDD = Auxiliary.FindObject(settingsPanel, "ResolutionButton").GetComponent<Dropdown>();
        scrDD = Auxiliary.FindObject(settingsPanel, "ScreenModeButton").GetComponent<Dropdown>();
        qualDD = Auxiliary.FindObject(settingsPanel, "QualitySettings").GetComponent<Dropdown>();
        List<Dropdown.OptionData> resOps = new List<Dropdown.OptionData>();

        MenuUI.instance.OnResolutionSelection += OnResSelChanged;
        MenuUI.instance.OnScreenmodeSelection += OnScrSelChanged;
        MenuUI.instance.OnQualitySelection += OnQuaSelChanged;

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

        // UNITS
        unitConversionSwitch = Auxiliary.FindObject("UnitConversionSwitch");
        unitConversionSwitch = Auxiliary.FindObject("UnitConversionSwitch");
        unitConversionSwitch.GetComponent<Slider>().value = PlayerPrefs.GetString("Measure").Equals("Metric") ? 0 : 1;
    }

    public void OnEnable()
    {
        this.Start();
    }

    public void LateUpdate() {
        resolutionT.text = selectedResolution;
        screenT.text = scrDD.options[selectedScreenMode].text;
        qualityT.text = QualitySettings.names[selectedQuality];
        analyticsT.text = collect == 1 ? "Yes" : "No";
    }

    public void ToggleAnalytics()
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

    /// <summary>
    /// Pushes the load replay state.
    /// </summary>
    //private void PushLoadReplayState()
    //{
    //    settingsPanel.SetActive(false);
    //    StateMachine.ChangeState(new LoadReplayState());
    //    Debug.Log("pushed");
    //}

    /// <summary>
    /// Pops the current<see cref="State"/> when the back button is pressed.
    /// </summary>
    //public void OnCancelButtonClicked()
    //{
    //    StateMachine.PopState();
    //}

    public void ApplySettings()
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

        UserMessageManager.Dispatch("Settings applied", 10);

        //OnCloseSettingsPanelClicked();
    }

    public void OnScrSelChanged(int a) {

        selectedScreenMode = a;

        MenuUI.instance.OnScreenmodeSelection += OnScrSelChanged;
    }

    public void OnResSelChanged(int b) {
        string entry = resDD.options[b].text;
        selectedResolution = entry;

        MenuUI.instance.OnResolutionSelection += OnResSelChanged;
    }

    public void OnQuaSelChanged(int b) {
        string entry = qualDD.options[b].text;
        int a;
        selectedQuality = (a = (new List<string>(QualitySettings.names)).IndexOf(entry)) == -1 ? 0 : a;

        Debug.Log("Is your card... " + selectedQuality);

        MenuUI.instance.OnQualitySelection += OnQuaSelChanged;
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
