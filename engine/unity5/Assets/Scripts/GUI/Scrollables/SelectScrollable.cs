using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class SelectScrollable : ScrollablePanel
{
    private readonly string targetFilename;

    /// <summary>
    /// Initializes the <see cref="SelectScrollable"/> with the given
    /// target file name.
    /// </summary>
    /// <param name="targetFilename"></param>
    protected SelectScrollable(string targetFilename, string errorMessage)
    {
        this.targetFilename = targetFilename;
        this.errorMessage = errorMessage;
    }

    /// <summary>
    /// Refreshes the scrollable with the directory provided.
    /// </summary>
    /// <param name="directory"></param>
    public void Refresh(string directory)
    {
        string[] folders = Directory.GetDirectories(directory);

        items.Clear();

        foreach (string robot in folders)
            if (File.Exists(robot + "\\" + targetFilename))
                items.Add(new DirectoryInfo(robot).Name);

        if (items.Count > 0)
            selectedEntry = items[0];

        position = Camera.main.WorldToScreenPoint(transform.position);
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
