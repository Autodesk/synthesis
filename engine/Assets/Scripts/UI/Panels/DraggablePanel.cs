using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggablePanel : MonoBehaviour, IDragHandler, IBeginDragHandler {
    private Vector3 lastRectPosition;
    private Vector3 offset;

    public RectTransform parent;
    public RectTransform draggableArea;

    private void Start() {
        draggableArea = GameObject.Find("Movable-Panels").GetComponent<RectTransform>();
    }

    /// <summary>
    /// This method will be called on the start of the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnBeginDrag(PointerEventData eventData) {
        lastRectPosition             = parent.position;
        Vector3 currentMousePosition = eventData.position;
        offset                       = currentMousePosition - parent.position;
    }

    /// <summary>
    /// This method will be called during the mouse drag
    /// </summary>
    /// <param name="eventData">mouse pointer event data</param>
    public void OnDrag(PointerEventData eventData) {
        Vector3 currentMousePosition = eventData.position;

        lastRectPosition = currentMousePosition - offset;

        float h  = parent.rect.height / 2;
        float dh = draggableArea.rect.height;
        if (lastRectPosition.y > dh - h) {
            lastRectPosition = new Vector3(lastRectPosition.x, dh - h);
        } else if (lastRectPosition.y < h) {
            lastRectPosition = new Vector3(lastRectPosition.x, h);
        }

        float w  = parent.rect.width / 2;
        float dw = draggableArea.rect.width;

        if (lastRectPosition.x > dw - w) {
            lastRectPosition = new Vector3(dw - w, lastRectPosition.y);
        } else if (lastRectPosition.x < w) {
            lastRectPosition = new Vector3(w, lastRectPosition.y);
        }

        parent.position = lastRectPosition;
    }
}
