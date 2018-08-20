using UnityEditor;
using UnityEngine;

namespace Crosstales.FB.EditorExt
{
    /// <summary>Sets the required .NET API level.</summary>
    [InitializeOnLoad]
    public static class SetApiLevel
    {

        #region Constructor

        static SetApiLevel()
        {
#if UNITY_STANDALONE_WIN

#if UNITY_2017 || UNITY_2018

            string apiLevel = PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone).ToString();
            
            //TODO add new entries from Unity 2018
            if (!apiLevel.Equals("NET_4_6") && PlayerSettings.GetApiCompatibilityLevel(BuildTargetGroup.Standalone) != ApiCompatibilityLevel.NET_2_0) {
                
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_2_0);
#else
            if (PlayerSettings.apiCompatibilityLevel != ApiCompatibilityLevel.NET_2_0)
            {
                PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
#endif
                Debug.Log("File Browser: API level changed to .NET 2.0.");
            }

#endif
        }

        #endregion
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)