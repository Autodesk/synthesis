using UnityEngine;
using UnityEngine.UI;



public class InputControlDemo_KeyButtonScript : MonoBehaviour
{
    public static InputControlDemo_KeyButtonScript selectedButton      = null;
    public static bool                             ignoreMouseMovement = true;
    public static bool                             useKeyModifiers     = false;



    public KeyMapping keyMapping;
    public int        keyIndex;

    private Text      mKeyText;



    // Use this for initialization
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
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
                    setInput(new KeyboardInput());
                }
                else
                {
                    setInput(currentInput);
                }
            }
        }
    }

    public void updateText()
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
            case 2:
                mKeyText.text = keyMapping.thirdInput.ToString();
            break;
        }
    }

    public void OnClick()
    {
        if (selectedButton != null)
        {
            selectedButton.updateText();
        }

        selectedButton = this;

        if (mKeyText == null)
        {
            mKeyText = GetComponentInChildren<Text>();
        }

        mKeyText.text = "...";
    }

    private void setInput(CustomInput input)
    {
        switch (keyIndex)
        {
            case 0:
                keyMapping.primaryInput = input;
            break;
            case 1:
                keyMapping.secondaryInput = input;
            break;
            case 2:
                keyMapping.thirdInput = input;
            break;
        }

        updateText();

        selectedButton = null;
    }
}
