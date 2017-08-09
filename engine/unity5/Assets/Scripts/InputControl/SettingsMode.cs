﻿using UnityEngine;
using UnityEngine.UI;

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;
    private Text enableTankDriveText;

    // Update is called once per frame
    void Update()
    {

    }

    public void OnIgnoreMouseMovementChanged(bool on)
    {
        KeyButton.ignoreMouseMovement = on;
    }

    public void OnUseKeyModifiersChanged(bool on)
    {
        KeyButton.useKeyModifiers = on;
    }

    public void OnSaveClick()
    {
        Controls.Save();
        UserMessageManager.Dispatch("Player preferances saved.", 5);
    }

    public void OnLoadClick()
    {
        Controls.Load();
        UpdateAllText();
    }

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

    public void UpdateTankText()
    {
        if (Controls.IsTankDrive)
        {
            enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
            enableTankDriveText.text = "Switch Arcade Drive";
        }
        else
        {
            enableTankDriveText = AuxFunctions.FindObject(gameObject, "EnableTankDriveText").GetComponent<Text>();
            enableTankDriveText.text = "Switch Tank Drive";
        }
    }

    public void UpdateAllText()
    {
        KeyButton[] keyButtons = GetComponentsInChildren<KeyButton>();

        foreach (KeyButton keyButton in keyButtons)
        {
            keyButton.updateText();
        }
    }
}