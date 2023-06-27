using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.ContextMenus;

using ContextMenu = Synthesis.UI.ContextMenus.ContextMenu;

public class MouseTracker : MonoBehaviour
{
    public RectTransform tracker;

    // Update is called once per frame
    void Update() {
        var refResolution = ContextMenu.CanvasScaler.referenceResolution;

        // Vector2 screen = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        Vector2 mouse = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        // Vector2 adjustedPos = new Vector2(refResolution.x * (mouse.x / screen.x), refResolution.y * (mouse.y /
        // screen.y));

        // tracker.position = adjustedPos;
        tracker.position = mouse;
    }
}
