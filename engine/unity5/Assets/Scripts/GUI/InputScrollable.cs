using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


public class InputScrollable : MonoBehaviour
{

    public string listType;

    private RectTransform rectTransform;
    private Vector2 scrollPosition;

    private List<string> inputNames;
    private List<KeyCode> inputKeys;
    public string selectedEntry { get; set; }

    private GUIStyle nameStyle;
    private GUIStyle keyStyle;
    private GUIStyle keyHighlightStyle;

    // Use this for initialization
    void Start()
    {
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

        inputNames = new List<string>();
        inputKeys = new List<KeyCode>();

        for (int i = 0; i < Controls.ControlName.Length; i++)
        {
            inputNames.Add(Controls.ControlName[i]);
        }

    }

    // Update is called once per frame
    void OnGUI()
    {
        if (inputKeys.Count == 0) UpdateControlList();
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


        string entry = SelectList(inputNames, inputKeys, selectedEntry) as string;
        if (entry != null)
        {
            selectedEntry = entry;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();

    }

    void Update()
    {
        if (selectedEntry != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                selectedEntry = null;
                return;        
            }
            foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(vKey))
                {
                    Controls.SetControl(inputNames.IndexOf(selectedEntry), vKey);
                    Controls.SaveControls();
                    selectedEntry = null;

                    if (Controls.CheckConflict()) UserMessageManager.Dispatch("There is a conflict in the input settings!", 5);

                    UpdateControlList();
                }
            }
        }
    }

    public void UpdateControlList()
    {
        inputKeys.Clear();
        for (int i = 0; i < Controls.ControlName.Length; i++)
        {
            inputKeys.Add(Controls.ControlKey[i]);
        }
    }

    private object SelectList(List<string> names, List<KeyCode> keys, string highlight)
    {
        object selected = null;
        int counter = 0;
        foreach (string o in names)
        {
            string entry = o.ToString();
            GUILayout.BeginHorizontal();
            GUILayout.Label(entry, nameStyle);
            if (highlight != null && highlight.Equals(entry))
            {
                if (GUILayout.Button(keys[counter].ToString(), keyHighlightStyle))
                {
                    selected = o;
                }
            }
            else
            {
                if (GUILayout.Button(keys[counter].ToString(), keyStyle))
                {
                    selected = o;
                }
            }
            GUILayout.EndHorizontal();
            counter++;
        }
        return selected;
    }
}
