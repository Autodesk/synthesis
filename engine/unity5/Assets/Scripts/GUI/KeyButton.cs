using UnityEngine;
using UnityEngine.UI;

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
        //Implement assets; (most assets are configured in Unity: OptionsTab > Canvas > SettingsMode
        mKeyText.font = Resources.Load("Fonts/Russo_One") as Font;
        mKeyText.color = Color.white;
        mKeyText.fontSize = 13;

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
                    ((KeyboardInput)currentInput).key == KeyCode.Escape
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
        Controls.Save();

        selectedButton = null;
    }
}