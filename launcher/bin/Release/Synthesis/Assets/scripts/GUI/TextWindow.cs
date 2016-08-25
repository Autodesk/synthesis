using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Generic window with labels and buttons <see cref="GUIController"/>.
/// </summary>
public class TextWindow : OverlayWindow
{
	/// <summary>
	/// The _active.
	/// </summary>
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

	private float xPosFactor = 1;
	private float yPosFactor = 1;
	private int initScreenWidth;
	private int initScreenHeight;
	private float initWindowX;
	private float initWindowY;

	/// <summary>
	/// The title.
	/// </summary>
	private readonly string title;

	/// <summary>
	/// The rect.
	/// </summary>
	private Rect windowRect;

	/// <summary>
	/// The label titles.
	/// </summary>
	private readonly string[] labelTitles;

	/// <summary>
	/// The label rects.
	/// </summary>
	private readonly Rect[] labelRects;

	/// <summary>
	/// The button titles.
	/// </summary>
	private readonly string[] buttonTitles;

	/// <summary>
	/// The button rects.
	/// </summary>
	private readonly Rect[] buttonRects;

	/// <summary>
	/// Passes option selected.
	/// </summary>
	public event Action<object> OnComplete;

	/// <summary>
	/// Initializes a new instance of the <see cref="TextWindow"/> class.
	/// </summary>
	/// <param name="title">Title.</param>
	/// <param name="rect">Rect.</param>
	/// <param name="labelTitles">Label titles.</param>
	/// <param name="labelRects">Label rects.</param>
	/// <param name="buttonTitles">Button titles.</param>
	/// <param name="buttonRects">Button rects.</param>
	public TextWindow(string title, Rect rect, string[] labelTitles, Rect[] labelRects, string[] buttonTitles, Rect[] buttonRects)
	{
		this.title = title;
		this.windowRect = rect;
		this.labelTitles = labelTitles;
		this.labelRects = labelRects;
		this.buttonTitles = buttonTitles;
		this.buttonRects = buttonRects;
		initScreenWidth = Screen.width;
		initScreenHeight = Screen.height;
		initWindowX = windowRect.x;
		initWindowY = windowRect.y;
	}


	/// <summary>
	/// Renders this overlay
	/// </summary>
	public void Render()
	{
		// Scale factor for when the user changes the window dimensions
		xPosFactor = (Screen.width - initScreenWidth) / 2.0f;
		yPosFactor = (Screen.height - initScreenHeight) / 2.0f;
		windowRect.x = (initWindowX + xPosFactor);
		windowRect.y = initWindowY + yPosFactor;

		if (_active)
		{
			GUI.Window
			(
				0, 
				windowRect, 
				(int windowID) =>
	           	{
					// Display the labels
					for(int i = 0; i < labelTitles.Length; i++)
						GUI.Label(labelRects[i], labelTitles[i]);

					// Display the buttons
					for(int i = 0; i < buttonTitles.Length; i++)
					{
						if(GUI.Button (buttonRects[i], buttonTitles[i]))
						{
							//_active = false;

							if(OnComplete != null)
								OnComplete(i);
						}
					}
				},
				title
          	);
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