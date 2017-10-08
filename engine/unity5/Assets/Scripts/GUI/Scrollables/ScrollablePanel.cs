using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is a class used to create a scrollable panel with selectable items. It is designed so that one can make derived classes for modular use.
/// </summary>
public class ScrollablePanel : MonoBehaviour {

    private RectTransform rectTransform;
    private Vector2 scrollPosition;

    protected GameObject canvas;

    protected GUIStyle listStyle;
    protected GUIStyle highlightStyle;

    protected bool toScale = true;

    protected List<string> items;
    protected string errorMessage;

    protected Vector3 position;

    public string selectedEntry { get; set; } //this is set to whatever item is currently being selected

    // Use this for initialization
    protected virtual void Start () {
        canvas = GameObject.Find("Canvas");
        //Universal style for all scrollable panels
        listStyle = new GUIStyle("button");
        listStyle.normal.background = new Texture2D(0, 0);
        listStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        listStyle.active.background = Resources.Load("images/highlightsquaretexture") as Texture2D;
        listStyle.alignment = TextAnchor.MiddleLeft;
        listStyle.normal.textColor = Color.white;

        highlightStyle = new GUIStyle(listStyle);
        highlightStyle.normal.background = listStyle.active.background;
        highlightStyle.hover.background = highlightStyle.normal.background;
    }
	
	// Update is called once per frame
	protected virtual void OnGUI () {

        //Uses canvas scale and current rectangle transform data to create a new rectangle that the panel will occupy
        float scale = canvas.GetComponent<Canvas>().scaleFactor;

        Rect rect = GetComponent<RectTransform>().rect;
        Rect area = new Rect(position.x, Screen.height - position.y, rect.width * scale, rect.height * scale);

        if (toScale)
        {
            listStyle.fontSize = Mathf.RoundToInt(16 * scale);
            highlightStyle.fontSize = Mathf.RoundToInt(20 * scale);
        }

        //Sets up the new rectangle area for drawing UI components
        GUILayout.BeginArea(new Rect(area.x, area.y * 1.01f, area.width, rect.height * scale * .95f));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        //if there are items to generate a list from, create a scrollable list and update selectedEntry to whatever the user selects
        if (items.Count > 0)
        {
            listStyle.fixedWidth = area.width - GUI.skin.verticalScrollbar.fixedWidth * 2;

            string entry = SelectableList(items, selectedEntry, listStyle, highlightStyle) as string;
            if (entry != null)
            {
                selectedEntry = entry;
            }
            DynamicCamera.MovingEnabled = false;
        }
        else
        {
            GUILayout.Label(errorMessage, listStyle);
            selectedEntry = null;
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    /// <summary>
    /// Creates a scrollable list with selectable items
    /// </summary>
    /// <returns></returns>
    private object SelectableList<T>(IEnumerable<T> items, string highlight, GUIStyle buttonStyle, GUIStyle highlightStyle)
    {
        object selected = null;
        foreach (T o in items)
        {
            string entry = o.ToString();
            if (highlight != null && highlight.Equals(entry))
            {
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
