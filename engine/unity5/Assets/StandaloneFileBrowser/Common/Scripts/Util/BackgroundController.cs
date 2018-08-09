using UnityEngine;

namespace Crosstales.Common.Util
{
    /// <summary>Enables or disable game objects on Android or iOS in the background.</summary>
    //[HelpURL("https://www.crosstales.com/media/data/assets/radio/api/class_crosstales_1_1_radio_1_1_demo_1_1_util_1_1_platform_controller.html")] 
    public class BackgroundController : MonoBehaviour
    {

        #region Variables

        ///<summary>Selected objects for the controller.</summary>
        [Tooltip("Selected objects for the controller.")]
        public GameObject[] Objects;

        private bool isFocused;

        #endregion


        #region MonoBehaviour methods

#if UNITY_2017 || UNITY_2018
#if UNITY_ANDROID || UNITY_IOS
        public void Start()
        {
            isFocused = Application.isFocused;
        }

        public void FixedUpdate()
        {
            if (Application.isFocused != isFocused)
            {
                isFocused = Application.isFocused;

                if ((BaseHelper.isAndroidPlatform || BaseHelper.isIOSPlatform) && !TouchScreenKeyboard.visible)
                {
                    foreach (GameObject go in Objects)
                    {
                        if (go != null)
                        {
                            go.SetActive(isFocused);
                        }
                    }

                    Debug.Log("Application.isFocused: " + isFocused);
                }
            }
        }
#endif
#else
        public void Start()
        {
            Debug.LogWarning("'BackgroundController' needs Unity 2017 or newer to work!");
        }

#endif

        #endregion

    }
}
// © 2018 crosstales LLC (https://www.crosstales.com)