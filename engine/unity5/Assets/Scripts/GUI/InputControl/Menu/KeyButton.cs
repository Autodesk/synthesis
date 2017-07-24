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

    private RectTransform rectTransform;
    private Vector2 scrollPosition;

    private GUIStyle nameStyle;
    private GUIStyle keyStyle;
    private GUIStyle keyHighlightStyle;

    // Use this for initialization
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);

        nameStyle = new GUIStyle("button");
        nameStyle.normal.background = new Texture2D(0, 0);
        nameStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        nameStyle.active.background = Resources.Load("images/highlightsquaretexture") as Texture2D;
        nameStyle.alignment = TextAnchor.MiddleLeft;
        nameStyle.normal.textColor = Color.white;

        keyStyle = new GUIStyle(nameStyle);
        keyStyle.alignment = TextAnchor.MiddleCenter;
        keyHighlightStyle = new GUIStyle(keyStyle);
        keyHighlightStyle.normal.background = nameStyle.active.background;
        keyHighlightStyle.hover.background = keyHighlightStyle.normal.background;
    }

    // Update is called once per frame
    void OnGUI()
    {
        Rect rect = GetComponent<RectTransform>().rect;
        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);

        float scale = GameObject.Find("MainMenuCanvas").GetComponent<Canvas>().scaleFactor;
        Rect position = new Rect(p.x, Screen.height - p.y, rect.width * scale, rect.height * scale);

        nameStyle.fontSize = Mathf.RoundToInt(20 * scale);
        keyStyle.fontSize = nameStyle.fontSize;
        keyHighlightStyle.fontSize = nameStyle.fontSize;

        nameStyle.fixedWidth = position.width * .7f;
        keyStyle.fixedWidth = position.width * .2f;
        keyHighlightStyle.fixedWidth = position.width * .2f;

        nameStyle.fixedHeight = 50 * scale;
        keyStyle.fixedHeight = nameStyle.fixedHeight;
        keyHighlightStyle.fixedHeight = nameStyle.fixedHeight;

        GUILayout.BeginArea(new Rect(position.x, position.y * 1.01f, position.width, rect.height * scale * .95f));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

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

        GUILayout.EndScrollView();
        GUILayout.EndArea();
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
            GUILayout.Label(mKeyText.text, nameStyle);
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
