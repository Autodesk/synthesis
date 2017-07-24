using UnityEngine;



public class InputControlDemo_SettingsModeScript : MonoBehaviour
{
    public GameObject                    gameMode;
    public InputControlDemo_PlayerScript playerScript;



    // Update is called once per frame
    void Update()
    {
        if (
            InputControlDemo_KeyButtonScript.selectedButton == null
            &&
            InputControl.GetKeyDown(KeyCode.Escape)
           )
        {
            gameMode.SetActive(true);
            gameObject.SetActive(false);

            playerScript.enabled = true;
        }
    }

    public void OnIgnoreMouseMovementChanged(bool on)
    {
        InputControlDemo_KeyButtonScript.ignoreMouseMovement = on;
    }

    public void OnUseKeyModifiersChanged(bool on)
    {
        InputControlDemo_KeyButtonScript.useKeyModifiers = on;
    }

    public void OnSaveClick()
    {
        Controls.save();
    }

    public void OnLoadClick()
    {
        Controls.load();

        InputControlDemo_KeyButtonScript[] keyButtons = GetComponentsInChildren<InputControlDemo_KeyButtonScript>();

        foreach(InputControlDemo_KeyButtonScript keyButton in keyButtons)
        {
            keyButton.updateText();
        }
    }
}
