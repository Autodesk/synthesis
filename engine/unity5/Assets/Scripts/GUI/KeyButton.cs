using UnityEngine;
using UnityEngine.UI;

//Adapted from: https://github.com/Gris87/InputControl

public class KeyButton : MonoBehaviour
{
    public static KeyButton selectedButton = null;
    public static bool ignoreMouseMovement = true;
    public static bool useKeyModifiers = false;

    public KeyMapping keyMapping;
    public int keyIndex;

    private Text mKeyText;

    // Use this for initialization
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void OnGUI()
    {
        //Implements styles; (most assets/styles are configured in Unity: OptionsTab > Canvas > SettingsMode > SettingsPanel
        mKeyText.font = Resources.Load("Fonts/Russo_One") as Font;
        mKeyText.color = Color.white;
        mKeyText.fontSize = 13;

        //Checks if the currentInput uses the ignoreMouseMovement or useKeyModifiers
        //Currently DISABLED (hidden in the Unity menu) due to inconsistent toggle to key updates 8/2017
        if (selectedButton == this)
        {
            CustomInput currentInput = InputControl.currentInput(ignoreMouseMovement, useKeyModifiers);

            if (currentInput != null)
            {
                if (
                    currentInput.modifiers == KeyModifier.NoModifier
                    &&
                    currentInput is KeyboardInput
                    &&
                    ((KeyboardInput)currentInput).key == KeyCode.Backspace
                   )
                {
                    SetInput(new KeyboardInput());
                }
                else
                {
                    SetInput(currentInput);
                }
            }
        }
    }

    /// <summary>
    /// Updates the KeyButtons' text.
    /// </summary>
    public void UpdateText()
    {
        if (mKeyText == null)
        {
            mKeyText = GetComponentInChildren<Text>();
        }

        switch (keyIndex)
        {

            case 0:
                mKeyText.text = keyMapping.primaryInput.ToString();
                break;
            case 1:
                mKeyText.text = keyMapping.secondaryInput.ToString();
                break;
        }
    }

    /// <summary>
    /// Updates the text when the user clicks the KeyButtons
    /// </summary>
    public void OnClick()
    {
        if (selectedButton != null)
        {
            selectedButton.UpdateText();
        }

        selectedButton = this;

        if (mKeyText == null)
        {
            mKeyText = GetComponentInChildren<Text>();
        }

        mKeyText.text = "...";
    }

    /// <summary>
    /// Sets the primary or secondary input to the selected input from the user.
    /// </summary>
    /// <param name="input">Input from any device or axis (e.g. Joysticks, Mouse, Keyboard)</param>
    private void SetInput(CustomInput input)
    {
        switch (keyIndex)
        {
            case 0:
                keyMapping.primaryInput = input;
                break;
            case 1:
                keyMapping.secondaryInput = input;
                break;
        }

        UpdateText();

        //Enable this for auto-saving. To complete auto-saving, enable the comments in SettingsMode.cs > OnLoadClick().
        Controls.Save();

        selectedButton = null;
    }
}