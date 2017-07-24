using UnityEngine;

public class SettingsMode : MonoBehaviour
{
    public GameObject gameMode;

    // Update is called once per frame
    void Update()
    {
        if (
            KeyButton.selectedButton == null
            &&
            InputControl.GetKeyDown(KeyCode.Escape)
           )
        {
            gameMode.SetActive(true);
            gameObject.SetActive(false);
        }
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
    }

    public void OnLoadClick()
    {
        Controls.Load();

        KeyButton[] keyButtons = GetComponentsInChildren<KeyButton>();

        foreach (KeyButton keyButton in keyButtons)
        {
            keyButton.updateText();
        }
    }
}
