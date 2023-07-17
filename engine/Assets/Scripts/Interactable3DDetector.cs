using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Synthesis.UI;
using Synthesis.UI.ContextMenus;

using ContextMenu = Synthesis.UI.ContextMenus.ContextMenu;

public class Interactable3DDetector : MonoBehaviour {
    private static Interactable3DDetector instance = null;

    private void Start() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            Debug.Log("3d Detector");
            var refResolution = ContextMenu.CanvasScaler.referenceResolution;

            Vector2 screen = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            Vector2 mouse  = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 adjustedPos =
                new Vector2(refResolution.x * (mouse.x / screen.x), refResolution.y * (mouse.y / screen.y));

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(ray, out RaycastHit hit); // TODO: Fix
            if (hit.collider != null) {
                var component = hit.collider.gameObject.GetComponent<InteractableObject>();
                if (component != null) {
                    component.OnPointerClick(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                }
            }
        }
    }
}
