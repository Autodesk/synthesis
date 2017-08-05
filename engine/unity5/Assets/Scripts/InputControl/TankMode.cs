using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMode : MonoBehaviour
{
    public GameObject tankMode;
    public bool enabled = false;

    private void Update()
    {

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
        Controls.TankDrive();
        Controls.Save();
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
