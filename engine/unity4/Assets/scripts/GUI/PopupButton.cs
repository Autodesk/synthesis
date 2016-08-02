using UnityEngine;
using System.Collections;

public class PopupButton : MonoBehaviour {

    public GUIContent[] list;
    public string setting;

    private bool showList;
    private int listEntry = 0;
    private GUIStyle listStyle;
    private GUIStyle buttonStyle;
    private GUIStyle boxStyle;

    private Texture2D buttonBackground;
    private Texture2D selectedBackground;

	// Use this for initialization
	void Start () {
        listStyle = new GUIStyle();
        listStyle.normal.textColor = Color.white;
        var tex = new Texture2D(2, 2);
        var colors = new Color[4];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        listStyle.hover.background = tex;
        listStyle.onHover.background = tex;
        listStyle.padding.left = listStyle.padding.right = listStyle.padding.top = listStyle.padding.bottom = 4;

        buttonStyle = new GUIStyle("button");
        buttonBackground = Resources.Load("Images/normalbuttontexture") as Texture2D;
        buttonStyle.normal.background = buttonBackground;
        buttonStyle.font = Resources.Load("Fonts/Russo_One") as Font;
        buttonStyle.normal.textColor = Color.white;
        buttonStyle.alignment = TextAnchor.MiddleCenter;

        boxStyle = new GUIStyle("box");
        boxStyle.normal.background = buttonBackground;
    }

	void OnGUI () {
        if (setting.Equals("Screen Mode"))
            if (!MainMenu.fullscreen) listEntry = 0;
            else listEntry = 1;
        else if (setting.Equals("Resolution"))
            listEntry = MainMenu.resolutionsetting; 

        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);
        Rect rect = GetComponent<RectTransform>().rect;
        if (Popup.List(new Rect(p.x-rect.width/2, Screen.height-p.y-rect.height/2, rect.width, rect.height), ref showList, ref listEntry, list[listEntry], list, buttonStyle, boxStyle, listStyle))
        {
            //This was a quick way to implement resolution settings. It might be 'bad code', but it works for the two settings we need.
            if (setting.Equals("Screen Mode"))
            {
                if (listEntry == 0) MainMenu.fullscreen = false;
                else MainMenu.fullscreen = true;
            }
            else if (setting.Equals("Resolution"))
            {
                MainMenu.resolutionsetting = listEntry;
            }
        }
    }
}
