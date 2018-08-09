﻿using UnityEngine;
using UnityEditor;

namespace Crosstales.FB.EditorTask
{
    /// <summary>Loads the configuration at startup.</summary>
    [InitializeOnLoad]
    public static class ConfigLoader
    {

        #region Constructor

        static ConfigLoader()
        {
            if (!Util.Config.isLoaded) {
                Util.Config.Load();

                if (Util.Config.DEBUG)
                    Debug.Log("Config data loaded");
            }
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)