using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;

public class OptionsTabState : State
{
    private GameObject graphics;
    private GameObject input;
    private GameObject settingsMode;
    private GameObject splashScreen;

    /// <summary>
    /// Establishes references to required <see cref="GameObject"/>s.
    /// </summary>
    public override void Start()
    {
        graphics = AuxFunctions.FindGameObject("Graphics");
        input = AuxFunctions.FindGameObject("Input");
        settingsMode = AuxFunctions.FindGameObject("SettingsMode");
        splashScreen = AuxFunctions.FindGameObject("LoadSplash");

        OnInputButtonPressed();

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().GetLastSavedControls();
    }

    /// <summary>
    /// Activates the input tab when the input button is pressed.
    /// </summary>
    public void OnInputButtonPressed()
    {
        graphics.SetActive(false);
        input.SetActive(true);
        settingsMode.SetActive(true);
    }

    /// <summary>
    /// Activates the graphics tab when the graphics button is pressed.
    /// </summary>
    public void OnGraphicsButtonPressed()
    {
        graphics.SetActive(true);
        input.SetActive(false);
        settingsMode.SetActive(true);
    }

    /// <summary>
    /// Changes the quality settings label when the quality settings button is pressed.
    /// </summary>
    public void OnQualitySettingsPressed()
    {
        QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length);
        GameObject.Find("QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    /// <summary>
    /// Applies the graphics settings when the apply button is pressed.
    /// </summary>
    public void OnApplyButtonPressed()
    {
        PopupButton resPopup = GameObject.Find("ResolutionButton").GetComponent<PopupButton>();
        int xRes;
        int yRes;

        ParseResolution(resPopup.list[PlayerPrefs.GetInt("resolution")].text, out xRes, out yRes);

        Screen.SetResolution(xRes, yRes, PlayerPrefs.GetInt("fullscreen") != 0);
        StateMachine.Instance.ChangeState(new HomeTabState());
    }

    /// <summary>
    /// Parses the given string representation of a screen resolution and assigns
    /// values to the provided out parameters.
    /// </summary>
    /// <param name="resolution"></param>
    /// <param name="xRes"></param>
    /// <param name="yRes"></param>
    private void ParseResolution(string resolution, out int xRes, out int yRes)
    {
        string[] components = resolution.Split('x');
        xRes = int.Parse(components[0]);
        yRes = int.Parse(components[1]);
    }
}
