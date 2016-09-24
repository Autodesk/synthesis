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
	/// Default textures.
	/// </summary>
	private Texture2D buttonTexture;
    private Texture2D greyWindowTexture;
    private Texture2D darkGreyWindowTexture;
    private Texture2D lightGreyWindowTexture;
    private Texture2D transparentWindowTexture;
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
	private GUIStyle window;
    /// <summary>
    /// Custom GUIStyle for buttons.
    /// </summary>
    private GUIStyle button;

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
        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        russoOne = Resources.Load("Fonts/Russo_One") as Font;
        greyWindowTexture = Resources.Load("Images/greyBackground") as Texture2D;

        //Custom style for windows
        window = new GUIStyle(GUI.skin.window);
        window.normal.background = greyWindowTexture;
        window.onNormal.background = greyWindowTexture;
        window.font = russoOne;
        window.fontSize = 35;

        //Custom style for buttons
        button = new GUIStyle(GUI.skin.button);
        button.normal.background = buttonTexture;
        button.hover.background = buttonSelected;
        button.active.background = buttonSelected;
        button.onNormal.background = buttonSelected;
        button.onHover.background = buttonSelected;
        button.onActive.background = buttonSelected;
        button.font = russoOne;
        button.fontSize = 20;

        windowRect = new Rect ((Screen.width / 2 - (50 + labels.Length * 250) / 2), Screen.height / 2 - 100, 50 + labels.Length * 250, 200);

		if (_active)
		{
			GUI.Window(0, windowRect, (int windowID) =>
			{
				for (int i = 0; i < labels.Length; i++)
				{
					if (GUI.Button(new Rect(50 + i * 250, 85, 200, 50), labels[i], button))
					{
						_active = false;
						if (OnComplete != null)
							OnComplete(i);
					}
				}
			}, title, window);
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