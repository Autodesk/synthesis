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
	/// Custom GUIStyle for labels.
	/// </summary>s
	private GUIStyle label;

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
        //Loads textures and fonts
        buttonTexture = Resources.Load("Images/greyButton") as Texture2D;
        buttonSelected = Resources.Load("Images/selectedbuttontexture") as Texture2D;
        gravityRegular = Resources.Load("Fonts/Gravity-Regular") as Font;
        greyWindowTexture = Resources.Load("Images/greyBackground") as Texture2D;

        //Custom style for windows
        window = new GUIStyle(GUI.skin.window);
        window.normal.background = greyWindowTexture;
        window.onNormal.background = greyWindowTexture;
        window.font = gravityRegular;
        window.fontSize = 16;
        
        //Custom style for buttons
        button = new GUIStyle(GUI.skin.button);
        button.normal.background = buttonTexture;
        button.hover.background = buttonSelected;
        button.active.background = buttonSelected;
        button.onNormal.background = buttonSelected;
        button.onHover.background = buttonSelected;
        button.onActive.background = buttonSelected;
        button.fontSize = 13;

        //Custom style for labels
        label = new GUIStyle(GUI.skin.label);
        label.font = gravityRegular;
        label.fontSize = 13;

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
						GUI.Label(labelRects[i], labelTitles[i], label);

					// Display the buttons
					for(int i = 0; i < buttonTitles.Length; i++)
					{
						if(GUI.Button (buttonRects[i], buttonTitles[i], button))
						{
							//_active = false;

							if(OnComplete != null)
								OnComplete(i);
						}
					}
				},
				title,
                window
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