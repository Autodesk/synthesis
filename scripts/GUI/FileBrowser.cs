// http://wiki.unity3d.com/index.php?title=FileBrowser

using UnityEngine;
using System.IO;

class FileBrowser
{
    public string fileLocation
    {
        get;
        private set;
    }

    private Vector2 directoryScroll, fileScroll;
    private bool _active;
    public bool Submit
    {
        get;
        set;
    }
    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            if (value && !_active)
            {
                Submit = false;
            }
            _active = value;
        }
    }

    public FileBrowser()
    {
        if (BXDSettings.Instance.LastSkeletonDirectory != null && Directory.Exists(BXDSettings.Instance.LastSkeletonDirectory))
        {
            fileLocation = BXDSettings.Instance.LastSkeletonDirectory;
        }
        else
        {
            fileLocation = Directory.GetParent(Application.dataPath).FullName;
        }
    }

    private delegate string Stringify<T>(T o);

    private static object SelectList<T>(T[] items, Stringify<T> stringify = null)
    {
        object selected = null;
        foreach (T o in items)
        {
            if (GUILayout.Button(stringify != null ? stringify(o) : o.ToString()))
            {
                selected = o;
            }
        }
        return selected;
    }

    private void FileBrowserWindow(int idx)
    {
        DirectoryInfo directoryInfo;
        DirectoryInfo directorySelection;
        FileInfo fileSelection;
        int contentWidth;

        // Get the directory info of the current location
        fileSelection = new FileInfo(fileLocation);
        if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
        {
            directoryInfo = new DirectoryInfo(fileLocation);
        }
        else
        {
            directoryInfo = fileSelection.Directory;
        }

        if (GUI.Button(new Rect(335, 5, 80, 20), "Exit"))
        {
            Active = false;
            Submit = false;
        }

        if (directoryInfo.Parent != null && GUI.Button(new Rect(10, 20, 200, 20), "Up one level"))
        {
            directoryInfo = directoryInfo.Parent;
            fileLocation = directoryInfo.FullName;
        }


        // Handle the directories list
        GUILayout.BeginArea(new Rect(10, 40, 200, 300));
        GUILayout.Label("Directories:");
        directoryScroll = GUILayout.BeginScrollView(directoryScroll);
        directorySelection = SelectList(directoryInfo.GetDirectories(), (DirectoryInfo o) =>
        {
            return o.Name;
        }) as DirectoryInfo;
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (directorySelection != null)
        // If a directory was selected, jump there
        {
            fileLocation = directorySelection.FullName;
        }


        // Handle the files list
        GUILayout.BeginArea(new Rect(220, 40, 200, 300));
        GUILayout.Label("Files:");
        fileScroll = GUILayout.BeginScrollView(fileScroll);
        fileSelection = SelectList(directoryInfo.GetFiles("*.bxdj"), (FileInfo f) =>
        {
            return f.Name;
        }) as FileInfo;
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (fileSelection != null)
        {
            fileLocation = fileSelection.FullName;
        }


        // The manual location box and the select button
        GUILayout.BeginArea(new Rect(10, 350, 410, 20));
        GUILayout.BeginHorizontal();
        const int labelLen = 50;
        GUILayout.Label(fileLocation.Length > labelLen ? 
            fileLocation.Substring(0, 5) + "..." +
            fileLocation.Substring(fileLocation.Length - labelLen + 8) : fileLocation);

        contentWidth = (int) GUI.skin.GetStyle("Button").CalcSize(new GUIContent("Select")).x;
        if (GUILayout.Button("Select", GUILayout.Width(contentWidth)))
        {
            Submit = true;
            _active = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    public void Show()
    {
        if (_active)
        {
            GUI.Window(0, new Rect((Screen.width - 430) / 2, (Screen.height - 380) / 2, 430, 380), FileBrowserWindow, "Browse");
        }
    }
}