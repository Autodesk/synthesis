// http://wiki.unity3d.com/index.php?title=FileBrowser

using UnityEngine;
using System.IO;

class FileBrowser
{
    private const float DOUBLE_CLICK_TIME = 0.2f;

    public string directoryLocation
    {
        get;
        private set;
    }

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

    private Vector2 directoryScroll;
    private float lastClick = 0;

    public FileBrowser()
    {
        string exampleDir = Application.dataPath + "\\..\\examples\\default-robot-chassis\\synthesis-output";
        if (BXDSettings.Instance.LastSkeletonDirectory != null && Directory.Exists(BXDSettings.Instance.LastSkeletonDirectory))
        {
            directoryLocation = BXDSettings.Instance.LastSkeletonDirectory;
        }
        else if (Directory.Exists(exampleDir))
        {
            directoryLocation = (new DirectoryInfo(exampleDir)).FullName;
        }
        else
        {
            directoryLocation = Directory.GetParent(Application.dataPath).FullName;
        }
    }

    private delegate string Stringify<T>(T o);

    private static object SelectList<T>(T[] items, Stringify<T> stringify = null, string highlight = null)
    {
        object selected = null;
        Color bg = GUI.backgroundColor;
        foreach (T o in items)
        {
            string entry = stringify != null ? stringify(o) : o.ToString();
            if (highlight != null && highlight.Equals(entry))
            {
                GUI.backgroundColor = new Color(1f, 0.25f, 0.25f, bg.a);
            }
            if (GUILayout.Button(entry))
            {
                selected = o;
            }
            GUI.backgroundColor = bg;
        }
        return selected;
    }

    private void FileBrowserWindow(int idx)
    {
        DirectoryInfo directoryInfo;
        DirectoryInfo directorySelection;
        int contentWidth;

        // Get the directory info of the current location
        {
            FileInfo fileSelection = new FileInfo(directoryLocation);
            if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                directoryInfo = new DirectoryInfo(directoryLocation);
                if (directoryInfo.GetDirectories().Length == 0)
                {
                    directoryInfo = directoryInfo.Parent;
                }
            }
            else
            {
                directoryInfo = fileSelection.Directory;
            }
        }

        if (GUI.Button(new Rect(335, 5, 80, 20), "Exit"))
        {
            Active = false;
            Submit = false;
        }

        if (directoryInfo.Parent != null && GUI.Button(new Rect(10, 20, 200, 20), "Up one level"))
        {
            directoryInfo = directoryInfo.Parent;
            directoryLocation = directoryInfo.FullName;
        }


        // Handle the directories list
        GUILayout.BeginArea(new Rect(10, 40, 410, 300));
        GUILayout.Label("Directories:");
        directoryScroll = GUILayout.BeginScrollView(directoryScroll);
        directorySelection = SelectList(directoryInfo.GetDirectories(), (DirectoryInfo o) =>
        {
            return o.Name;
        }, new DirectoryInfo(directoryLocation).Name) as DirectoryInfo;
        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (directorySelection != null)
        {
            // If a directory was selected, jump there
            directoryLocation = directorySelection.FullName;
        }

        // The manual location box and the select button
        GUILayout.BeginArea(new Rect(10, 350, 410, 20));
        GUILayout.BeginHorizontal();
        const int labelLen = 50;
        GUILayout.Label(directoryLocation.Length > labelLen ?
            directoryLocation.Substring(0, 5) + "..." +
            directoryLocation.Substring(directoryLocation.Length - labelLen + 8) : directoryLocation);

        contentWidth = (int) GUI.skin.GetStyle("Button").CalcSize(new GUIContent("Select")).x;
        bool doubleClick = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;
        if (doubleClick || GUILayout.Button("Select", GUILayout.Width(contentWidth)))
        {
            Submit = true;
            _active = false;
        }
        if (directorySelection != null)
        {
            lastClick = Time.time;
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