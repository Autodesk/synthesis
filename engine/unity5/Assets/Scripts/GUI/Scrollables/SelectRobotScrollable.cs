using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Meant to be used for selecting a robot in the main menu
/// </summary>
public class SelectRobotScrollable : ScrollablePanel
{
    public void Refresh(string directory)
    {
        string[] folders = Directory.GetDirectories(directory);

        items.Clear();

        foreach (string robot in folders)
            if (File.Exists(robot + "\\skeleton.bxdj"))
                items.Add(new DirectoryInfo(robot).Name);

        if (items.Count > 0)
            selectedEntry = items[0];

        position = Camera.main.WorldToScreenPoint(transform.position);
    }

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        errorMessage = "No robots found in directory!";
    }

    void OnEnable()
    {
        items = new List<string>();
        items.Clear();
    }
}
