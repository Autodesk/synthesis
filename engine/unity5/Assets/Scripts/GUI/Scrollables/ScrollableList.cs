using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Synthesis.GUI.Scrollables
{
    public class ScrollableList : MonoBehaviour
    {
        public string listType;

        private RectTransform rectTransform;
        private Vector2 scrollPosition;

        private List<string> items;
        public string selectedEntry { get; set; }

        private GameObject mainMenuCanvas;
        private MainMenu mainMenu;

        private GUIStyle listStyle;
        private GUIStyle highlightStyle;

        private Color ListTextColor { get; set; }

        // Use this for initialization
        void Start()
        {
            mainMenuCanvas = GameObject.Find("Canvas");
            mainMenu = mainMenuCanvas.GetComponent<MainMenu>();

            listStyle = new GUIStyle("button");
            listStyle.normal.background = new Texture2D(0, 0);
            listStyle.active.background = Resources.Load("images/New Textures/greenButton") as Texture2D;
            listStyle.font = Resources.Load("Fonts/Artifakt Element Regular") as Font;
            listStyle.active.textColor = Color.white;
            listStyle.alignment = TextAnchor.MiddleLeft;

            if (ListTextColor == Color.clear) ListTextColor = Color.black;

            highlightStyle = new GUIStyle(listStyle);
            highlightStyle.normal.background = listStyle.active.background;
            highlightStyle.hover.background = highlightStyle.normal.background;
            highlightStyle.hover.textColor = Color.white;
        }

        void OnEnable()
        {
            items = new List<string>();
            items.Clear();
        }

        // Update is called once per frame
        void OnGUI()
        {
            if (listType.Equals("Replays") && items.Count == 0)
            {
                string[] files = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Autodesk\Synthesis\Replays\", "*.replay");

                foreach (string file in files)
                    items.Add(new FileInfo(file).Name.Split('.')[0]);

                if (items.Count > 0)
                    selectedEntry = items[0];
            }

            Vector3 p = UnityEngine.Camera.main.WorldToScreenPoint(transform.position);

            float scale = GameObject.Find("Canvas").GetComponent<Canvas>().scaleFactor;
            Rect rect = GetComponent<RectTransform>().rect;
            listStyle.fontSize = Mathf.RoundToInt(24 * scale);
            highlightStyle.fontSize = Mathf.RoundToInt(24 * scale);
            Rect position = new Rect(p.x, Screen.height - p.y, rect.width * scale, rect.height * scale);

            listStyle.normal.textColor = ListTextColor;
            listStyle.hover.textColor = ListTextColor;
            listStyle.hover.background = Resources.Load("Images/New Textures/Synthesis_an_Autodesk_Technology_2019_lockup_OL_stacked_no_year") as Texture2D;

            UnityEngine.GUI.skin.verticalScrollbar.normal.background = null;
            UnityEngine.GUI.skin.verticalScrollbarThumb.normal.background = Resources.Load("Images/New Textures/Synthesis_an_Autodesk_Technology_2019_lockup_OL_stacked_no_year") as Texture2D;

            GUILayout.BeginArea(new Rect(position.x, position.y * 1.01f, position.width, rect.height * scale * .95f));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (items.Count > 0)
            {
                listStyle.fixedWidth = position.width - UnityEngine.GUI.skin.verticalScrollbar.fixedWidth * 2;

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
}
