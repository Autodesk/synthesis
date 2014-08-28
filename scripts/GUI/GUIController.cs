using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

class GUIController
{
    private const float GUI_SHOW_TIME = 0.5f;
    private float GUI_SIDEBAR_WIDTH = 100f;
    private static readonly Vector2 GUI_SIDEBAR_PADDING = new Vector2(10, 25);
    private const float GUI_SIDEBAR_ENTRY_HEIGHT = 45f;
    private const float GUI_SIDEBAR_ENTRY_PADDING_Y = 5;

    private FileBrowser fileBrowser = new FileBrowser();
    private bool exitWindowVisible = false;

    private float guiFadeIntensity = 0;
    public bool guiVisible = false;

    public event System.Action<string> OpenedRobot = null;

    #region windows
    private void InitExitWindow(int windowID)
    {
        if (GUI.Button(new Rect(50, 50, 175, 100), "No"))
        {
            exitWindowVisible = false;
        }
        else if (GUI.Button(new Rect(350, 50, 175, 100), "Yes"))
        {
            Application.Quit();
        }
    }
    #endregion

    private bool keyDebounce = false;
    private volatile bool recalcWidth = false;
    private KeyValuePair<string, Action>[] entries;

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

    public GUIController()
    {
        entries = new KeyValuePair<string, Action>[] { new KeyValuePair<string, Action>("Load Model", () => {
        fileBrowser.Active = true;
    }), new KeyValuePair<string, Action>("Exit", () => {
        exitWindowVisible = true; })};
        recalcWidth = true;
    }

    public void AddAction(string caption, Action act)
    {
        var res = new KeyValuePair<string, Action>[entries.Length + 1];
        Array.Copy(entries, res, entries.Length - 1);
        res[res.Length - 2] = new KeyValuePair<string, Action>(caption, act);
        res[res.Length - 1] = entries[entries.Length - 1];
        entries = res;
        recalcWidth = true;
    }

    public void ShowExit()
    {
        guiFadeIntensity = 1;
        guiVisible = true;
        fileBrowser.Active = false;
        exitWindowVisible = true;
    }

    public void ShowBrowser()
    {
        guiFadeIntensity = 1;
        guiVisible = true;
        fileBrowser.Active = true;
        exitWindowVisible = false;
    }


    public void Render()
    {
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
            GUI_SIDEBAR_WIDTH = width + 2 * GUI_SIDEBAR_PADDING.x;
        }
        #region hotkeys
        {
            bool escPressed = Input.GetKeyDown(KeyCode.Escape);
            if (escPressed && !keyDebounce)
            {
                if (guiVisible && exitWindowVisible)
                {
                    exitWindowVisible = false;
                }
                else if (guiVisible && fileBrowser.Active)
                {
                    fileBrowser.Active = false;
                }
                else
                {
                    guiVisible = !guiVisible;
                }
                if (!guiVisible)
                {
                    fileBrowser.Active = false;
                    exitWindowVisible = false;
                }
            }
            keyDebounce = escPressed;
        }
        #endregion


        bool overlayActive = !exitWindowVisible && !fileBrowser.Active;

        guiFadeIntensity += (guiVisible ? 1f : -1f) * Time.deltaTime / GUI_SHOW_TIME;
        guiFadeIntensity = Mathf.Clamp01(guiFadeIntensity);
        if (guiFadeIntensity > 0)
        {
            GUI.backgroundColor = new Color(1, 1, 1, 0.45f * guiFadeIntensity);
            GUI.Box(new Rect(-10, -10, Screen.width + 20, Screen.height + 20), "", BlackBoxStyle);
        }

        UserMessageManager.Render();

        if (guiFadeIntensity > 0)
        {
            GUI.BeginGroup(new Rect((1f - guiFadeIntensity) * -GUI_SIDEBAR_WIDTH, 0, GUI_SIDEBAR_WIDTH, Screen.height));

            // Only render if no overlay
            GUI.backgroundColor = new Color(1, 1, 1, 0.9f);
            GUI.Box(new Rect(-1, -10, GUI_SIDEBAR_WIDTH + 2, Screen.height + 20), "", BlackBoxStyle);

            float y = GUI_SIDEBAR_PADDING.y;
            foreach (var btn in entries)
            {
                if (GUI.Button(new Rect(GUI_SIDEBAR_PADDING.x, y, GUI_SIDEBAR_WIDTH - GUI_SIDEBAR_PADDING.x * 2, GUI_SIDEBAR_ENTRY_HEIGHT), btn.Key, btnStyle) && overlayActive)
                {
                    btn.Value();
                }
                y += GUI_SIDEBAR_ENTRY_HEIGHT + GUI_SIDEBAR_ENTRY_PADDING_Y;
            }
            GUI.EndGroup();

            if (fileBrowser.Active)
            {
                fileBrowser.Show();
            }
            if (fileBrowser.Submit)
            {
                fileBrowser.Active = false;
                fileBrowser.Submit = false;
                string fileLocation = fileBrowser.directoryLocation;
                // If dir was selected...
                if (File.Exists(fileLocation + "\\skeleton.bxdj"))
                    fileLocation += "\\skeleton.bxdj";
                DirectoryInfo parent = Directory.GetParent(fileLocation);
                if (parent != null && parent.Exists && File.Exists(parent.FullName + "\\skeleton.bxdj"))
                {
                    if (OpenedRobot != null)
                    {
                        OpenedRobot(parent.FullName + "\\");
                    }
                }
                else
                {
                    UserMessageManager.Dispatch("Invalid selection!");
                }
            }

            if (exitWindowVisible)
            {
                Rect window = new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 200);
                window = GUI.Window(0, window, InitExitWindow, "Exit?");
            }
        }
    }
}