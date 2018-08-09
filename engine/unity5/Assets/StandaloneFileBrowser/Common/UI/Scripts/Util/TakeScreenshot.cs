#if !UNITY_WEBPLAYER && !UNITY_WSA && !UNITY_WEBGL
using UnityEngine;
using System;

namespace Crosstales.UI.Util
{
    /// <summary>Take a screen shot of the application.</summary>
    public class TakeScreenshot : MonoBehaviour
    {

        #region Variables

        public string Prefix = "CT_Screenshot";
        public int Scale = 1;
        public KeyCode KeyCode = KeyCode.F8;

        private Texture2D texture;

        #endregion


        #region MonoBehaviour methods

        public void Start()
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                string file = Prefix + DateTime.Now.ToString("_d-MM-yyyy-HH-mm-ss-f") + ".png";

                ScreenCapture.CaptureScreenshot(file, Scale);
                
                Debug.Log("Screenshot saved: " + file);
            }
        }

        #endregion

    }
}
#endif
// © 2014-2018 crosstales LLC (https://www.crosstales.com)