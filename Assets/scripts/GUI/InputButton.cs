using UnityEngine;
using System.Collections;

public class InputButton : MonoBehaviour
{
    public static bool isEditing;

    public int controlKey;

    private string buttonContent;
    private bool active;
    private bool tick;

    private Texture2D buttonTexture;
    private Texture2D buttonSelected;
    private Font gravityRegular;

    // Use this for initialization
    void Start()
    {
        //Custom style for buttons
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Russo_One") as Font;
    }

    // Update is called once per frame
    void OnGUI()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = gravityRegular;
        buttonStyle.fontSize = 12;
        if (active)
        {
            buttonStyle.normal.textColor = Color.cyan;
            buttonStyle.hover.textColor = Color.cyan;
        }
        else
        {
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = Color.white;
        }
        buttonStyle.normal.background = buttonTexture;
        buttonStyle.hover.background = buttonSelected;
        buttonStyle.active.background = buttonSelected;
        buttonStyle.onNormal.background = buttonSelected;
        buttonStyle.onHover.background = buttonSelected;
        buttonStyle.onActive.background = buttonSelected;

        tick = !tick;
        buttonContent = Controls.ControlKey[controlKey].ToString();

        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);
        Rect rect = GetComponent<RectTransform>().rect;
        if (GUI.Button(new Rect(p.x - rect.width / 2, Screen.height - p.y - rect.height / 2, rect.width, rect.height), buttonContent, buttonStyle) && (!isEditing))
        {
            isEditing = true;
            active = true;
        }

        if (active)
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(vKey) && tick)
                {
                    if (!Controls.SetControl(controlKey, vKey)) UserMessageManager.Dispatch("Conflicts with another input!", 2f);
                    active = false;
                    isEditing = false;
                }
            }
        }
    }
}