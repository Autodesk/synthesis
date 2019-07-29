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
    public class LoadReplayScrollable : ScrollablePanel
    {
        private string directory;

        // Use this for initialization
        protected override void Start()
        {
            base.Start();
            listStyle.fontSize = 14;
            highlightStyle.fontSize = 14;
            toScale = false;
            errorMessage = "No replays found in directory!";
        }

        void OnEnable()
        {
            directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + "Autodesk"
                + Path.DirectorySeparatorChar + "Synthesis" + Path.DirectorySeparatorChar + "Replays";
            items = new List<string>();
            items.Clear();
        }

        // Update is called once per frame
        protected override void OnGUI()
        {
            if (directory != null && items.Count == 0)
            {
                string[] files = Directory.GetFiles(directory);
                foreach (string file in files)
                {
                    if (file.EndsWith(".replay")) items.Add(Path.GetFileName(file));
                    Debug.Log(directory);
                    Debug.Log(file);
                }
                if (items.Count > 0) selectedEntry = items[0];
                // items.Add(new DirectoryInfo(field).Name);
            }

            position = GetComponent<RectTransform>().position;

            base.OnGUI();
        }
    }
}
