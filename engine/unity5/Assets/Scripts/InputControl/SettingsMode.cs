using UnityEngine;
using UnityEngine.UI;

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;
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
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerOne();
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
        if (Controls.IsTankDrive)
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
    public void OnEnableTankDrive()
    {
        if (!Controls.TankDriveEnabled)
        {
            Controls.TankDriveEnabled = true;
            Controls.IsTankDrive = true;
            Controls.CheckForKeyRemoval = false;
            Controls.SwitchControls();
            Controls.Save();
            enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
            enableTankDriveText.text = "Switch Arcade Drive";
        }
        else
        {
            Controls.TankDriveEnabled = false;
            Controls.IsTankDrive = false;
            Controls.CheckForKeyRemoval = true;
            Controls.SwitchControls();
            Controls.Save();
            enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
            enableTankDriveText.text = "Switch Tank Drive";
        }
    }

    #region Player Buttons
    public void OnPlayerOne()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerOne();
    }

    public void OnPlayerTwo()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerTwo();
    }

    public void OnPlayerThree()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerThree();
    }

    public void OnPlayerFour()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerFour();
    }

    public void OnPlayerFive()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerFive();
    }

    public void OnPlayerSix()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().PlayerSix();
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