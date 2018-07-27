using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Synthesis.GUI
{
    /// <summary>
    /// Modified version of http://wiki.unity3d.com/index.php?title=FileBrowser.
    /// Directory only based file browser.
    /// </summary>
    class FileBrowser
    {
        //private Rect windowRect = new Rect((Screen.width - 430) / 2, (Screen.height - 380) / 2, 430, 380);

        /// <summary>
        /// The maximum time in seconds between clicks to be considered a double click.
        /// </summary>
        private const float DOUBLE_CLICK_TIME = .3f;

        /// <summary>
        /// The selected directory location for two clicks.
        /// </summary>
        private string directoryLocation;

        /// <summary>
        /// The selected directory location for one click.
        /// </summary>
        private string selectedDirectoryLocation;

        /// <summary>
        /// The title of the window.
        /// </summary>
        private string title;

        private bool _active;

        private bool _allowEsc;

        public event Action<object> OnComplete;

        private List<string> targetFolderList = new List<string>();

        private bool directorySearched;

        /// <summary>
        /// Default Directory Path
        /// </summary>
        private string directoryPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private DirectoryInfo tempSelection; 

        public FileBrowser(string windowTitle, bool allowEsc = true)
        {
            Init(windowTitle, Directory.GetParent(Application.dataPath).FullName, allowEsc);
        }

        public FileBrowser(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            if (Directory.Exists(defaultDirectory)) directoryPath = defaultDirectory;
            Init(windowTitle, defaultDirectory, allowEsc);
        }

        void Init(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            title = windowTitle;
            _allowEsc = allowEsc;

            directoryLocation = defaultDirectory;
            selectedDirectoryLocation = defaultDirectory;
        }

        /// <summary>
        /// Renders the browser window.
        /// </summary>
        /// <param name="idx">Window index</param>
        private void FileBrowserWindow(int idx)
        {
            DirectoryInfo directoryInfo;
            DirectoryInfo directorySelection;

            // Get the directory info of the current location
            FileInfo fileSelection = new FileInfo(directoryLocation);
            if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                directoryInfo = new DirectoryInfo(directoryLocation);
                //If there is no directory in the current location go back to its parent folder
                if (directoryInfo.GetDirectories().Length == 0 && title.Equals("Load Robot"))
                {
                    directoryInfo = directoryInfo.Parent;
                }
            }
            else
            {
                directoryInfo = fileSelection.Directory;
            }

            // if empty string
            //If click Exit, close file browser
            if (_allowEsc)
            {
                // return default path
                //If there is no directory in the current location go back to its parent folder
                if (directoryInfo.GetDirectories().Length == 0 && title.Equals("Load Robot"))
                {
                    directoryInfo = directoryInfo.Parent;
                }
            }

            //Create a scrolling list and all the buttons having the folder names
            directorySelection = new DirectoryInfo(selectedDirectoryLocation);
            //directorySelection = SelectList(directoryInfo.GetDirectories(), (DirectoryInfo o) =>
            //{
            //    return o.Name;
            //}, new DirectoryInfo(directoryLocation).Name, targetFolderList) as DirectoryInfo;

            if (directorySelection != null && selectedDirectoryLocation != null)
            {
                
                //Use try/catch to prevent users from getting in unauthorized folders
                try
                {
                    // If directory contains field or robot files, display error message to user prompting them to select directory
                    // instead of the actual field
                    //if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
                    //                                                  || directorySelection.GetFiles("*.bxdj").Length != 0)
                    //{
                    //    UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
                    //}
                    //else
                    //{
                    //    // If a directory without robot/field files was double clicked, jump there
                    //    directoryLocation = directorySelection.FullName;
                    //    selectedDirectoryLocation = directorySelection.FullName;

                    //    targetFolderList.Clear();
                    //    directorySearched = false;
                    //}
                    //tempSelection = null;
                }
                catch (UnauthorizedAccessException e)
                {
                    UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
                }
            }

            //// The manual location box and the select button
            //GUILayout.BeginArea(new Rect(12, 335, 480, 25));
            ////GUILayout.BeginHorizontal();
            //const int labelLen = 70;

            //bool twoClicks = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;

            //try
            //{
            //    if (twoClicks)
            //    {
            //        //If the file path is greater than labelLen, then it will replace part of the path name with "..."
            //        GUILayout.Label(directoryLocation.Length > labelLen ?
            //                directoryLocation.Substring(0, 5) + "..." + directoryLocation.Substring(directoryLocation.Length - labelLen + 8) :
            //                directoryLocation, pathLabel);
            //    }
            //    else
            //    {
            //        //One click displays the path of the selected folder
            //        GUILayout.Label(selectedDirectoryLocation.Length > labelLen ?
            //                        selectedDirectoryLocation.Substring(0, 5) + "..." +
            //                        selectedDirectoryLocation.Substring(selectedDirectoryLocation.Length - labelLen + 8) :
            //                        selectedDirectoryLocation, pathLabel);
            //    }
            //}
            //catch (UnauthorizedAccessException e)
            //{
            //    UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
            //}
            //GUILayout.EndArea();
            //LabelPanel();
            
        }

        //public void LabelPanel()
        //{
        //    GUILayout.BeginArea(new Rect(12, 360, 480, 25));
        //    GUILayout.BeginHorizontal();

        //    if (GUILayout.Button("Select", fileBrowserButton, GUILayout.Width(68)))
        //    {
        //        _active = false;
        //        OnComplete?.Invoke(selectedDirectoryLocation);
        //    }
        //    //if (directorySelection != null)
        //    //{
        //    //    lastClick = Time.time;
        //    //}

        //    GUILayout.EndHorizontal();
        //    GUILayout.EndArea();
        //}

        public void Complete()
        {
            OnComplete?.Invoke(selectedDirectoryLocation);
        }

        /// <summary>
        /// Renders the window if it is active.
        /// </summary>
        //public void Render()
        //{
        //    if (_active)
        //    {
        //        windowRect = new Rect((Screen.width - 500) / 2, (Screen.height - 420) / 2, 500, 420);
        //        UnityEngine.GUI.Window(0, windowRect, FileBrowserWindow, title, fileBrowserWindow);
        //    }
        //}

        /// <summary>
        /// Gets the rect.
        /// </summary>
        /// <returns>The rect.</returns>
        //public Rect GetWindowRect()
        //{
        //    return windowRect;
        //}
    }
}