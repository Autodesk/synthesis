using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// Modified version of http://wiki.unity3d.com/index.php?title=FileBrowser.
/// Directory only based file browser.
/// </summary>
class FileBrowser : OverlayWindow
{
    private Rect windowRect = new Rect((Screen.width - 430) / 2, (Screen.height - 380) / 2, 430, 380);

    /// <summary>
    /// The maximum time in seconds between clicks to be considered a double click.
    /// </summary>
    private const float DOUBLE_CLICK_TIME = .2f;

    /// <summary>
    /// The selected directory location for two clicks.
    /// </summary>
    private string directoryLocation;

    /// <summary>
    /// The selected directory location for one click.
    /// </summary>
    private string selectedDirectoryLocation;

    /// <summary>
    /// Temporary placeholder for directoryLocation and selectedDirectoryLocation
    /// </summary>
    private string currentDirectory;

    /// <summary>
    /// The title of the window.
    /// </summary>
    private string title;

    private bool _active;

    private bool _allowEsc;

    public event Action<object> OnComplete;

    /// <summary>
    /// If this file browser is currently visible.
    /// </summary>
    public bool Active
    {
        get
        {
            return _active;
        }
        set
        {
            _active = value;
        }
    }

    /// <summary>
    /// Default Directory Path
    /// </summary>
    private string directoryPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
    private string backupPath = new DirectoryInfo(Application.dataPath).FullName;

    /// <summary>
    /// Internal reference to scroll position.
    /// </summary>
    private Vector2 directoryScroll;

    /// <summary>
    /// Internal reference to the last click time.
    /// </summary>
    private float lastClick = 0;

    /// <summary>
    /// Default textures.
    /// </summary>
    private Texture2D buttonTexture;
    private Texture2D windowTexture;
    /// <summary>
	/// Selected button texture.
	/// </summary>
	private Texture2D buttonSelected;
    /// <summary>
	/// Gravity-Regular font.
	/// </summary>
	private Font gravityRegular;
    private Font russoOne;
    /// <summary>
    /// Custom GUIStyle for windows.
    /// </summary>
    private GUIStyle fileBrowserWindow;
    /// <summary>
	/// Custom GUIStyle for buttons.
	/// </summary>
	private static GUIStyle fileBrowserButton;
    /// <summary>
	/// Custom GUIStyle for labels.
	/// </summary>s
	private GUIStyle fileBrowserLabel;

    /// <summary>
    /// Custom GUIStyle for highlight feature
    /// Same theme as scene in ScrollableList.cs
    /// </summary>
    private GUIStyle listStyle;
    private GUIStyle highlightStyle;
    private GUIStyle buttonStyle;

    public FileBrowser(string windowTitle, bool allowEsc = true)
    {
        Init(windowTitle, Directory.GetParent(Application.dataPath).FullName, allowEsc);
    }

    public FileBrowser(string windowTitle, string defaultDirectory, bool allowEsc = true)
    {
        if (Directory.Exists(defaultDirectory)) directoryPath = defaultDirectory;
        Init(windowTitle, defaultDirectory, allowEsc);
    }

    void Init(string windowTitle, string defaultDirectory, bool allowEsc = true)
    {
        title = windowTitle;
        _allowEsc = allowEsc;

        directoryLocation = defaultDirectory;
        selectedDirectoryLocation = defaultDirectory;
        currentDirectory = defaultDirectory;

        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
        windowTexture = Resources.Load("Images/greyBackground") as Texture2D;

        //Custom style for windows
        fileBrowserWindow = new GUIStyle(GUI.skin.window);
        fileBrowserWindow.normal.background = windowTexture;
        fileBrowserWindow.onNormal.background = windowTexture;
        fileBrowserWindow.font = russoOne;

        //Custom style for buttons
        fileBrowserButton = new GUIStyle(GUI.skin.button);
        fileBrowserButton.font = russoOne;
        fileBrowserButton.normal.background = buttonTexture;
        fileBrowserButton.hover.background = buttonSelected;
        fileBrowserButton.active.background = buttonSelected;
        fileBrowserButton.onNormal.background = buttonSelected;
        fileBrowserButton.onHover.background = buttonSelected;
        fileBrowserButton.onActive.background = buttonSelected;

        //Custom style for highlighted directory buttons (same theme as seen in the ScrollableList.cs)
        listStyle = new GUIStyle("button");
        listStyle.normal.background = buttonTexture;
        listStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        listStyle.active.background = Resources.Load("images/highlightsquaretexture") as Texture2D;
        listStyle.font = russoOne;

        //Custome style for highlight feature
        highlightStyle = new GUIStyle(listStyle);
        highlightStyle.normal.background = listStyle.active.background;
        highlightStyle.hover.background = highlightStyle.normal.background;

        //Custom style for labels
        fileBrowserLabel = new GUIStyle(GUI.skin.label);
        fileBrowserLabel.font = russoOne;
    }

    /// <summary>
    /// Draws the given list as buttons, and returns whichever one was selected.
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <param name="items">The items</param>
    /// <param name="stringify">Optional function to convert object to string</param>
    /// <param name="highlight">Optional currently-selected item's string representation</param>
    /// <returns>The selected object</returns>
    private object SelectList<T>(IEnumerable<T> items, System.Func<T, string> stringify, string highlight)
    {
        object selected = null;
        foreach (T o in items)
        {
            string entry = stringify != null ? stringify(o) : o.ToString();;

            if (highlight != null && highlight.Equals(entry))
            {
                Debug.Log(entry);
                if (GUILayout.Button(entry, highlightStyle))
                {
                    selected = o;
                }
            }
            if (GUILayout.Button(entry, listStyle))
            {
                selected = o;
            }
        }
        return selected;
    }

    /// <summary>
    /// Renders the browser window.
    /// </summary>
    /// <param name="idx">Window index</param>
    private void FileBrowserWindow(int idx)
    {
        DirectoryInfo directoryInfo;
        DirectoryInfo directorySelection;

        // Get the directory info of the current location
        {
            FileInfo fileSelection = new FileInfo(directoryLocation);
            if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                directoryInfo = new DirectoryInfo(directoryLocation);
                if (directoryInfo.GetDirectories().Length == 0 && title.Equals("Load Robot"))
                {
                    directoryInfo = directoryInfo.Parent;
                }
            }
            else
            {
                directoryInfo = fileSelection.Directory;
            }
        }

        if (_allowEsc && GUI.Button(new Rect(410, 10, 80, 20), "Exit", fileBrowserButton))
        {
            Active = false;
        }

        if (directoryInfo.Parent != null && GUI.Button(new Rect(10, 10, 120, 25), "Up one level", fileBrowserButton))
        {
            directoryInfo = directoryInfo.Parent;
            directoryLocation = directoryInfo.FullName;
            selectedDirectoryLocation = directoryInfo.FullName;
            currentDirectory = directoryInfo.FullName;
        }

        // Handle the directories list
        GUILayout.BeginArea(new Rect(10, 35, 480, 300));
        GUILayout.Label("When choosing a folder, please select the field/robot folder containing these elements" +
                " " + "NOT the field/robot itself!", fileBrowserLabel);

        directoryScroll = GUILayout.BeginScrollView(directoryScroll);
        directorySelection = SelectList(directoryInfo.GetDirectories(), (DirectoryInfo o) =>
        {
            return o.Name;
        }, new DirectoryInfo(directoryLocation).Name) as DirectoryInfo;

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (directorySelection != null && selectedDirectoryLocation != null && currentDirectory != null)
        {

            bool doubleClick = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;

            if (doubleClick)
            {
                // If a directory was double clicked, jump there
                directoryLocation = directorySelection.FullName;

                // If directory contains field or robot files, display error message to user prompting them to select directory
                // instead of the actual field
                if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
                                                                      || directorySelection.GetFiles("*.bxdj").Length != 0)
                {
                    directoryLocation = currentDirectory;
                    UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
                }
            }

            else
            {
                // If directory was clicked once, select it as a current path and highlight it
                selectedDirectoryLocation = directorySelection.FullName;

                // If directory contains field or robot files, display error message to user prompting them to select directory
                // instead of the actual field
                if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
                                                                      || directorySelection.GetFiles("*.bxdj").Length != 0)
                {
                    directoryLocation = currentDirectory;
                    UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
                }
            }
        }

        // The manual location box and the select button
        GUILayout.BeginArea(new Rect(10, 390, 480, 25));
        GUILayout.BeginHorizontal();
        const int labelLen = 50;

        bool twoClicks = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;

        if (twoClicks)
        {
            GUILayout.Label(directoryLocation.Length > labelLen ?
                        directoryLocation.Substring(0, 5) + "..." + directoryLocation.Substring(directoryLocation.Length - labelLen + 8) :
                        directoryLocation, fileBrowserLabel);
        }
        else
        {
            GUILayout.Label(selectedDirectoryLocation.Length > labelLen ?
                            selectedDirectoryLocation.Substring(0, 5) + "..." +
                            selectedDirectoryLocation.Substring(selectedDirectoryLocation.Length - labelLen + 8) :
                            selectedDirectoryLocation, fileBrowserLabel);
        }

        if (GUILayout.Button("Select", fileBrowserButton, GUILayout.Width(68)))
        {
            _active = false;
            if (OnComplete != null)
            {
                OnComplete(directoryLocation);
            }
            OnComplete(selectedDirectoryLocation);
        }
        if (directorySelection != null)
        {
            lastClick = Time.time;
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    /// <summary>
    /// Renders the window if it is active.
    /// </summary>
    public void Render()
    {
        if (_active)
        {
            windowRect = new Rect((Screen.width - 500) / 2, (Screen.height - 420) / 2, 500, 420);
            GUI.Window(0, windowRect, FileBrowserWindow, title, fileBrowserWindow);
        }
    }

    /// <summary>
    /// Gets the rect.
    /// </summary>
    /// <returns>The rect.</returns>
    public Rect GetWindowRect()
    {
        return windowRect;
    }
}