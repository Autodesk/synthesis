using UnityEngine;
using UnityEngine.UI;

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;

    //public Button button;
    public Sprite DefaultImage;
    public Sprite HighlightedImage;

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
    }

    /// <summary>
    /// Reset function; resets to tank drive defaults or arcade drive defaults.
    /// </summary>
    public void OnReset()
    {
        if (Controls.TankDriveEnabled)
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

    public void GetLastSavedControls()
    {
        Controls.Load();
        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
    }

    /// <summary>
    /// Updates and creates the appropriate list for each player when the coordinated button is clicked.
    /// </summary>
    #region Player Buttons
    public void OnPlayerOne()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
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
    /// Updates the active player button to the active player style. This makes the button
    /// appear highlighted (and stay highlighted) when the player clicks on a specific button.
    /// </summary>
    public void UpdateButtonStyle()
    {
        switch (InputControl.activePlayerIndex)
        {
            case 0:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = HighlightedImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultImage;
                break;
            case 1:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = HighlightedImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultImage;
                break;
            case 2:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = HighlightedImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultImage;
                break;
            case 3:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = HighlightedImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultImage;
                break;
            case 4:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = HighlightedImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultImage;
                break;
            case 5:
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = HighlightedImage;
                break;
            default: //Default to player one's button styles
                GameObject.Find("PlayerOne Button").GetComponent<Image>().sprite = HighlightedImage;
                GameObject.Find("PlayerTwo Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerThree Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFour Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerFive Button").GetComponent<Image>().sprite = DefaultImage;
                GameObject.Find("PlayerSix Button").GetComponent<Image>().sprite = DefaultImage;
                break;
        }
    }

    public void OnTankToggle()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().TankSlider();
    }

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