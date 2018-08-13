using System;

namespace Crosstales.FB
{
    /// <summary>Native file browser various actions like open file, open folder and save file.</summary>
    public class FileBrowser
    {
        #region Variables

        private static Wrapper.IFileBrowser platformWrapper;

        #endregion

        
        #region Constructor

        static FileBrowser()
        {
            if (Util.Helper.isEditor)
            {
                //Debug.Log("FileBrowserEditor");
#if UNITY_EDITOR
                platformWrapper = new Wrapper.FileBrowserEditor();
#endif
            }
            else if (Util.Helper.isMacOSPlatform)
            {
                //Debug.Log("FileBrowserMac");
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
                platformWrapper = new Wrapper.FileBrowserMac();
#endif
            }
            else if (Util.Helper.isWindowsPlatform)
            {
                //Debug.Log("FileBrowserWindows");
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                platformWrapper = new Wrapper.FileBrowserWindows();
#endif
            }
            else
            {
                //Debug.Log("FileBrowserGeneric");
                platformWrapper = new Wrapper.FileBrowserGeneric();
            }

            //Debug.Log(platformWrapper);
        }

        #endregion


        #region Public methods

        /// <summary>Open native file browser for a single file.</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <returns>Returns a string of the chosen file. Empty string when cancelled</returns>
        public static string OpenSingleFile(string title, string directory, string extension)
        {
            return OpenSingleFile(title, directory, getFilter(extension));
        }

        /// <summary>Open native file browser for a single file.</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <returns>Returns a string of the chosen file. Empty string when cancelled</returns>
        public static string OpenSingleFile(string title, string directory, ExtensionFilter[] extensions)
        {
            return platformWrapper.OpenSingleFile(title, directory, extensions);
        }

        /// <summary>Open native file browser for multiple files.</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen files. Zero length array when cancelled</returns>
        public static string[] OpenFiles(string title, string directory, string extension, bool multiselect)
        {
            return OpenFiles(title, directory, getFilter(extension), multiselect);
        }

        /// <summary>Open native file browser for multiple files.</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <returns>Returns array of chosen files. Zero length array when cancelled</returns>
        public static string[] OpenFiles(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            return platformWrapper.OpenFiles(title, directory, extensions, multiselect);
        }

        /// <summary>Open native folder browser for a single folder.</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <returns>Returns a string of the chosen folder. Empty string when cancelled</returns>
        public static string OpenSingleFolder(string title, string directory = "")
        {
            return platformWrapper.OpenSingleFolder(title, directory);
        }

        /// <summary>
        /// Open native folder browser for multiple folders.
        /// NOTE: Multiple folder selection isnt't supported on Windows!
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="multiselect">Allow multiple folder selection</param>
        /// <returns>Returns array of chosen folders. Zero length array when cancelled</returns>
        public static string[] OpenFolders(string title, string directory = "", bool multiselect = true)
        {
            return platformWrapper.OpenFolders(title, directory, multiselect);
        }

        /// <summary>Open native save file browser</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extension">File extension</param>
        /// <returns>Returns chosen file. Empty string when cancelled</returns>
        public static string SaveFile(string title, string directory, string defaultName, string extension)
        {
            return SaveFile(title, directory, defaultName, getFilter(extension));
        }

        /// <summary>Open native save file browser</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <returns>Returns chosen file. Empty string when cancelled</returns>
        public static string SaveFile(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            return platformWrapper.SaveFile(title, directory, defaultName, extensions);
        }

        /// <summary>Open native file browser for multiple files.</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extension">Allowed extension</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <param name="cb">Callback for the async operation.</param>
        /// <returns>Returns array of chosen files. Zero length array when cancelled</returns>
        public static void OpenFilesAsync(string title, string directory, string extension, bool multiselect, Action<string[]> cb)
        {
            OpenFilesAsync(title, directory, getFilter(extension), multiselect, cb);
        }

        /// <summary>Open native file browser for multiple files (async).</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="multiselect">Allow multiple file selection</param>
        /// <param name="cb">Callback for the async operation.</param>
        /// <returns>Returns array of chosen files. Zero length array when cancelled</returns>
        public static void OpenFilesAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb)
        {
            //System.Threading.Thread worker = new System.Threading.Thread(() => platformWrapper.OpenFilesAsync(title, directory, extensions, multiselect, cb));
            //worker.Start();
            platformWrapper.OpenFilesAsync(title, directory, extensions, multiselect, cb);
        }

        /// <summary>Open native folder browser for multiple folders (async).</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="multiselect"></param>
        /// <param name="cb">Callback for the async operation.</param>
        /// <returns>Returns array of chosen folders. Zero length array when cancelled</returns>
        public static void OpenFoldersAsync(string title, string directory, bool multiselect, Action<string[]> cb)
        {
            //System.Threading.Thread worker = new System.Threading.Thread(() => platformWrapper.OpenFoldersAsync(title, directory, multiselect, cb));
            //worker.Start();
            platformWrapper.OpenFoldersAsync(title, directory, multiselect, cb);
        }

        /// <summary>Open native save file browser</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extension">File extension</param>
        /// <param name="cb">Callback for the async operation.</param>
        /// <returns>Returns chosen file. Empty string when cancelled</returns>
        public static void SaveFileAsync(string title, string directory, string defaultName, string extension, Action<string> cb)
        {
            SaveFileAsync(title, directory, defaultName, getFilter(extension), cb);
        }

        /// <summary>Open native save file browser (async).</summary>
        /// <param name="title">Dialog title</param>
        /// <param name="directory">Root directory</param>
        /// <param name="defaultName">Default file name</param>
        /// <param name="extensions">List of extension filters. Filter Example: new ExtensionFilter("Image Files", "jpg", "png")</param>
        /// <param name="cb">Callback for the async operation.</param>
        /// <returns>Returns chosen file. Empty string when cancelled</returns>
        public static void SaveFileAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
        {
            //System.Threading.Thread worker = new System.Threading.Thread(() => platformWrapper.SaveFileAsync(title, directory, defaultName, extensions, cb));
            //worker.Start();
            platformWrapper.SaveFileAsync(title, directory, defaultName, extensions, cb);
        }

        #endregion


        #region Private methods

        private static ExtensionFilter[] getFilter(string extension)
        {
            return string.IsNullOrEmpty(extension) ? null : new[] { new ExtensionFilter(string.Empty, extension) };
        }

        #endregion
    }

    /// <summary>Filter for extensions.</summary>
    public struct ExtensionFilter
    {
        public string Name;
        public string[] Extensions;

        public ExtensionFilter(string filterName, params string[] filterExtensions)
        {
            Name = filterName;
            Extensions = filterExtensions;
        }
    }
}
// © 2017-2018 crosstales LLC (https://www.crosstales.com)