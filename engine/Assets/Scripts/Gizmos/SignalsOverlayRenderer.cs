using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalsOverlayRenderer : MonoBehaviour {
    public SignalsOverlayRenderer Instance { get; private set; }

    public Font GuiFont;

    public void Awake() {
        Instance = this;
    }

    public void OnGUI() {

        var style = new GUIStyle() {
            alignment = TextAnchor.MiddleCenter,
            font = GuiFont,
            fontSize = 36,
            normal = new GUIStyleState() { textColor = Color.white },
        };

        GUI.contentColor = Color.white;

        GUI.Label(new Rect(100f, 100f, 50f, 50f), "0", style);
    }
}
