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
	private const float DOUBLE_CLICK_TIME = 0.2f;
	
	/// <summary>
	/// The selected directory location.
	/// </summary>
	private string directoryLocation;
	
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
	
	public FileBrowser(string windowTitle, bool allowEsc = true)
	{
		title = windowTitle;
		_allowEsc = allowEsc;
		
		string exampleDir = Application.dataPath + "\\..\\examples\\default-robot-chassis\\synthesis-output";
		// If we have a last-used directory.
		/** /
        if (BXDSettings.Instance.LastSkeletonDirectory != null && Directory.Exists(BXDSettings.Instance.LastSkeletonDirectory))
        {
            directoryLocation = BXDSettings.Instance.LastSkeletonDirectory;
        }
        else if (Directory.Exists(exampleDir))  // Otherwise try the example directory
        {
            directoryLocation = (new DirectoryInfo(exampleDir)).FullName;
        }
        else // Otherwise the application data directory
        {
            directoryLocation = Directory.GetParent(Application.dataPath).FullName;
        }
        /**/
		if (Directory.Exists(exampleDir))  // Try the example directory
		{
			directoryLocation = (new DirectoryInfo(exampleDir)).FullName;
		}
		else // Otherwise the application data directory
		{
			directoryLocation = Directory.GetParent(Application.dataPath).FullName;
		}

        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyBackground") as Texture2D;
		buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load ("Fonts/Gravity-Regular") as Font;
        windowTexture = Resources.Load("Images/greyBackground") as Texture2D;
        
        //Custom style for windows
		fileBrowserWindow = new GUIStyle (GUI.skin.window);
		fileBrowserWindow.normal.background = windowTexture;
		fileBrowserWindow.onNormal.background = windowTexture;
		fileBrowserWindow.font = gravityRegular;

        //Custom style for buttons
		fileBrowserButton = new GUIStyle (GUI.skin.button);
		fileBrowserButton.font = gravityRegular;
		fileBrowserButton.normal.background = buttonTexture;
		fileBrowserButton.hover.background = buttonSelected;
		fileBrowserButton.active.background = buttonSelected;
		fileBrowserButton.onNormal.background = buttonSelected;
		fileBrowserButton.onHover.background = buttonSelected;
		fileBrowserButton.onActive.background = buttonSelected;

        //Custom style for labels
		fileBrowserLabel = new GUIStyle (GUI.skin.label);
		fileBrowserLabel.font = gravityRegular;
	}
	
	/// <summary>
	/// Draws the given list as buttons, and returns whichever one was selected.
	/// </summary>
	/// <typeparam name="T">The object type</typeparam>
	/// <param name="items">The items</param>
	/// <param name="stringify">Optional function to convert object to string</param>
	/// <param name="highlight">Optional currently-selected item's string representation</param>
	/// <returns>The selected object</returns>
	private static object SelectList<T>(IEnumerable<T> items, System.Func<T, string> stringify, string highlight)
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
			if (GUILayout.Button(entry, fileBrowserButton))
			{
				selected = o;
			}
			GUI.backgroundColor = bg;
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
		
		if (_allowEsc && GUI.Button(new Rect(410, 10, 80, 20), "Exit", fileBrowserButton))
		{
			Active = false;
		}
		
		if (directoryInfo.Parent != null && GUI.Button(new Rect(10, 10, 110, 25), "Up one level", fileBrowserButton))
		{
			directoryInfo = directoryInfo.Parent;
			directoryLocation = directoryInfo.FullName;
		}
		
		
		// Handle the directories list
		GUILayout.BeginArea(new Rect(10, 35, 480, 300));
		GUILayout.Label("Directories:", fileBrowserLabel);
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
		GUILayout.BeginArea(new Rect(10, 390, 480, 25));
		GUILayout.BeginHorizontal();
		const int labelLen = 50;
		GUILayout.Label(directoryLocation.Length > labelLen ?
		                directoryLocation.Substring(0, 5) + "..." + directoryLocation.Substring(directoryLocation.Length - labelLen + 8) : 
		                directoryLocation);
		
		bool doubleClick = directorySelection != null && (Time.time - lastClick) > 0 && (Time.time - lastClick) < DOUBLE_CLICK_TIME;
		if (doubleClick || GUILayout.Button("Select", fileBrowserButton, GUILayout.Width(58)))
		{
			_active = false;
			if (OnComplete != null)
				OnComplete(directoryLocation);
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