using UnityEngine;
using System.Collections;

namespace Synthesis.GUI
{
    public class PopupButton : MonoBehaviour
    {
        public GUIContent[] list;
        public string setting;

        private bool showList = false;
        private int listEntry = 0;
        private GUIStyle listStyle;
        private GUIStyle buttonStyle;
        private GUIStyle boxStyle;

        private Texture2D buttonBackground;
        private Texture2D selectedBackground;

        private Canvas canvas;

        private bool Fullscreen
        {
            get
            {
                return PlayerPrefs.GetInt("fullscreen") != 0;
            }
            set
            {
                PlayerPrefs.SetInt("fullscreen", value ? 1 : 0);
            }
        }

        private int ResolutionSetting
        {
            get
            {
                return PlayerPrefs.GetInt("resolution");
            }
            set
            {
                PlayerPrefs.SetInt("resolution", value);
            }
        }

        // Use this for initialization
        void Start()
        {
            UnityEngine.GUI.depth = 0;

            listStyle = new GUIStyle();
            listStyle.font = Resources.Load("Fonts/Artifakt Element Regular") as Font;
            listStyle.normal.textColor = Color.black;
            var tex = new Texture2D(2, 2);
            var colors = new Color[4];
            for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            listStyle.hover.background = tex;
            listStyle.onHover.background = tex;
            listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

            buttonStyle = new GUIStyle("button");
            buttonBackground = Resources.Load("Images/New Textures/highlightGray") as Texture2D;
            buttonStyle.normal.background = buttonBackground;
            buttonStyle.font = Resources.Load("Fonts/Artifakt Element Bold") as Font;
            buttonStyle.normal.textColor = Color.black;
            buttonStyle.alignment = TextAnchor.MiddleCenter;

            boxStyle = new GUIStyle("box");
            boxStyle.normal.background = buttonBackground;

            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }

        void OnGUI()
        {

            if (setting.Equals("Screen Mode"))
                if (!Fullscreen) listEntry = 0;
                else listEntry = 1;
            else if (setting.Equals("Resolution"))
                listEntry = ResolutionSetting;

            float scale = canvas.scaleFactor;

            buttonStyle.fontSize = Mathf.RoundToInt(30 * scale);
            Vector3 p = UnityEngine.Camera.main.WorldToScreenPoint(transform.position);
            Rect rect = GetComponent<RectTransform>().rect;
            if (Popup.List(new Rect(p.x - rect.width / 2 * scale, Screen.height - p.y - rect.height / 2 * scale, rect.width * scale, rect.height * scale), ref showList, ref listEntry, list[listEntry], list, buttonStyle, boxStyle, listStyle))
            {
                //This was a quick way to implement resolution settings. It might be 'bad code', but it works for the two settings we need.
                if (setting.Equals("Screen Mode"))
                {
                    if (listEntry == 0) Fullscreen = false;
                    else Fullscreen = true;
                }
                else if (setting.Equals("Resolution"))
                {
                    ResolutionSetting = listEntry;
                }
            }
        }
    }
}