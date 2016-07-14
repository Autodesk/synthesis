using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DialogWindow : OverlayWindow
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
	/// The windowRect.
	/// </summary>
	private Rect windowRect;
	
	/// <summary>
	/// Passes option selected.
	/// </summary>
	public event Action<object> OnComplete;

	/// <summary>
	/// Dialog title.
	/// </summary>
	private readonly string title;

	/// <summary>
	/// A list of the button labels.
	/// </summary>
	private readonly string[] labels;

    /// <summary>
    /// Creates a dialog with the given title and buttons with their corresponding labels.
    /// </summary>
    /// <param name="title">The overlay title</param>
    /// <param name="labels">The button labels</param> 
    public DialogWindow(string title, params string[] labels)
	{
		this.title = title;
		this.labels = labels;
	}
	
	/// <summary>
	/// Renders this overlay.
	/// </summary>
	public void Render()
	{
        windowRect = new Rect ((Screen.width / 2 - (50 + labels.Length * 250) / 2), Screen.height / 2 - 100, 50 + labels.Length * 250, 200);

		if (_active)
		{
			GUI.Window(0, windowRect, (int windowID) =>
			{
				for (int i = 0; i < labels.Length; i++)
				{
					if (GUI.Button(new Rect(50 + i * 250, 50, 200, 100), labels[i]))
					{
						_active = false;
						if (OnComplete != null)
							OnComplete(i);
					}
				}
			}, title);
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