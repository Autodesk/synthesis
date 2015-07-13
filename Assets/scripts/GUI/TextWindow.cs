using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Generic interface representing an overlay shown by <see cref="GUIController"/>.
/// </summary>
public class TextWindow : OverlayWindow
{
	private bool _active = false;

	/// <summary>
	/// Is this overlay currently visible.
	/// </summary>
	/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
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

	private readonly string title;
	private readonly Rect rect;
	private readonly string[] labelTitles;
	private readonly Rect[] labelRects;

	/// <summary>
	/// Passes option selected.
	/// </summary>
	public event Action<object> OnComplete;

	public TextWindow(string title, Rect rect, string[] labelTitles, Rect[] labelRects)
	{
		this.title = title;
		this.rect = rect;
		this.labelTitles = labelTitles;
		this.labelRects = labelRects;
	}


	/// <summary>
	/// Renders this overlay
	/// </summary>
	public void Render()
	{
		if (_active)
		{
			GUI.Window
			(
				1, 
				rect, 
				(int windowID) =>
	           	{
					for(int i = 0; i < labelTitles.Length; i++)
					{
						GUI.Label(labelRects[i], labelTitles[i]);
					}
				},
				title
          	);
		}
	}
}