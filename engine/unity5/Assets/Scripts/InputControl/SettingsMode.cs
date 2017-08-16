using UnityEngine;
using UnityEngine.UI;

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;
    GameObject tankDriveSwitch;
    private Text enableTankDriveText;

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Ignore mouse movement toggle.
    /// </summary>
    /// <param name="on"></param>
    public void OnIgnoreMouseMovementChanged(bool on)
    {
        KeyButton.ignoreMouseMovement = on;
    }

    /// <summary>
    /// Use key modifiers toggle.
    /// </summary>
    /// <param name="on"></param>
    public void OnUseKeyModifiersChanged(bool on)
    {
        KeyButton.useKeyModifiers = on;
    }

    /// <summary>
    /// Save function.
    /// </summary>
    public void OnSaveClick()
    {
        Controls.Save();
        UserMessageManager.Dispatch("Player preferances saved.", 5);
    }

    /// <summary>
    /// Load function; updates buttons when user changes the input
    /// </summary>
    public void OnLoadClick()
    {
        Controls.Load();
        UpdateAllText();
    }

    /// <summary>
    /// Reset function; resets to tank drive defaults or arcade drive defaults.
    /// </summary>
    public void OnReset()
    {
        if (Controls.TankDriveEnabled)
        {
            Controls.ResetTankDrive();
            Controls.Save();
        }
        else
        {
            Controls.ResetArcadeDrive();
            Controls.Save();
        }
    }

    /// <summary>
    /// Enables tank drive.
    /// </summary>
    //public void OnEnableTankDrive()
    //{
    //    //TankDriveEnabled is true
    //    if (!Controls.TankDriveEnabled)
    //    {
    //        Controls.TankDriveEnabled = true;
    //        Controls.CheckForKeyRemoval = false;
    //        //Controls.SwitchControls();
    //        GameObject.Find("Player").GetComponent<Player>().SetTankDrive();
    //        Controls.Save();
    //        enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
    //        enableTankDriveText.text = "Switch Arcade Drive";
    //    }
    //    else
    //    {
    //        //TankDriveEnabled is false
    //        Controls.TankDriveEnabled = false;
    //        Controls.CheckForKeyRemoval = true;
    //        Controls.SwitchControls();
    //        GameObject.Find("Player").GetComponent<Player>().SetArcadeDrive();
    //        Controls.Save();
    //        enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
    //        enableTankDriveText.text = "Switch Tank Drive";
    //    }
    //}

    //void OnEnable()
    //{
    //    if (GameObject.Find("SettingsMode") != null)
    //    {
    //        if (!Controls.TankDriveEnabled)
    //        {
    //            enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
    //            enableTankDriveText.text = "Switch Tank Drive";
    //        }
    //        else
    //        {
    //            enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
    //            enableTankDriveText.text = "Switch Arcade Drive";
    //        }
    //    }

    //}

    //public void ToggleUnitConversion()
    //{
    //    int i = (int)unitConversionSwitch.GetComponent<Slider>().value;

    //    main.IsMetric = (i == 1 ? true : false);
    //}

    public void OnTankToggle()
    {
        tankDriveSwitch = AuxFunctions.FindObject("TankDriveSwitch");
        int i = (int)tankDriveSwitch.GetComponent<Slider>().value;

        //if (Player.isTankDrive)

        switch(i)
        {
            case 0:
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetArcadeDrive();
                break;
            case 1:
                InputControl.mPlayerList[InputControl.activePlayerIndex].SetTankDrive();
                break;
        }
    }

    //public static OnValueChanged()
    //{
    //get slider component
    //if slider = 0 (tank drive is off)
    //SetArcadeDrive() = active player index
    //mPlayerList[activePlayerIndex].SetArcadeDrive();
    //else tank drive on = 1
    //SetTankDrive()
    //so basically the OnValueChanged() function called by the slider will find the slider object
    //(which hopefully has a decent name) and then check its value. If it is 0 (tank drive off), then call 
    //SetArcadeDrive on the active player (which can be found with the active player index), else it is 1, 
    //then call SetTankDrive
    //public void ToggleUnitConversion()
    //{
    //    int i = (int)unitConversionSwitch.GetComponent<Slider>().value;

    //    main.IsMetric = (i == 1 ? true : false);
    //}
    //}

    //unitConversionSwitch = AuxFunctions.FindObject(canvas, "UnitConversionSwitch");

    #region Player Buttons
    public void OnPlayerOne()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();
    }

    public void OnPlayerTwo()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerTwoButtons();
    }

    public void OnPlayerThree()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerThreeButtons();
    }

    public void OnPlayerFour()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerFourButtons();
    }

    public void OnPlayerFive()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerFiveButtons();
    }

    public void OnPlayerSix()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerSixButtons();
    }
    #endregion

    /// <summary>
    /// Updates all the key buttons.
    /// </summary>
    public void UpdateAllText()
    {
        KeyButton[] keyButtons = GetComponentsInChildren<KeyButton>();

        foreach (KeyButton keyButton in keyButtons)
        {
            keyButton.UpdateText();
        }
    }
}