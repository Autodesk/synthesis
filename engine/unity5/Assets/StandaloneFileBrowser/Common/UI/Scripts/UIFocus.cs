using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI
{
    /// <summary>Change the Focus on from a Window.</summary>
    public class UIFocus : MonoBehaviour
    {
        #region Variables

        public string CanvasName = "Canvas";

        private UIWindowManager manager;
        private Image image;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            manager = GameObject.Find(CanvasName).GetComponent<UIWindowManager>();

            image = transform.Find("Panel/Header").GetComponent<Image>();
        }

        #endregion


        #region Public methods

        public void OnPanelEnter()
        {
            manager.ChangeState(gameObject);

            Color c = image.color;
            c.a = 255;
            image.color = c;

            transform.SetAsLastSibling(); //move to the front (on parent)
            transform.SetAsFirstSibling(); //move to the back (on parent)
            transform.SetSiblingIndex(-1); //move to position, whereas 0 is the backmost, transform.parent.childCount -1 is the frontmost position 
            transform.GetSiblingIndex(); //get the position in the hierarchy (on parent)
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)