using UnityEngine;
using System.Collections;

public class PopupButton : MonoBehaviour {

    public GUIContent[] list;
    
    private bool showList;
    private int listEntry = 0;
    private GUIStyle listStyle;
    private bool picked = false;

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
    }
	
	// Update is called once per frame
	void OnGUI () {
        Vector3 p = Camera.main.WorldToScreenPoint(transform.position);
        Rect rect = GetComponent<RectTransform>().rect;
        Debug.Log (p.y);
        if (Popup.List(new Rect(p.x-rect.width/2, Screen.height-p.y-rect.height/2, rect.width, rect.height), ref showList, ref listEntry, list[listEntry], list, listStyle))
        {
            picked = true;
        }
    }
}
