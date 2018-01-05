using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


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
        directory = PlayerPrefs.GetString("RobotDirectory", (System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "//synthesis//Robots"));
        items = new List<string>();
        items.Clear();

    }

    // Update is called once per frame
    protected override void OnGUI()
    {
        if (directory != null && items.Count == 0)
        {
            string[] folders = System.IO.Directory.GetDirectories(directory);
            foreach (string robot in folders)
            {
                if (File.Exists(robot + "\\skeleton.bxdj")) items.Add(new DirectoryInfo(robot).Name);
            }
            if (items.Count > 0) selectedEntry = items[0];
        }

        position = GetComponent<RectTransform>().position;

        base.OnGUI();

    }
}
