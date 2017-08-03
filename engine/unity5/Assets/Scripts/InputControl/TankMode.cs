using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankMode : MonoBehaviour
{
    //public GameObject settingsMode;

    private void Update()
    {
        //if (InputControl.GetKeyDown(KeyCode.Escape))
        //{
        //    //settingsMode.SetActive(true);
        //    //gameObject.SetActive(false);
        //}
    }

    public void OnLoadClick()
    {
        TankDrive.Load();
        UpdateAllText();
    }

    public void OnTankDrive()
    {
        TankDrive.StartTankDrive();
        TankDrive.Save();
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
