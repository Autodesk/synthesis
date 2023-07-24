using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.UI.ContextMenus;

using ContextMenu = Synthesis.UI.ContextMenus.ContextMenu;

public class MouseTracker : MonoBehaviour {
    public RectTransform tracker;

    private void Update() {
        var refResolution = ContextMenu.CanvasScaler.referenceResolution;
        Vector2 mouse     = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        tracker.position  = mouse;
    }
}
