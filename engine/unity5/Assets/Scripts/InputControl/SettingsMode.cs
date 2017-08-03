using UnityEngine;

public class SettingsMode : MonoBehaviour
{
    //public GameObject tankMode;

    // Update is called once per frame
    void Update()
    {
        //if (KeyButton.selectedButton == null && InputControl.GetKeyDown(KeyCode.Escape))
        //{
        //    tankMode.SetActive(true);
        //    //gameObject.SetActive(false);
        //}
    }

    //public void Test()
    //{
    //    tankMode.SetActive(true);
    //}

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
        Controls.Reset();
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