using UnityEngine;

namespace Crosstales.UI
{
    /// <summary>Allow to Drag the Windows arround.</summary>
    public class UIDrag : MonoBehaviour
    {
        #region Variables

        private float offsetX;
        private float offsetY;

        #endregion


        #region Public methods

        public void BeginDrag()
        {
            offsetX = transform.position.x - Input.mousePosition.x;
            offsetY = transform.position.y - Input.mousePosition.y;
        }

        public void OnDrag()
        {
            transform.position = new Vector3(offsetX + Input.mousePosition.x, offsetY + Input.mousePosition.y);
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)