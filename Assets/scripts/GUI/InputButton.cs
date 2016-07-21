using UnityEngine;
using System.Collections;

/// <summary>
/// Attach this script/class to a panel to make it a button that can alter input settings.
/// </summary>
public class InputButton : MonoBehaviour
{
    public static bool isEditing; //Global variable that is true when an input is currently being changed in one button.

    public int controlKey; //The index of the control key. See Controls.cs to see what index is associated with what control.

    private string buttonContent; //The current control key.
    private bool active; //Whether this button is currently changing an input.

    private Texture2D buttonTexture; //The default texture of the button.
    private Texture2D buttonSelected; //The texture of the button when it is hovered over. 
    private Font russoOne; //The best font.

    // Use this for initialization
    void Start()
    {
        //Loads the resources to be used for the button style.
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
    }

    //Renders the GUI
    void OnGUI()
    {
        //Initializes the custom style for the button.
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = russoOne;
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

        //Sets the button content to the current control key.
        buttonContent = Controls.ControlKey[controlKey].ToString();

        //Converts the world coordinates of the game object to local screen coordinates.
        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);
        Rect rect = GetComponent<RectTransform>().rect;

        //Creates the button and sets the proper variables to active if it is clicked.
        if (GUI.Button(new Rect(p.x - rect.width / 2 - 0.5f, Screen.height - p.y - rect.height / 2 - 0.5f, rect.width - 0.5f, rect.height - 0.5f), buttonContent, buttonStyle) && (!isEditing))
        {
            isEditing = true;
            active = true;
        }

        //if the button was pressed, wait until the user presses a key.
        if (active)
        {
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(vKey))
                {
                    Controls.SetControl(controlKey, vKey);
                    active = false;
                    isEditing = false;
                    Controls.SaveControls();

                    if (Controls.CheckConflict()) MainMenu.InputConflict.SetActive(true);
                    else MainMenu.InputConflict.SetActive(false);
                }
            }
        }
    }
}