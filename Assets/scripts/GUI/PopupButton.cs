using UnityEngine;
using System.Collections;

public class PopupButton : MonoBehaviour {

    public GUIContent[] list; //List of the different options that can be selected.
    public string setting; //The setting that this popup button modifies.

    private bool showList; //Is true when the popup button is active and is showing the list.
    private int listEntry = 0; //The selected list entry index.
    private GUIStyle listStyle; //The style of the list.
    private GUIStyle buttonStyle; //The style of the popup button.
    private GUIStyle boxStyle; //The style of the box containing the list.

    private Texture2D buttonBackground; //The button background.

	// Initalizes all the GUIStyles for use.
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
        //Updates the list entries with the current setting (so that when the user resets the scene, the selected option isn't reset to default)
        if (setting.Equals("Screen Mode"))
            if (!MainMenu.fullscreen) listEntry = 1;
            else listEntry = 0;
        else if (setting.Equals("Resolution"))
            listEntry = MainMenu.resolutionsetting; 

        //Converting world coordinates of the gameobject to position based on the camera.
        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);
        Rect rect = GetComponent<RectTransform>().rect;
        if (Popup.List(new Rect(p.x-rect.width/2, Screen.height-p.y-rect.height/2, rect.width, rect.height), ref showList, ref listEntry, list[listEntry], list, buttonStyle, boxStyle, listStyle))
        {
            //This is what makes the popup button control different variables. If there is need to add more popup buttons that control other settings,
            //just be sure to add the code in here.
            if (setting.Equals("Screen Mode"))
            {
                if (listEntry == 0) MainMenu.fullscreen = true;
                else MainMenu.fullscreen = false;
            }
            else if (setting.Equals("Resolution"))
            {
                MainMenu.resolutionsetting = listEntry;
            }
        }
    }
}
