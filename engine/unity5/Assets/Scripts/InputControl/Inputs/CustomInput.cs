using UnityEngine;



/// <summary>
/// <see cref="CustomInput"/> is an interface for handling some input device(keyboard, mouse, joystick).
/// Source: https://github.com/Gris87/InputControl
/// </summary>
public abstract class CustomInput
{
    protected KeyModifier mModifiers;

    private   string      mCachedModifiersToString = null;



    protected static int         mCachedModifiersFrame = 0;
    protected static KeyModifier mCachedModifiersState = KeyModifier.NoModifier;



    #region Properties

    #region modifiers
    /// <summary>
    /// Gets the key modifiers.
    /// </summary>
    /// <value>Key modifiers.</value>
    public KeyModifier modifiers
    {
        get
        {
            return mModifiers;
        }
    }
    #endregion

    #endregion



    /// <summary>
    /// Returns input value while the user holds down the key.
    /// </summary>
    /// <returns>Input value if button or axis is still active.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public abstract float getInput(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any);

    /// <summary>
    /// Returns input value during the frame the user starts pressing down the key.
    /// </summary>
    /// <returns>Input value if button or axis become active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public abstract float getInputDown(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any);

    /// <summary>
    /// Returns input value during the frame the user releases the key.
    /// </summary>
    /// <returns>Input value if button or axis stopped being active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public abstract float getInputUp(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any);

    /// <summary>
    /// Verifies that specified key modifiers are active during current frame.
    /// </summary>
    /// <returns>Specified key modifiers are active during current frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    protected bool checkModifiers(bool exactKeyModifiers = false)
    {
        if (!exactKeyModifiers && mModifiers == KeyModifier.NoModifier)
        {
            return true;
        }

        if (mCachedModifiersFrame != Time.frameCount)
        {
            KeyModifier res = KeyModifier.NoModifier;

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                res |= KeyModifier.Ctrl;
            }

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                res |= KeyModifier.Alt;
            }

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                res |= KeyModifier.Shift;
            }

            mCachedModifiersFrame = Time.frameCount;
            mCachedModifiersState = res;
        }

        if (exactKeyModifiers)
        {
            return mModifiers == mCachedModifiersState;
        }
        else
        {
            return (mModifiers & mCachedModifiersState) == mModifiers;
        }
    }

    /// <summary>
    /// Parse string argument and try to get key modifiers.
    /// </summary>
    /// <returns>Parsed key modifiers.</returns>
    /// <param name="value">String representation of key modifiers with the rest of CustomInput string representation.</param>
    protected static KeyModifier modifiersFromString(ref string value)
    {
        KeyModifier res = KeyModifier.NoModifier;

		if (value == null)
		{
			return res;
		}

        do
        {
            if (value.StartsWith("Ctrl+"))
            {
                value  = value.Substring(5);
                res   |= KeyModifier.Ctrl;

                continue;
            }

            if (value.StartsWith("Alt+"))
            {
                value  = value.Substring(4);
                res   |= KeyModifier.Alt;

                continue;
            }

            if (value.StartsWith("Shift+"))
            {
                value  = value.Substring(6);
                res   |= KeyModifier.Shift;

                continue;
            }

            break;
        } while(true);

        return res;
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents key modifiers.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents key modifiers.</returns>
    protected string modifiersToString()
    {
        if (mCachedModifiersToString == null)
        {
            string res = "";

            if ((mModifiers & KeyModifier.Ctrl) != 0)
            {
                res += "Ctrl+";
            }

            if ((mModifiers & KeyModifier.Alt) != 0)
            {
                res += "Alt+";
            }

            if ((mModifiers & KeyModifier.Shift) != 0)
            {
                res += "Shift+";
            }

            mCachedModifiersToString = res;
        }

        return mCachedModifiersToString;
    }
}
