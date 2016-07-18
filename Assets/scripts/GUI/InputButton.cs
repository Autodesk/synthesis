using UnityEngine;
using System.Collections;

public class InputButton : MonoBehaviour
{
    public static bool isEditing;

    public int controlKey;

    private string buttonContent;
    private bool active;
    private bool tick;

    private GUIStyle buttonStyle;

    // Use this for initialization
    void Start()
    {
        //Custom style for buttons
        buttonStyle = new GUIStyle();
        buttonStyle = MainMenu.buttonStyle;
    }

    // Update is called once per frame
    void OnGUI()
    {
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