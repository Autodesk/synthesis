using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Synthesis.GUI.Scrollables
{
    /// <summary>
    /// Meant to be used for changing robots within the simulator
    /// </summary>
    public class ChangeRobotScrollable : ScrollablePanel
    {
        private string directory;

        // Use this for initialization
        protected override void Start()
        {

            base.Start();
            listStyle.fontSize = 14;
            highlightStyle.fontSize = 14;
            toScale = false;
            errorMessage = "No robots found in directory!";
        }

        void OnEnable()
        {
            directory = PlayerPrefs.GetString("RobotDirectory");
            items = new List<string>();
            items.Clear();

        }

        // Update is called once per frame
        protected override void OnGUI()
        {
            if (directory != null && Directory.Exists(directory) && items.Count == 0)
            {
                string[] folders = Directory.GetDirectories(directory);
                string[] extensions = { ".json", ".bxdj" };
                foreach (string robot in folders)
                {
                   foreach(string ext in extensions)
                    {
                        if (File.Exists(robot + Path.DirectorySeparatorChar + "skeleton" + ext)) items.Add(new DirectoryInfo(robot).Name);
                    }
                }
                if (items.Count > 0) selectedEntry = items[0];
            }
            if (directory == null || !Directory.Exists(directory) || items.Count == 0)
            {
                GUILayout.Label(errorMessage, listStyle);
            }

            position = GetComponent<RectTransform>().position;

            base.OnGUI();

        }
    }
}