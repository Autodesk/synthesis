using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Synthesis.GUI
{
    class FileBrowser
    {
        /// <summary>
        /// The selected directory location for two clicks.
        /// </summary>
        private string directoryLocation;

        /// <summary>
        /// The selected directory location for one click.
        /// </summary>
        private string selectedDirectoryLocation;

        private string tempSelection;

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

        public FileBrowser(string windowTitle, bool allowEsc = true)
        {
            Init(windowTitle, Directory.GetParent(Application.dataPath).FullName, allowEsc);
        }

        public FileBrowser(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            DirectoryInfo directorySelection;

            if (Directory.Exists(defaultDirectory)) directoryPath = defaultDirectory;
            Init(windowTitle, defaultDirectory, allowEsc);

            directorySelection = new DirectoryInfo(defaultDirectory);

            //if (defaultDirectory != null)
            //{

            //    //Use try/catch to prevent users from getting in unauthorized folders
            //    try
            //    {
            //        //If directory contains field or robot files, display error message to user prompting them to select directory
            //        //instead of the actual field
            //        if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
            //                                                          || directorySelection.GetFiles("*.bxdj").Length != 0)
            //        {
            //            UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
            //            defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ("//synthesis//");
            //            //directorySelection = directorySelection.Parent;
            //            //defaultDirectory = directorySelection.FullName;
            //        }
            //        tempSelection = null;
            //    }
            //    catch (UnauthorizedAccessException e)
            //    {
            //        UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
            //    }
            //}
        }

        void Init(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            title = windowTitle;
            _allowEsc = allowEsc;

            directoryLocation = defaultDirectory;
            selectedDirectoryLocation = defaultDirectory;
        }

        public void CompleteDirectorySelection()
        {
            OnComplete?.Invoke(selectedDirectoryLocation);
        }
    }
}