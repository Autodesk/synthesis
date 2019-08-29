using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Synthesis.GUI
{
    class SynthesisFileBrowser
    {
        public string directoryLocation;

        private string tempSelection;

        /// <summary>
        /// The title of the window.
        /// </summary>
        private string title;

        private bool _active;

        private bool _allowEsc;

        public event Action<object> OnComplete;

        /// <summary>
        /// Default Directory Path
        /// </summary>
        private string directoryPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public SynthesisFileBrowser(string windowTitle, bool allowEsc = true)
        {
            Init(windowTitle, Directory.GetParent(Application.dataPath).FullName, allowEsc);
        }

        public SynthesisFileBrowser(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            DirectoryInfo directorySelection;

            if (Directory.Exists(defaultDirectory)) directoryPath = defaultDirectory;
            Init(windowTitle, defaultDirectory, allowEsc);

            directorySelection = new DirectoryInfo(defaultDirectory);

            if (defaultDirectory != null)
            {

                //Use try/catch to prevent users from getting in unauthorized folders
                try
                {
                    //If directory contains field or robot files, display error message to user prompting them to select directory
                    //instead of the actual field
                    if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
                                                                      || directorySelection.GetFiles("*.bxdj").Length != 0)
                    {
                        UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
                        directorySelection = directorySelection.Parent;
                        directoryPath = directorySelection.FullName;
                        directoryLocation = directorySelection.FullName;
                        defaultDirectory = directorySelection.FullName;
                        defaultDirectory = directoryLocation;
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
                }
            }
        }

        void Init(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            title = windowTitle;
            _allowEsc = allowEsc;

            directoryLocation = defaultDirectory;
        }

        public void CompleteDirectorySelection()
        {
            OnComplete?.Invoke(directoryLocation);
        }
    }
}