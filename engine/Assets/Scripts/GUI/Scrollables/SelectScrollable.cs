using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.GUI.Scrollables
{
    public class SelectScrollable : ScrollablePanel
    {
        public string TargetFilename;
        public string[] TargetExtensions;
        public string ErrorMessage;

        public bool trigger = false;

        protected override void Start()
        {
            base.Start();
            listStyle.fontSize = 14;
            highlightStyle.fontSize = 14;
            toScale = false;
            errorMessage = ErrorMessage;
        }

        /// <summary>
        /// Refreshes the scrollable with the directory provided.
        /// </summary>
        /// <param name="directory"></param>
        public void Refresh(string directory)
        {
            items.Clear();
            if (Directory.Exists(directory))
            {
                foreach (string item in Directory.GetDirectories(directory))
                    foreach (string extension in TargetExtensions)
                        if (File.Exists(item + Path.DirectorySeparatorChar + TargetFilename + extension))
                            items.Add(new DirectoryInfo(item).Name);
            }
            if (items.Count > 0)
                selectedEntry = items[0];
            else
                selectedEntry = null;

            if (!trigger)
            {
                position = UnityEngine.Camera.main.WorldToScreenPoint(transform.position);
                trigger = true;
            }
        }

        /// <summary>
        /// Clears the list of items when this <see cref="SelectScrollable"/>
        /// is enabled.
        /// </summary>
        void OnEnable()
        {
            items = new List<string>();
            items.Clear();
        }
    }
}
