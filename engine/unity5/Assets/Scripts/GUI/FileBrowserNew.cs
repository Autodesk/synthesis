using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using System.IO;
using Synthesis.GUI;

namespace Crosstales.FB
{
    public class FileBrowserNew : MonoBehaviour
    {
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
        /// If this file browser is currently visible.
        /// </summary>
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        GameObject ScrollView;
        GameObject TextPrefab;

        /// <summary>
        /// Default Directory Path
        /// </summary>
        private string directoryPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        private DirectoryInfo tempSelection;

        public FileBrowserNew(string windowTitle, bool allowEsc = true)
        {
            Init(windowTitle, Directory.GetParent(Application.dataPath).FullName, allowEsc);
        }

        public FileBrowserNew(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            if (Directory.Exists(defaultDirectory)) directoryPath = defaultDirectory;
            Init(windowTitle, defaultDirectory, allowEsc);
        }

        void Init(string windowTitle, string defaultDirectory, bool allowEsc = true)
        {
            title = windowTitle;
            _allowEsc = allowEsc;
        }

        /// <summary>
        /// Renders the window if it is active.
        /// </summary>
        public void Render()
        {
            FileBrowserWindow();
        }

        // return the file string from OpenSingleFolder()
        public void OpenSingleFolder()
        {
            //Debug.Log("OpenSingleFolder");

            directoryPath = FileBrowser.OpenSingleFolder("Open Folder");

            //Debug.Log("Selected folder: " + path);

            RebuildList(directoryPath);
        }

        private void RebuildList(params string[] e)
        {
            for (int ii = ScrollView.transform.childCount - 1; ii >= 0; ii--)
            {
                Transform child = ScrollView.transform.GetChild(ii);
                child.SetParent(null);
                Destroy(child.gameObject);
            }

            ScrollView.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 80 * e.Length);

            for (int ii = 0; ii < e.Length; ii++)
            {
                //if (Config.DEBUG)
                //    Debug.Log(e[ii]);

                GameObject go = Instantiate(TextPrefab);

                go.transform.SetParent(ScrollView.transform);
                go.transform.localScale = Vector3.one;
                go.transform.localPosition = new Vector3(10, -80 * ii, 0);
                go.GetComponent<Text>().text = e[ii].ToString();
            }
        }


        /// <summary>
        /// Renders the browser window.
        /// </summary>
        /// <param name="idx">Window index</param>
        private void FileBrowserWindow()
        {
            DirectoryInfo directoryInfo;
            DirectoryInfo directorySelection;

            // Get the directory info of the current location
            FileInfo fileSelection = new FileInfo(directoryPath);
            if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                directoryInfo = new DirectoryInfo(directoryPath);
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

            //if (directoryPath != null)
            //{
            //    try
            //    {
            //        // If directory contains field or robot files, display error message to user prompting them to select directory
            //        // instead of the actual field
            //        if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
            //                                                          || directorySelection.GetFiles("*.bxdj").Length != 0)
            //        {
            //            UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
            //        }
            //        else
            //        {
            //            // If a directory without robot/field files was double clicked, jump there
            //            directoryLocation = directorySelection.FullName;

            //            targetFolderList.Clear();
            //            directorySearched = false;
            //        }
            //        tempSelection = null;
            //        else
            //        {
            //            // If directory contains field or robot files, display error message to user prompting them to select directory
            //            // instead of the actual field
            //            if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
            //                                                                  || directorySelection.GetFiles("*.bxdj").Length != 0)
            //            {
            //                UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
            //            }
            //            else
            //            {
            //                // If directory was clicked once, select it as a current path and highlight it
            //                selectedDirectoryLocation = directorySelection.FullName;
            //            }
            //        }
            //    }
            //    catch (UnauthorizedAccessException e)
            //    {
            //        UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
            //    }
            //}
            //else
            //{
            //    Active = false;
            //}
        }
    }
}
