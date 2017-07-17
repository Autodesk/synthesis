using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


public class ScrollableList : MonoBehaviour {

    public string listType;

    private RectTransform rectTransform;
    private Vector2 scrollPosition;

    private List<string> items;
    public string selectedEntry { get; set; }

    private GameObject mainMenuCanvas;
    private MainMenu mainMenu;

    private GUIStyle listStyle;
    private GUIStyle highlightStyle;

	// Use this for initialization
	void Start () {
        mainMenuCanvas = GameObject.Find("MainMenuCanvas");
        mainMenu = mainMenuCanvas.GetComponent<MainMenu>();

        listStyle = new GUIStyle("button");
        listStyle.normal.background = new Texture2D(0,0);
        listStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        listStyle.active.background = Resources.Load("images/highlightsquaretexture") as Texture2D;
        listStyle.alignment = TextAnchor.MiddleLeft;
        listStyle.normal.textColor = Color.white;

        highlightStyle = new GUIStyle(listStyle);
        highlightStyle.normal.background = listStyle.active.background;
        highlightStyle.hover.background = highlightStyle.normal.background;
    }

    void OnEnable()
    {
        items = new List<string>();
        items.Clear();
    }
	
	// Update is called once per frame
	void OnGUI () {
        if (listType.Equals("Fields") && mainMenu.fieldDirectory != null && items.Count == 0)
        {
            string[] folders = System.IO.Directory.GetDirectories(mainMenu.fieldDirectory);
            foreach (string field in folders)
            {
                if (File.Exists(field + "\\definition.bxdf")) items.Add(new DirectoryInfo(field).Name);
            }
            if (items.Count > 0) selectedEntry = items[0];
        }

       else if (listType.Equals("Robots") && mainMenu.robotDirectory != null && items.Count == 0)
        {
            string[] folders = System.IO.Directory.GetDirectories(mainMenu.robotDirectory);
            foreach (string robot in folders)
            {
                if (File.Exists(robot + "\\skeleton.bxdj")) items.Add(new DirectoryInfo(robot).Name);
            }
            if (items.Count > 0) selectedEntry = items[0];
        }

        else if (listType.Equals("DPMFields") && mainMenu.fieldDirectory != null && items.Count == 0)
        {
            string[] folders = System.IO.Directory.GetDirectories(mainMenu.fieldDirectory);
            foreach (string field in folders)
            {
                if (File.Exists(field + "\\definition.bxdf") && File.Exists(field + "\\driverpracticemode.txt")) items.Add(new DirectoryInfo(field).Name);
            }
            if (items.Count > 0) selectedEntry = items[0];
        }

        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);

        float scale = GameObject.Find("MainMenuCanvas").GetComponent<Canvas>().scaleFactor;
        Rect rect = GetComponent<RectTransform>().rect;
        listStyle.fontSize = Mathf.RoundToInt(16 * scale);
        highlightStyle.fontSize = Mathf.RoundToInt(20 * scale);
        Rect position = new Rect(p.x, Screen.height - p.y, rect.width * scale, rect.height * scale);

        GUILayout.BeginArea(new Rect(position.x, position.y * 1.01f, position.width, rect.height * scale * .95f));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (items.Count > 0)
        {
            listStyle.fixedWidth = position.width - GUI.skin.verticalScrollbar.fixedWidth * 2;

            string entry = SelectList(items, selectedEntry, listStyle) as string;
            if (entry != null)
            {
                selectedEntry = entry;
            }
        }
        else
        {
            if (listType.Equals("Fields")) GUILayout.Label("No fields found in directory!", listStyle);
            else if (listType.Equals("Robots")) GUILayout.Label("No robots found in directory!", listStyle);
            selectedEntry = null;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private object SelectList<T>(IEnumerable<T> items, string highlight, GUIStyle buttonStyle)
    {
        object selected = null;
        foreach (T o in items)
        {
            string entry = o.ToString();
            if (highlight != null && highlight.Equals(entry))
            {
                Debug.Log(entry);
                if (GUILayout.Button(entry, highlightStyle))
                {
                    selected = o;
                }
            }
            else if (GUILayout.Button(entry, buttonStyle))
            {
                selected = o;
            }
        }
        return selected;
    }
}
