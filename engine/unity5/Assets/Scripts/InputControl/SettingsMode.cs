using UnityEngine;
using UnityEngine.UI;

//=========================================================================================
//                                      SettingsMode.cs
// Description: The sciprt that enables all the control panel functions.
// Main Content: Primarily functions attached to control panel buttons.
//=========================================================================================

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;

    public Sprite DefaultButtonImage;
    public Sprite ActiveButtonImage;

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Ignore mouse movement toggle.
    /// Current State: DISABLED 08/2017 - need to fix bugs for consistent toggle updates
    /// </summary>
    /// <param name="on"></param>
    public void OnIgnoreMouseMovementChanged(bool on)
    {
        KeyButton.ignoreMouseMovement = on;
    }

    /// <summary>
    /// Use key modifiers toggle.
    /// Current State: DISABLED 08/2017 - need to fix bugs for consistent toggle updates
    /// </summary>
    /// <param name="on"></param>
    public void OnUseKeyModifiersChanged(bool on)
    {
        KeyButton.useKeyModifiers = on;
    }

    /// <summary>
    /// Saves ALL player controls when clicked.
    /// </summary>
    public void OnSaveClick()
    {
        Controls.Save();
        UserMessageManager.Dispatch("Player preferances saved.", 5);
    }

    /// <summary>
    /// Loads ALL player controls when clicked.
    /// </summary>
    public void OnLoadClick()
    {
        Controls.Load();
    }

    /// <summary>
    /// Resets ALL player controls to tank drive or arcade drive defaults when clicked.
    /// </summary>
    public void OnReset()
    {
        if (InputControl.mPlayerList[InputControl.activePlayerIndex].isTankDrive)
        {
            GameObject.Find("Content").GetComponent<CreateButton>().ResetTankDrive();
            Controls.Save();
        }
        else
        {
            GameObject.Find("Content").GetComponent<CreateButton>().ResetArcadeDrive();
            Controls.Save();
        }
    }

    /// <summary>
    /// Allows the player to toggle their drive preferences between arcade and tank drive.
    /// </summary>
    public void OnTankToggle()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().TankSlider();
    }

    //=========================================================================================
    //                                     Update Player Buttons
    // Creates and updates a specific player and their control's list. Checks for possible
    // toggle updates and if controls were saved.
    //=========================================================================================
    #region Player Buttons
    public void OnPlayerOne()
    {
        //Creates and generates player one's keys and control buttons
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();

        //Checks if the tank drive toggle/slider needs to be updated (according to the player)
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();

        //If the user did not press the save button, revert back to the last loaded and saved controls (no auto-save.)
        GetLastSavedControls();
    }

    public void OnPlayerTwo()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerTwoButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
        GetLastSavedControls();
    }

    public void OnPlayerThree()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerThreeButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
        GetLastSavedControls();
    }

    public void OnPlayerFour()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerFourButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
        GetLastSavedControls();
    }

    public void OnPlayerFive()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerFiveButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
        GetLastSavedControls();
    }

    public void OnPlayerSix()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerSixButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
        GetLastSavedControls();
    }
    #endregion

    /// <summary>
    /// Updates the active player button to the active button style. This makes the button
    /// appear highlighted (and stay highlighted) when the player clicks on a specific button.
    /// </summary>
    public void UpdateButtonStyle()
    {
        switch (InputControl.activePlayerIndex)
        {
            #region Active and Default Button Styles
            case 0:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = ActiveButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultButtonImage;
                break;
            case 1:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = ActiveButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultButtonImage;
                break;
            case 2:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = ActiveButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultButtonImage;
                break;
            case 3:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = ActiveButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultButtonImage;
                break;
            case 4:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = ActiveButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultButtonImage;
                break;
            case 5:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = ActiveButtonImage;
                break;
            default: //Default to player one as active
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = ActiveButtonImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultButtonImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultButtonImage;
                break;
                #endregion
        }
    }

    /// <summary>
    /// Gets the last loaded controls if the player did not press the "Save" button.
    /// Helps prevent auto-saving (in case a user accidentally changes their controls.)
    /// </summary>
    public void GetLastSavedControls()
    {
        Controls.Load();
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
    }

    /// <summary>
    /// Updates all the key/control buttons.
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