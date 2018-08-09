using UnityEngine;
using UnityEngine.UI;

namespace Crosstales.UI
{
    /// <summary>Change the state of all Window panels.</summary>
    public class UIWindowManager : MonoBehaviour
    {
        #region Variables

        /// <summary>All Windows of the scene.</summary>
        [Tooltip("All Windows of the scene.")]
        public GameObject[] Windows;

        private Image image;
        private GameObject DontTouch;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            foreach (GameObject window in Windows)
            {
                image = window.transform.Find("Panel/Header").GetComponent<Image>();
                Color c = image.color;
                c.a = 0.2f;
                image.color = c;
            }
        }

        #endregion


        #region Public methods

        public void ChangeState(GameObject x)
        {
            foreach (GameObject window in Windows)
            {
                if (window != x)
                {
                    image = window.transform.Find("Panel/Header").GetComponent<Image>();
                    Color c = image.color;
                    c.a = 0.2f;
                    image.color = c;
                }

                DontTouch = window.transform.Find("Panel/DontTouch").gameObject;
                DontTouch.SetActive(window != x);
            }
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)