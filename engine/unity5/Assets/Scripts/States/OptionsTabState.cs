using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;

// TODO: Make it so all the buttons behave the same way - the popup menu shouldn't be a thing

public class OptionsTabState : State
{
    private readonly int[] xresolution = new int[9];
    private readonly int[] yresolution = new int[9];

    private GameObject graphics;
    private GameObject input;
    private GameObject settingsMode;
    private GameObject splashScreen;

    private bool fullscreen;
    private int resolutionsetting;

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

        xresolution[0] = 1024;
        xresolution[1] = 1280;
        xresolution[2] = 1280;
        xresolution[3] = 1280;
        xresolution[4] = 1400;
        xresolution[5] = 1600;
        xresolution[6] = 1680;
        xresolution[7] = 1920;
        xresolution[8] = Screen.currentResolution.width;

        yresolution[0] = 768;
        yresolution[1] = 720;
        yresolution[2] = 768;
        yresolution[3] = 1024;
        yresolution[4] = 900;
        yresolution[5] = 900;
        yresolution[6] = 1050;
        yresolution[7] = 1080;
        yresolution[8] = Screen.currentResolution.height;

        fullscreen = (PlayerPrefs.GetInt("fullscreen", 0) == 1);
        int width = xresolution[8];
        int height = yresolution[8];
        if (width == xresolution[0] && height == yresolution[0]) resolutionsetting = 0;
        else if (width == xresolution[1] && height == yresolution[1]) resolutionsetting = 1;
        else if (width == xresolution[2] && height == yresolution[2]) resolutionsetting = 2;
        else if (width == xresolution[3] && height == yresolution[3]) resolutionsetting = 3;
        else if (width == xresolution[4] && height == yresolution[4]) resolutionsetting = 4;
        else if (width == xresolution[5] && height == yresolution[5]) resolutionsetting = 5;
        else if (width == xresolution[6] && height == yresolution[6]) resolutionsetting = 6;
        else if (width == xresolution[7] && height == yresolution[7]) resolutionsetting = 7;
        else resolutionsetting = 8;
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

    public void OnQualitySettingsPressed()
    {
        QualitySettings.SetQualityLevel((QualitySettings.GetQualityLevel() + 1) % QualitySettings.names.Length);
        GameObject.Find("QualitySettingsText").GetComponent<Text>().text = QualitySettings.names[QualitySettings.GetQualityLevel()];
    }

    public void OnApplyButtonPressed()
    {
        Screen.SetResolution(xresolution[resolutionsetting], yresolution[resolutionsetting], fullscreen);
        PlayerPrefs.SetInt("fullscreen", (fullscreen ? 1 : 0));
        StateMachine.Instance.ChangeState(new HomeTabState());
    }
}
