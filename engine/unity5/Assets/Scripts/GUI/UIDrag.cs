using UnityEngine;
using System.Collections;

public class UIDrag : MonoBehaviour
{
    private float offsetX;
    private float offsetY;

    public void BeginDrag()
    {
        offsetX = transform.parent.position.x - Input.mousePosition.x;
        offsetY = transform.parent.position.y - Input.mousePosition.y;

        DynamicCamera.MovingEnabled = false;
    }

    public void OnDrag()
    {
        Vector3 lastPos = transform.parent.position;
        transform.parent.position = new Vector3(offsetX + Input.mousePosition.x, offsetY + Input.mousePosition.y);

        Rect rect = GetComponent<RectTransform>().rect;

        if (transform.parent.position.x < -0 ||
            transform.parent.position.x > Screen.width  ||
            transform.parent.position.y < 0 ||
            transform.parent.position.y > Screen.width)
        {
            transform.parent.position = lastPos;
        }
    }

    public void EndDrag()
    {
        DynamicCamera.MovingEnabled = true;
    }
}