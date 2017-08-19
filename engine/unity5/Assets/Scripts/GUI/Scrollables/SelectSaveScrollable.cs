using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Meant to be used for selecting a robot in the main menu
/// </summary>
public class SelectFieldScrollable : ScrollablePanel
{
    private MainMenu saveMainMenu;

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        errorMessage = "No fields found in directory!";

        saveMainMenu = canvas.GetComponent<MainMenu>();
    }

    void OnEnable()
    {
        items = new List<string>();
        items.Clear();

    }

    // Update is called once per frame
    protected override void OnGUI()
    {
        if (saveMainMenu.fieldDirectory != null && items.Count == 0)
        {
            string[] folders = System.IO.Directory.GetDirectories(mainMenu.fieldDirectory);
            foreach (string save in folders)
            {
                if (File.Exists(save + "\\definition.csv")) items.Add(new DirectoryInfo(save).Name);
            }
            if (items.Count > 0) selectedEntry = items[0];
        }

        position = Camera.main.WorldToScreenPoint(transform.position);

        base.OnGUI();

    }
}
