using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Synthesis.GUI.Scrollables
{
    /// <summary>
    /// Meant to be used for changing fields within the simulator
    /// </summary>
    public class ChangeFieldScrollable : ScrollablePanel
    {
        private string directory;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            listStyle.fontSize = 14;
            highlightStyle.fontSize = 14;
            toScale = false;
            errorMessage = "No fields found in directory!";
        }

        void OnEnable()
        {
            directory = PlayerPrefs.GetString("FieldDirectory", (System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk" + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Fields"));
            items = new List<string>();
            items.Clear();
        }

        // Update is called once per frame
        protected override void OnGUI()
        {
            if (directory != null && items.Count == 0)
            {
                string[] folders = System.IO.Directory.GetDirectories(directory);
                foreach (string field in folders)
                {
                    if (File.Exists(field + Path.DirectorySeparatorChar + "definition.bxdf")) items.Add(new DirectoryInfo(field).Name);
                }
                if (items.Count > 0) selectedEntry = items[0];
            }

            position = GetComponent<RectTransform>().position;

            base.OnGUI();
        }
    }
}
