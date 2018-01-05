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
    private const float DOUBLE_CLICK_TIME = .3f;

    /// <summary>
    /// The selected directory location for two clicks.
    /// </summary>
    private string directoryLocation;

    /// <summary>
    /// The selected directory location for one click.
    /// </summary>
    private string selectedDirectoryLocation;

    /// <summary>
    /// The title of the window.
    /// </summary>
    private string title;

    private bool _active;

    private bool _allowEsc;

    public event Action<object> OnComplete;

    private List<string> targetFolderList = new List<string>();

    private bool directorySearched;

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
    private string directoryPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
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
    private Texture2D searchedButtonTexture;
    /// <summary>
    /// Custom GUIStyle for the search button after searching the current directory
    /// </summary>
    private static GUIStyle searchedButton;
    /// <summary>
	/// Custom GUIStyle for labels.
	/// </summary>s
	private GUIStyle fileBrowserLabel;
    /// <summary>
    /// Custom GUIStyle for path labels.
    /// </summary>
    private GUIStyle pathLabel;

    /// <summary>
    /// Custom GUIStyle for highlight feature
    /// Same theme as scene in ScrollableList.cs
    /// </summary>
    private GUIStyle listStyle;
    private GUIStyle highlightStyle;
    private GUIStyle targetStyle;
    private GUIStyle buttonStyle;

    /// <summary>
    /// Custom GUIStyle for browser description text
    /// </summary>
    private GUIStyle descriptionStyle;

    private DirectoryInfo tempSelection;

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

        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
        windowTexture = Resources.Load("Images/greyBackground") as Texture2D;
        searchedButtonTexture = Resources.Load("Images/searchedButton") as Texture2D;
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

        //Custom style for the search button after searching
        searchedButton = new GUIStyle(GUI.skin.button);
        searchedButton.font = russoOne;
        searchedButton.normal.background = searchedButtonTexture;
        searchedButton.hover.background = searchedButtonTexture;
        searchedButton.active.background = searchedButtonTexture;
        searchedButton.onNormal.background = searchedButtonTexture;
        searchedButton.onHover.background = searchedButtonTexture;
        searchedButton.onActive.background = searchedButtonTexture;

        //Custom style for highlighted directory buttons (same theme as seen in the ScrollableList.cs)
        listStyle = new GUIStyle("button");
        listStyle.normal.background = buttonTexture;
        listStyle.hover.background = Resources.Load("Images/darksquaretexture") as Texture2D;
        listStyle.active.background = Resources.Load("Images/highlightsquaretexture") as Texture2D;
        listStyle.font = russoOne;

        //Custome style for highlight feature
        highlightStyle = new GUIStyle(listStyle);
        highlightStyle.normal.background = listStyle.active.background;
        highlightStyle.hover.background = highlightStyle.normal.background;

        //Custom style for target folder buttons
        targetStyle = new GUIStyle(listStyle);
        targetStyle.normal.background = Resources.Load("Images/targetsquaretexture") as Texture2D;
        targetStyle.hover.background = listStyle.active.background;

        //Custom style for labels
        fileBrowserLabel = new GUIStyle(GUI.skin.label);
        fileBrowserLabel.font = russoOne;

        //Custom style for path labels (smaller font size than  fileBrowserLabel
        pathLabel = new GUIStyle(GUI.skin.label);
        pathLabel.font = russoOne;
        pathLabel.fontSize = 12;

        //Custom style for description text
        descriptionStyle = new GUIStyle(GUI.skin.label);
        descriptionStyle.font = Resources.GetBuiltinResource<Font>("Arial.ttf") as Font;
        descriptionStyle.fontSize = 13;
        descriptionStyle.margin = new RectOffset(5, 5, 5, 2);

    }

    /// <summary>
    /// Draws the given list as buttons, and returns whichever one was selected.
    /// </summary>
    /// <typeparam name="T">The object type</typeparam>
    /// <param name="items">The items</param>
    /// <param name="stringify">Optional function to convert object to string</param>
    /// <param name="highlight">Optional currently-selected item's string representation</param>
    /// <param name="targetName"></param>A list of target folder names that needs to be highlighted</param>
    /// <returns>The selected object</returns>
    private object SelectList<T>(IEnumerable<T> items, System.Func<T, string> stringify, string highlight, List<string> targetName)
    {
        object selected = null;
        foreach (T o in items)
        {
            string entry = stringify != null ? stringify(o) : o.ToString(); ;

            //highlight temporary selection
            if (tempSelection != null && entry.Equals(tempSelection.Name) && !targetName.Contains(entry))
            {
                if (GUILayout.Button(entry, highlightStyle))
                {
                    selected = o;
                    tempSelection = o as DirectoryInfo;
                }
            }
            //highlight target folders after searching
            else if (targetName.Contains(entry))
            {
                if (GUILayout.Button(entry, targetStyle))
                {
                    selected = o;
                    tempSelection = o as DirectoryInfo;
                }
            }
            //regular button style
            else if (GUILayout.Button(entry, listStyle))
            {
                selected = o;
                tempSelection = o as DirectoryInfo;
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
        FileInfo fileSelection = new FileInfo(directoryLocation);
        if ((fileSelection.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
        {
            directoryInfo = new DirectoryInfo(directoryLocation);
            //If there is no directory in the current location go back to its parent folder
            if (directoryInfo.GetDirectories().Length == 0 && title.Equals("Load Robot"))
            {
                directoryInfo = directoryInfo.Parent;
            }
        }
        else
        {
            directoryInfo = fileSelection.Directory;
        }

        //If click Exit, close file browser
        if (_allowEsc && GUI.Button(new Rect(410, 10, 80, 20), "Exit", fileBrowserButton))
        {
            Active = false;
        }

        //If hit Up One Level, go back to parent folder level
        if (directoryInfo.Parent != null && GUI.Button(new Rect(10, 10, 120, 25), "Up One Level", fileBrowserButton))
        {
            directoryInfo = directoryInfo.Parent;
            directoryLocation = directoryInfo.FullName;
            selectedDirectoryLocation = directoryInfo.FullName;
            tempSelection = null;
            //Reset the target folder list and set the folder to unsearched
            targetFolderList.Clear();
            directorySearched = false;
        }

        // Handle the directories list
        GUILayout.BeginArea(new Rect(10, 35, 480, 300));

        GUILayout.Label("When choosing a folder, please select the field/robot folder containing these elements" +
                " " + "NOT the field/robot itself!", descriptionStyle);

        directoryScroll = GUILayout.BeginScrollView(directoryScroll);

        //Create a scrolling list and all the buttons having the folder names
        directorySelection = SelectList(directoryInfo.GetDirectories(), (DirectoryInfo o) =>
        {
            return o.Name;
        }, new DirectoryInfo(directoryLocation).Name, targetFolderList) as DirectoryInfo;

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        if (directorySelection != null && selectedDirectoryLocation != null)
        {
            bool doubleClick = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;
            //Use try/catch to prevent users from getting in unauthorized folders
            try
            {
                if (doubleClick)
                {
                    // If directory contains field or robot files, display error message to user prompting them to select directory
                    // instead of the actual field
                    if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
                                                                      || directorySelection.GetFiles("*.bxdj").Length != 0)
                    {
                        UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
                    }
                    else
                    {
                        // If a directory without robot/field files was double clicked, jump there
                        directoryLocation = directorySelection.FullName;

                        targetFolderList.Clear();
                        directorySearched = false;
                    }
                    tempSelection = null;

                }
                else
                {
                    // If directory contains field or robot files, display error message to user prompting them to select directory
                    // instead of the actual field
                    if (directorySelection.GetFiles("*.bxdf").Length != 0 || directorySelection.GetFiles("*.bxda").Length != 0
                                                                          || directorySelection.GetFiles("*.bxdj").Length != 0)
                    {
                        UserMessageManager.Dispatch("Please DO NOT select the field/robot itself!", 5);
                    }
                    else
                    {
                        // If directory was clicked once, select it as a current path and highlight it
                        selectedDirectoryLocation = directorySelection.FullName;
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
            }
        }

        // The manual location box and the select button
        GUILayout.BeginArea(new Rect(12, 335, 480, 25));
        //GUILayout.BeginHorizontal();
        const int labelLen = 70;

        bool twoClicks = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;

        try
        {
            if (twoClicks)
            {
                //If the file path is greater than labelLen, then it will replace part of the path name with "..."
                GUILayout.Label(directoryLocation.Length > labelLen ?
                        directoryLocation.Substring(0, 5) + "..." + directoryLocation.Substring(directoryLocation.Length - labelLen + 8) :
                        directoryLocation, pathLabel);
            }
            else
            {
                //One click displays the path of the selected folder
                GUILayout.Label(selectedDirectoryLocation.Length > labelLen ?
                                selectedDirectoryLocation.Substring(0, 5) + "..." +
                                selectedDirectoryLocation.Substring(selectedDirectoryLocation.Length - labelLen + 8) :
                                selectedDirectoryLocation, pathLabel);
            }
        }
        catch (UnauthorizedAccessException e)
        {
            UserMessageManager.Dispatch("You don't have the authorization to access this folder", 3f);
        }
        GUILayout.EndArea();
        GUILayout.BeginArea(new Rect(12, 360, 480, 25));
        GUILayout.BeginHorizontal();

        //When this button is clicked, search the directory for target files
        if (!directorySearched)
        {
            if (GUILayout.Button("Search for Target Directory", fileBrowserButton, GUILayout.Width(250)))
            {
                SearchDirectories(directoryInfo);

                //Notify the user there's nothing related inside the current directory
                if (targetFolderList.Count == 0)
                {
                    if (title.Equals("Choose Robot Directory"))
                    {
                        UserMessageManager.Dispatch("No exported robot files found in current directory", 5f);
                    }
                    else if (title.Equals("Choose Field Directory"))
                    {
                        UserMessageManager.Dispatch("No exported robot files found in current directory", 5f);
                    }
                }
            }
        }
        else
        {
            if (GUILayout.Button("Search for Target Directory", searchedButton, GUILayout.Width(250)))
            {
                UserMessageManager.Dispatch("The current directory has been searched.", 5f);
            }
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
        GUILayout.BeginArea(new Rect(12, 385, 480, 25));
        GUILayout.Label("Searching through a large directory takes time. Please be patient :)", descriptionStyle);
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

    /// <summary>
    /// Search through the directory to look for target files and add the name of the directory containing those files
    /// to the list for highlighting
    /// </summary>
    /// <param name="directoryInfo"></param>
    public void SearchDirectories(DirectoryInfo directoryInfo)
    {
        if (!directorySearched)
        {
            SearchOption so = SearchOption.AllDirectories;
            foreach (DirectoryInfo info in directoryInfo.GetDirectories())
            {
                //Use try/catch to prevent users from getting in unauthorized folders
                try
                {
                    if (title.Equals("Choose Robot Directory"))
                    {
                        if (info.GetFiles("*.bxdj", so).Length > 0 && info.GetFiles("*.bxda", so).Length > 0)
                        {
                            targetFolderList.Add(info.Name);
                        }
                    }
                    else if (title.Equals("Choose Field Directory"))
                    {
                        if (info.GetFiles("*.bxdf", so).Length > 0 && info.GetFiles("*.bxda", so).Length > 0)
                        {
                            targetFolderList.Add(info.Name);
                        }
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    continue;
                }
            }
            //Prevent unnecessary multiple search after searching result is out
            directorySearched = true;
        }
    }
}
