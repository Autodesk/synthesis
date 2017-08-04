using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMode : MonoBehaviour
{
    public GameObject tankMode;

    private void Update()
    {

    }

    public void OnLoadClick()
    {
        Controls.Load();
        UpdateAllText();
    }

    //public void OnTankDrive()
    //{
    //    Controls.StartTankDrive();
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
