using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// The GUI sidebar access to overlay windows, etc.
/// Useful functions are AddAction and AddWindow.
/// </summary>
class GUIController
{
    #region Style
    /// <summary>
    /// The sidebar fade time, seconds.
    /// </summary>
    private const float GUI_SHOW_TIME = .75f;
    /// <summary>
    /// The padding for the sidebar content, pixels.
    /// </summary>
    private static readonly Vector2 GUI_SIDEBAR_PADDING = new Vector2(10, 30);
    /// <summary>
    /// The height of a sidebar entry.
    /// </summary>
    private const float GUI_SIDEBAR_ENTRY_HEIGHT = 45f;
    /// <summary>
    /// The space between sidebar entries.
    /// </summary>
    private const float GUI_SIDEBAR_ENTRY_PADDING_Y = 10;
	private const float GUI_SIDEBAR_PADDING_Y = 30;

    // Objects to allow rendering of GUI boxes with black backgrounds.
    #region make it black
    private Texture2D _black;
    private GUIStyle _blackBox;
    public Texture2D Black
    {
        get
        {
            if (_black == null)
            {
                _black = new Texture2D(1, 1);
                _black.SetPixel(0, 0, new Color(0, 0, 0));
                _black.Apply();
            }
            return _black;
        }
    }
    public GUIStyle BlackBoxStyle
    {
        get
        {
            if (_blackBox == null)
            {
                _blackBox = new GUIStyle(GUI.skin.box);
                _blackBox.normal.background = Black;
            }
            return _blackBox;
        }
    }
    #endregion
    #endregion

    /// <summary>
    /// All the entries on this sidebar.
    /// </summary>
    private KeyValuePair<string, Action>[] entries;

	/// <summary>
	/// The show GUI.
	/// </summary>
	public Action showGuiCallback = null;

	/// <summary>
	/// The hide GUI.
	/// </summary>
	public Action hideGuiCallback = null;

    /// <summary>
    /// All the overlay windows that are linked to this sidebar.
    /// </summary>
    private List<OverlayWindow> windows = new List<OverlayWindow>();

    /// <summary>
    /// Current intensity of the sidebar, [0-1].
    /// </summary>
    private float guiFadeIntensity = 0;
    /// <summary>
    /// Is the sidebar visible.
    /// </summary>
    public bool guiVisible = false;

	public bool guiBackgroundVisible = false;
	/// <summary>
	/// The fake esc pressed.
	/// </summary>
	private bool fakeEscPressed = false;

    /// <summary>
    /// Escape key state last time OnGUI was called.
    /// </summary>
    private bool keyDebounce = false;
    /// <summary>
    /// Does the sidebar width need recalculating.
    /// </summary>
    private volatile bool recalcWidth = false;
    /// <summary>
    /// The current sidebar width, pixels.  This is dynamically calculated.
    /// </summary>
    private float sidebarWidth = 100f;

	private float sidebarHeight = 0;

    /// <summary>
    /// Creates a GUI sidebar with an exit button.
    /// </summary>
    public GUIController()
    {
		recalcWidth = true;
    }

    /// <summary>
    /// Adds an overlay window to the sidebar.
    /// </summary>
    /// <param name="caption">The title of the sidebar entry</param>
    /// <param name="window">The window to control</param>
    /// <param name="onReturn">Optional callback on window close</param>
    public void AddWindow(string caption, OverlayWindow window, Action<object> onReturn)
    {
        windows.Add(window);
        AddAction(caption, () =>
        {
            bool state = window.Active;
            foreach (OverlayWindow win in windows)
            {
                win.Active = false;
            }
            window.Active = !state;
        });

        if (onReturn != null)
            window.OnComplete += onReturn;
    }

    /// <summary>
    /// Adds an entry to the sidebar.
    /// </summary>
    /// <param name="caption">The title of the entry</param>
    /// <param name="act">The action to execute when the entry is pressed</param>
    public void AddAction(string caption, Action act)
    {
		// Resizing for entries
        if (entries == null || entries.Length == 0)
        {
            entries = new KeyValuePair<string, Action>[1] { new KeyValuePair<string, Action>(caption, act) };
            return;
        }

        var res = new KeyValuePair<string, Action>[entries.Length + 1];
        Array.Copy(entries, res, entries.Length);
        res[res.Length - 1] = new KeyValuePair<string, Action>(caption, act);
        entries = res;
        recalcWidth = true;
    }

    /// <summary>
    /// Executes the action with the given title in the sidebar
    /// </summary>
    /// <param name="caption">The sidebar title</param>
    public void DoAction(string caption)
    {
        foreach (var v in entries)
        {
            if (v.Key.Equals(caption))
            {
                v.Value();
                break;
            }
        }
    }

    /// <summary>
    /// Renders the overlay.
    /// </summary>
    public void Render()
    {
        bool windowVisible = false;
        #region windowVisible
        foreach (OverlayWindow window in windows)
        {
            if (window.Active)
            {
                windowVisible = true;
                break;
            }
        }
        #endregion

        #region calculate width
        GUIStyle btnStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
        btnStyle.fontSize *= 3;
        if (recalcWidth)
        {
            recalcWidth = false;
            float width = -1;
            foreach (var btn in entries)
            {
                width = Math.Max(btnStyle.CalcSize(new GUIContent(btn.Key)).x, width);
            }
            sidebarWidth = width + 2 * GUI_SIDEBAR_PADDING.x;
			sidebarHeight = entries.Length * (GUI_SIDEBAR_ENTRY_HEIGHT + GUI_SIDEBAR_ENTRY_PADDING_Y) + GUI_SIDEBAR_ENTRY_PADDING_Y;
        }
        #endregion

        #region hotkeys
        {
            bool escPressed = Input.GetKeyDown(KeyCode.Escape) || fakeEscPressed;
			fakeEscPressed = false;

            if (escPressed && !keyDebounce)
            {
				// Hide all windows if gui is visible and windows are active
				if (guiVisible && windowVisible)
                {
                    foreach (OverlayWindow window in windows)
                    {
                        window.Active = false;
                    }
                }

				// Show/Hide the gui if no windows are active
                else
                {
                    guiVisible = !guiVisible;

					if(guiVisible && showGuiCallback != null)
						showGuiCallback.Invoke();

					else if(!guiVisible && hideGuiCallback != null)
						hideGuiCallback.Invoke();
                }
            }
            keyDebounce = escPressed;
        }
        #endregion
		guiFadeIntensity += ((guiVisible || guiBackgroundVisible) ? 1f : -1f) * Time.deltaTime / GUI_SHOW_TIME;
        guiFadeIntensity = Mathf.Clamp01(guiFadeIntensity);

        // Dims the background
        if (guiFadeIntensity > 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.75f * guiFadeIntensity);
			GUI.Box(new Rect(-10, -10, Screen.width + 20, Screen.height + 20), "", BlackBoxStyle);
        }

        UserMessageManager.Render();

        if (guiFadeIntensity > 0 && guiVisible)
        {
			float topOffset = (Screen.height - sidebarHeight) / 2.0f;
			GUI.BeginGroup(new Rect((1f - guiFadeIntensity) * -sidebarWidth, GUI_SIDEBAR_PADDING_Y, sidebarWidth, sidebarHeight));
			//GUI.BeginGroup(new Rect(0, 0, sidebarWidth, Screen.height * guiFadeIntensity));

            // Render sidebar
            {
                GUI.backgroundColor = new Color(1, 1, 1, 0.9f);
				GUI.Box(new Rect(-5, 0, sidebarWidth + 5, sidebarHeight), "");//, BlackBoxStyle);
            }

            #region Render entries
			float y = GUI_SIDEBAR_ENTRY_PADDING_Y;

            foreach (var btn in entries)
            {
                if (GUI.Button(new Rect(GUI_SIDEBAR_PADDING.x, y, sidebarWidth - GUI_SIDEBAR_PADDING.x * 2, GUI_SIDEBAR_ENTRY_HEIGHT), btn.Key, btnStyle))
				{
					foreach(OverlayWindow window in windows)
						window.Active = false;

                    btn.Value();
                }

                y += GUI_SIDEBAR_ENTRY_HEIGHT + GUI_SIDEBAR_ENTRY_PADDING_Y;
            }
            GUI.EndGroup();
            #endregion

            foreach (OverlayWindow window in windows)
            {
                window.Render();
            }
        }
    }

	/// <summary>
	/// Fake esc key press to trigger a real esc key press
	/// </summary>
	public void EscPressed()
	{
		fakeEscPressed = true;
	}

	/// <summary>
	/// Clickeds the inside window.
	/// </summary>
	/// <returns><c>true</c>, if inside window was clickeded, <c>false</c> otherwise.</returns>
	public bool ClickedInsideWindow()
	{
		float mouseX = Input.mousePosition.x;
		float mouseY = Screen.height - Input.mousePosition.y; // Convert mouse coordinates to unity window positions coordinates

		foreach(OverlayWindow win in windows)
			if((win.Active && auxFunctions.MouseInWindow(win.GetWindowRect())) || (mouseX < sidebarWidth && mouseY < GUI_SIDEBAR_PADDING_Y + sidebarHeight))
			   return true;

		return false;
	}

	/// <summary>
	/// Hides all windows.
	/// </summary>
	public void HideAllWindows()
	{
		foreach (OverlayWindow window in windows)
			window.Active = false;
	}

	/// <summary>
	/// Gets the width of the sidebar.
	/// </summary>
	/// <returns>The sidebar width.</returns>
	public float GetSidebarWidth()
	{
		return sidebarWidth;
	}
}