using UnityEngine;
using UnityEngine.UI;

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;

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

        //Enable this for auto-saving. To complete auto-saving, enable the comments in KeyButton.cs > SetInput().
        //UpdateAllText();
    }

    /// <summary>
    /// Reset function; resets to tank drive defaults or arcade drive defaults.
    /// </summary>
    public void OnReset()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().ResetTankDrive();
        //if (Controls.TankDriveEnabled)
        //{
        //    //Controls.ResetTankDrive();
        //    //Controls.Save();
        //    GameObject.Find("Content").GetComponent<CreateButton>().ResetTankDrive();
        //    //Controls.Save();
        //    Debug.Log("Reset");
        //    //UpdateAllText();
        //}
        //else
        //{
        //    //Controls.ResetArcadeDrive();
        //    //Controls.Save();
        //    Debug.Log("Reset Arcade");
        //}
    }

    #region Player Buttons
    public void OnPlayerOne()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerOneButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
    }

    public void OnPlayerTwo()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerTwoButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
    }

    public void OnPlayerThree()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerThreeButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
    }

    public void OnPlayerFour()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerFourButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
    }

    public void OnPlayerFive()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerFiveButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
    }

    public void OnPlayerSix()
    {
        GameObject.Find("Content").GetComponent<CreateButton>().UpdatePlayerSixButtons();
        GameObject.Find("Content").GetComponent<CreateButton>().UpdateTankSlider();
    }
    #endregion

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