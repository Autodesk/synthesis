using Synthesis.FSM;
using Synthesis.GUI;
using System;
using System.IO;
using UnityEngine;

namespace Synthesis.States
{
    public abstract class BrowseFileState : State
    {
        private readonly string prefsKey;
        private readonly string directory;

        private FileBrowserNew fileBrowserNew;
        private FileBrowser fileBrowser;

        public string filePath;

        private GameObject navPanel;

        /// <summary>
        /// Initializes a new <see cref="BrowseFileState"/> instance.
        /// </summary>
        /// <param name="prefsKey"></param>
        /// <param name="directory"></param>
        protected BrowseFileState(string prefsKey, string directory)
        {
            this.prefsKey = prefsKey;
            this.directory = directory;
        }

        /// <summary>
        /// Disables the navigation bar when the <see cref="BrowseFileState"/>
        /// is launched.
        /// </summary>
        public override void Start()
        {
            //navPanel = GameObject.Find("NavigationPanel");
            //navPanel?.SetActive(false);
            //fileBrowser.Active = true;
        }

        /// <summary>
        /// Enables the navigation bar when the <see cref="BrowseFileState"/>
        /// is exited.
        /// </summary>
        public override void End()
        {
            //navPanel?.SetActive(true);
        }

        /// <summary>
        /// Renders the file browser.
        /// </summary>
        public override void OnGUI()
        {
            if (fileBrowser == null)
            {
                filePath = Crosstales.FB.FileBrowser.OpenSingleFolder(prefsKey, directory);

                // check for empty string (if native file browser is closed without selection) and default to Fields directory
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + ("//synthesis//Fields");
                }

                fileBrowser = new GUI.FileBrowser("Choose Robot Directory", filePath, true);
                fileBrowser.OnComplete += OnBrowserComplete;
                fileBrowser.CompleteDirectorySelection();

            }

            //fileBrowser.Render();


            //if (!fileBrowser.Active)
            //    StateMachine.PopState();
        }

        /// <summary>
        /// Exits the current <see cref="State"/> when the file browser is closed.
        /// </summary>
        /// <param name="obj"></param>
        public void OnBrowserComplete(object obj)
        {
            string fileLocation = (string)obj;
            DirectoryInfo directory = new DirectoryInfo(fileLocation);

            if (directory != null && directory.Exists)
            {
                PlayerPrefs.SetString(prefsKey, directory.FullName);
                PlayerPrefs.Save();
                StateMachine.PopState();
            }
            else
            {
                UserMessageManager.Dispatch("Invalid selection!", 10f);
            }
        }
    }
}
