using UnityEngine;
using System;

/// <summary>
/// Simple yes/no dialog overlay.
/// Passes true on yes, false on no through the completion handler.
/// </summary>
class YesNoWindow : OverlayWindow
{
    private bool _active = false;
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
    /// Passes true on yes, false on no.
    /// </summary>
    public event Action<object> OnComplete;

    /// <summary>
    /// Dialog title.
    /// </summary>
    private readonly string title;

    /// <summary>
    /// Creates a yes/no dialog with the given title.
    /// </summary>
    /// <param name="title">The overlay title</param>
    public YesNoWindow(string title)
    {
        this.title = title;
    }

    /// <summary>
    /// Renders this overlay.
    /// </summary>
    public void Render()
    {
        if (_active)
        {
            GUI.Window(0, new Rect(Screen.width / 2 - 300, Screen.height / 2 - 100, 600, 200), (int windowID) =>
                {
                    if (GUI.Button(new Rect(50, 50, 175, 100), "No"))
                    {
                        _active = false;
                        if (OnComplete != null)
                            OnComplete(false);
                    }
                    if (GUI.Button(new Rect(350, 50, 175, 100), "Yes"))
                    {
                        _active = false;
                        if (OnComplete != null)
                            OnComplete(true);
                    }
                }, title);
        }
    }
}