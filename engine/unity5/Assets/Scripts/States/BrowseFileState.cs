using Synthesis.FSM;
using Synthesis.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.States
{
    public abstract class BrowseFileState : State
    {
        private readonly string prefsKey;
        private readonly string directory;

        public string filePath;

        private SynthesisFileBrowser fileBrowser;
        Text pathLabel = GameObject.Find("PathLabel").GetComponent<Text>();

        /// <summary>
        /// Initializes a new <see cref="BrowseFileState"/> instance.
        /// </summary>
        /// <param name="prefsKey"></param>
        protected BrowseFileState(string prefsKey)
        {
            this.prefsKey = prefsKey;
            this.directory = PlayerPrefs.GetString(prefsKey);
            if (!Directory.Exists(directory)) {
                this.directory = "";
            }
        }

        /// <summary>
        /// Disables the navigation bar when the <see cref="BrowseFileState"/>
        /// is launched.
        /// </summary>
        public override void Start()
        {

        }

        /// <summary>
        /// Enables the navigation bar when the <see cref="BrowseFileState"/>
        /// is exited.
        /// </summary>
        public override void End()
        {

        }

        /// <summary>
        /// Renders the file browser. Native filebrowser class derived from Crosstales plugin.
        /// </summary>
        public override void OnGUI()
        {
            if (fileBrowser == null)
            {
                // Standalone plugin adaptions from: https://github.com/gkngkc/UnityStandaloneFileBrowser
                filePath = SFB.StandaloneFileBrowser.OpenFolderPanel(prefsKey, directory, false);

                if (!string.IsNullOrEmpty(filePath))
                {
                    fileBrowser = new SynthesisFileBrowser("Choose Directory", filePath, true);
                    filePath = fileBrowser.directoryLocation;
                    fileBrowser.OnComplete += OnBrowserComplete;
                    fileBrowser.CompleteDirectorySelection();
                    pathLabel.text = filePath;
                }
                else
                {
                    UserMessageManager.Dispatch("Selection canceled", 5);
                    StateMachine.PopState();
                }
            }
        }

        /// <summary>
        /// Exits the current <see cref="State"/> when the file browser is closed.
        /// </summary>
        /// <param name="obj"></param>
        public void OnBrowserComplete(object obj)
        {
            string fileLocation = (string)obj;
            DirectoryInfo dir = new DirectoryInfo(fileLocation);
            PlayerPrefs.SetString(prefsKey, dir.Exists ? dir.FullName : "");
            PlayerPrefs.Save();
            StateMachine.PopState();
        }
    }
}
