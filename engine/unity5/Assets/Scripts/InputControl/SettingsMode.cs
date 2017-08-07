using UnityEngine;

public class SettingsMode : MonoBehaviour
{
    public GameObject settingsMode;

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
        Controls.ArcadeDrive();
        Controls.Save();
    }

    //public void OnTankDrive()
    //{
    //    Controls.IsTankDrive = true;
    //    Controls.TankDrive();
    //    Controls.Save();
    //}


    public void UpdateAllText()
    {
        KeyButton[] keyButtons = GetComponentsInChildren<KeyButton>();

        foreach (KeyButton keyButton in keyButtons)
        {
            keyButton.updateText();
        }
    }
}