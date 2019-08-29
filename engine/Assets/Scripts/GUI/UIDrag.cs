using UnityEngine;
using System.Collections;

namespace Synthesis.GUI
{
    public class UIDrag : MonoBehaviour
    {
        private float offsetX;
        private float offsetY;

        public void BeginDrag()
        {
            offsetX = transform.parent.position.x - UnityEngine.Input.mousePosition.x;
            offsetY = transform.parent.position.y - UnityEngine.Input.mousePosition.y;

            DynamicCamera.ControlEnabled = false;
        }

        public void OnDrag()
        {
            Vector3 lastPos = transform.parent.position;
            transform.parent.position = new Vector3(offsetX + UnityEngine.Input.mousePosition.x, offsetY + UnityEngine.Input.mousePosition.y);

            Rect rect = GetComponent<RectTransform>().rect;

            if (transform.parent.position.x < -0 ||
                transform.parent.position.x > Screen.width ||
                transform.parent.position.y < 0 ||
                transform.parent.position.y > Screen.width)
            {
                transform.parent.position = lastPos;
            }
        }

        public void EndDrag()
        {
            DynamicCamera.ControlEnabled = true;
        }
    }
}