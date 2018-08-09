using UnityEngine;
using UnityEditor;

namespace Crosstales.FB.EditorTask
{
    /// <summary>Installs the 'FileBrowser'-package.</summary>
    [InitializeOnLoad]
    public static class ZInstaller
    {
        #region Constructor

        static ZInstaller()
        {
#if !fb_ignore_setup
            string path = Application.dataPath + "/Plugins/crosstales/FileBrowser/FileBrowser.unitypackage";
            //string path = Application.dataPath;
            //string subpath = "/Plugins/crosstales/FileBrowser/FileBrowser.unitypackage";
            
            //Debug.Log(path + subpath);
            
            try
            {
                if (System.IO.File.Exists(path))
                {
                    AssetDatabase.ImportPackage(path, false);

                    System.IO.File.Delete(path);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Could not import the 'FileBrowser'-package: " + ex);
            }
#endif
        }

        #endregion

    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)