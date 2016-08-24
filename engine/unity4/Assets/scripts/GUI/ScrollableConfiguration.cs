using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


public class ScrollableConfiguration : MonoBehaviour
{

    public string listType;

    private RectTransform rectTransform;
    private Vector2 scrollPosition;

    private List<string> items;
    public string selectedEntry { get; set; }

    private RobotConfiguration robotConfig;

    private GUIStyle listStyle;
    private GUIStyle highlightStyle;

    // Use this for initialization
    void Start()
    {
        robotConfig = GameObject.Find("RobotConfiguration").GetComponent<RobotConfiguration>();

        listStyle = new GUIStyle("button");
        listStyle.normal.background = new Texture2D(0, 0);
        listStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        listStyle.active.background = Resources.Load("images/highlightsquaretexture") as Texture2D;
        listStyle.alignment = TextAnchor.MiddleLeft;
        listStyle.normal.textColor = Color.white;

        highlightStyle = new GUIStyle(listStyle);
        highlightStyle.normal.background = listStyle.active.background;
        highlightStyle.hover.background = highlightStyle.normal.background;

        items = new List<string>();

    }

    // Update is called once per frame
    void OnGUI()
    {
        if (listType.Equals("Nodes") && robotConfig.dpmNodes != null && items.Count == 0)
        {
            items = robotConfig.dpmNodes;
            if (items.Count > 0) selectedEntry = items[0];
        }

        else if (listType.Equals("Vectors") && robotConfig.dpmVectors != null && items.Count == 0)
        {
            items = robotConfig.dpmVectors;
            if (items.Count > 0) selectedEntry = items[0];
        }

        else if (listType.Equals("NodePopup") && robotConfig.nodeList != null && items.Count == 0)
        {
            foreach (GameObject objects in robotConfig.nodeList)
            {
                items.Add(objects.name);
            }
            if (items.Count > 0) selectedEntry = robotConfig.nodeSave[robotConfig.editingIndex];
            robotConfig.HighlightNode();
        }

       
        float scale = GameObject.Find("Canvas").GetComponent<Canvas>().scaleFactor;
        Rect rect = GetComponent<RectTransform>().rect;
        Vector3 p = RectTransformUtility.WorldToScreenPoint(null, new Vector2(transform.position.x, transform.position.y));
        listStyle.fontSize = Mathf.RoundToInt(20 * scale);
        highlightStyle.fontSize = Mathf.RoundToInt(20 * scale);
        Rect position = new Rect(p.x, Screen.height - p.y, rect.width * scale, rect.height * scale);

        GUILayout.BeginArea(new Rect(position.x, position.y * 1.01f , position.width, rect.height * scale * .95f));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if (items.Count > 0)
        {
            string entry = SelectList(items, selectedEntry, listStyle) as string;
            if (entry != null)
            {
                if (listType.Equals("Nodes"))
                {
                    if (!robotConfig.editingVector && !robotConfig.editingNode)
                    {

                        for (int i = 0; i < items.Count; i++)
                        {
                            if (entry.Equals(items[i].ToString())) robotConfig.editingIndex = i;
                        }
                        robotConfig.OpenNodePopup();
                        selectedEntry = entry;
                        robotConfig.HighlightNode();
                    }
                }
                else if (listType.Equals("Vectors"))
                {
                    if (!robotConfig.editingNode && !robotConfig.editingVector)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (entry.Equals(items[i].ToString())) robotConfig.editingIndex = i;

                        }
                        robotConfig.OpenVectorPopup();
                        selectedEntry = entry;
                    }
                }
                else if (listType.Equals("NodePopup"))
                {
                    selectedEntry = entry;
                    robotConfig.HighlightNode();
                }
            }
        }
        else
        {
            if (listType.Equals("Nodes")) GUILayout.Label("No node information found from field configuration file! File has been modified.", listStyle);
            else if (listType.Equals("Vectors")) GUILayout.Label("No vector information found from field configuration file! File has been modified.", listStyle);
            selectedEntry = null;
        }
        if (listType.Equals("Nodes") && !robotConfig.editingNode) selectedEntry = "";
        else if (listType.Equals("Vectors") && !robotConfig.editingVector) selectedEntry = "";

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
