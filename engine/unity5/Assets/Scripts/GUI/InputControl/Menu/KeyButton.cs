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

        //nameStyle = new GUIStyle("button");
        //nameStyle.normal.background = new Texture2D(0, 0);
        //nameStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        //nameStyle.active.background = Resources.Load("images/highlightsquaretexture") as Texture2D;
        //nameStyle.alignment = TextAnchor.MiddleLeft;
        //nameStyle.normal.textColor = Color.white;

        //keyStyle = new GUIStyle(nameStyle);
        //keyStyle.alignment = TextAnchor.MiddleCenter;
        //keyHighlightStyle = new GUIStyle(keyStyle);
        //keyHighlightStyle.normal.background = nameStyle.active.background;
        //keyHighlightStyle.hover.background = keyHighlightStyle.normal.background;

        //inputNames = new List<string>();
        //inputKeys = new List<KeyCode>();
    }

    // Update is called once per frame
    void OnGUI()
    {
        //Implement assets (most assets are configured in Unity: OptionsTab > Canvas > SettingsMode
        mKeyText.font = Resources.Load("Fonts/Russo_One") as Font;


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
                //case 2:
                //    mKeyText.text = keyMapping.thirdInput.ToString();
                //    break;
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
                //case 2:
                //    keyMapping.thirdInput = input;
                //    break;
        }

        updateText();

        selectedButton = null;
    }
}
