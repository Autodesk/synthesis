using UnityEngine;
using System;



/// <summary>
/// <see cref="JoystickInput"/> handles joystick input device.
/// Source: https://github.com/Gris87/InputControl
/// </summary>
public class JoystickInput : CustomInput
{
    private JoystickAxis   mAxis;
    private JoystickButton mButton;
    private Joystick       mTarget;

    private string         mCachedToString;
    private string         mCachedInputName;



    #region Properties

    #region axis
    /// <summary>
    /// Gets the joystick axis.
    /// </summary>
    /// <value>Joystick axis.</value>
    public JoystickAxis axis
    {
        get
        {
            return mAxis;
        }
    }
    #endregion

    #region button
    /// <summary>
    /// Gets the joystick button.
    /// </summary>
    /// <value>Joystick button.</value>
    public JoystickButton button
    {
        get
        {
            return mButton;
        }
    }
    #endregion

    #region target
    /// <summary>
    /// Gets the target joystick.
    /// </summary>
    /// <value>Target joystick.</value>
    public Joystick target
    {
        get
        {
            return mTarget;
        }
    }
    #endregion

    #endregion



    /// <summary>
    /// Create a new instance of <see cref="JoystickInput"/> that handles specified joystick axis for a target joystick.
    /// </summary>
    /// <param name="axis">Joystick axis.</param>
    /// <param name="target">Target joystick.</param>
    /// <param name="modifiers">Key modifiers.</param>
    public JoystickInput(JoystickAxis axis, Joystick target = Joystick.AllJoysticks, KeyModifier modifiers = KeyModifier.NoModifier)
    {
        if (axis == JoystickAxis.None)
        {
            Debug.LogError("axis can't be JoystickAxis.None");
        }

        mAxis      = axis;
        mButton    = JoystickButton.None;
        mTarget    = target;
        mModifiers = modifiers;

        mCachedToString  = null;
        mCachedInputName = null;
    }

    /// <summary>
    /// Create a new instance of <see cref="JoystickInput"/> that handles specified joystick button for a target joystick.
    /// </summary>
    /// <param name="button">Joystick button.</param>
    /// <param name="target">Target joystick.</param>
    /// <param name="modifiers">Key modifiers.</param>
    public JoystickInput(JoystickButton button, Joystick target = Joystick.AllJoysticks, KeyModifier modifiers = KeyModifier.NoModifier)
    {
        if (button == JoystickButton.None)
        {
            Debug.LogError("button can't be JoystickButton.None");
        }

        mAxis      = JoystickAxis.None;
        mButton    = button;
        mTarget    = target;
        mModifiers = modifiers;

        mCachedToString  = null;
        mCachedInputName = null;
    }

    /// <summary>
    /// Parse string argument and try to create <see cref="JoystickInput"/> instance.
    /// </summary>
    /// <returns>Parsed JoystickInput.</returns>
    /// <param name="value">String representation of JoystickInput.</param>
    public static JoystickInput FromString(string value)
    {
		if (value == null)
		{
			return null;
		}

        KeyModifier modifiers = modifiersFromString(ref value);

        if (!value.StartsWith("Joystick "))
        {
            return null;
        }

        value = value.Substring(9);

        if (value.Length == 0)
        {
            return null;
        }

        Joystick target;

        if (value[0] >= '0' && value[0] <= '9')
        {
            int index = value.IndexOf(" ");

            if (index < 0)
            {
                return null;
            }

            try
            {
                int targetNumber = Convert.ToInt32(value.Substring(0, index));

                if (
                    targetNumber < 1
                    ||
                    targetNumber >= (int)Joystick.None
                   )
                {
                    return null;
                }

                target = (Joystick)targetNumber;
            }
            catch (Exception)
            {
                return null;
            }

            value = value.Substring(index + 1);
        }
        else
        {
            target = Joystick.AllJoysticks;
        }

        if (value.StartsWith("Axis "))
        {
            value = value.Substring(5);

            bool positive;

            if (value.EndsWith(" (-)"))
            {
                positive = false;
            }
            else
            if (value.EndsWith(" (+)"))
            {
                positive = true;
            }
            else
            {
                return null;
            }

            value = value.Remove(value.Length - 4);

            try
            {
                int axisNumber = (Convert.ToInt32(value) - 1) * 2;

                if (!positive)
                {
                    ++axisNumber;
                }

                if (
                    axisNumber < 0
                    ||
                    axisNumber >= (int)JoystickAxis.None
                   )
                {
                    return null;
                }

                return new JoystickInput((JoystickAxis)axisNumber, target, modifiers);
            }
            catch (Exception)
            {
                return null;
            }
        }

        if (!value.StartsWith("Button "))
        {
            return null;
        }

        value = value.Substring(7);

        try
        {
            int button = Convert.ToInt32(value) - 1;

            if (
                button < 0
                ||
                button >= (int)JoystickButton.None
               )
            {
                return null;
            }

            return new JoystickInput((JoystickButton)button, target, modifiers);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Returns a <see cref="System.String"/> that represents the current <see cref="JoystickInput"/>.
    /// </summary>
    /// <returns>A <see cref="System.String"/> that represents the current <see cref="JoystickInput"/>.</returns>
    public override string ToString()
    {
        if (mCachedToString == null)
        {
            string res = modifiersToString() + "Joystick ";

            if (mTarget != Joystick.AllJoysticks)
            {
                res += ((int)mTarget).ToString() + " ";
            }

            if (mAxis != JoystickAxis.None)
            {
                int  axisId = (int)mAxis;
                bool positive;

                if (axisId % 2 == 0)
                {
                    axisId   = (axisId / 2) + 1;
                    positive = true;
                }
                else
                {
                    axisId   = ((axisId - 1) / 2) + 1;
                    positive = false;
                }

                res += "Axis " + axisId.ToString() + " " + (positive ? "(+)" : "(-)");
            }
            else
            if (mButton != JoystickButton.None)
            {
                res += "Button " + ((int)mButton + 1).ToString();
            }

            mCachedToString = res;
        }

        return mCachedToString;
    }

    /// <summary>
    /// Returns input value while the user holds down the key.
    /// </summary>
    /// <returns>Input value if button or axis is still active.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public override float getInput(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        if (
            device != InputDevice.Any
            &&
            device != InputDevice.Joystick
            ||
            !checkModifiers(exactKeyModifiers)
           )
        {
            return 0;
        }

        float sensitivity = 1;

        if (
            axis != null
            &&
            (
             axis.Equals("Mouse X")
             ||
             axis.Equals("Mouse Y")
            )
           )
        {
            sensitivity = 0.1f;
        }

        if (mButton != JoystickButton.None)
        {
            return Input.GetButton(getInputName()) ? sensitivity : 0;
        }

        return getInputByAxis() * sensitivity;
    }

    /// <summary>
    /// Returns input value during the frame the user starts pressing down the key.
    /// </summary>
    /// <returns>Input value if button or axis become active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public override float getInputDown(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        if (
            device != InputDevice.Any
            &&
            device != InputDevice.Joystick
            ||
            !checkModifiers(exactKeyModifiers)
           )
        {
            return 0;
        }

        float sensitivity = 1;

        if (
            axis != null
            &&
            (
            axis.Equals("Mouse X")
            ||
            axis.Equals("Mouse Y")
            )
            )
        {
            sensitivity = 0.1f;
        }

        if (mButton != JoystickButton.None)
        {
            return Input.GetButtonDown(getInputName()) ? sensitivity : 0;
        }

        return getInputByAxis() * sensitivity;
    }

    /// <summary>
    /// Returns input value during the frame the user releases the key.
    /// </summary>
    /// <returns>Input value if button or axis stopped being active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public override float getInputUp(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        if (
            device != InputDevice.Any
            &&
            device != InputDevice.Joystick
            ||
            !checkModifiers(exactKeyModifiers)
           )
        {
            return 0;
        }

        float sensitivity = 1;

        if (
            axis != null
            &&
            (
            axis.Equals("Mouse X")
            ||
            axis.Equals("Mouse Y")
            )
            )
        {
            sensitivity = 0.1f;
        }

        if (mButton != JoystickButton.None)
        {
            return Input.GetButtonUp(getInputName()) ? sensitivity : 0;
        }

        return getInputByAxis() * sensitivity;
    }

    /// <summary>
    /// Calls Input.GetAxis for a specified joystick axis.
    /// </summary>
    /// <returns>Value of joystick axis.</returns>
    private float getInputByAxis()
    {
        float joyAxis = Input.GetAxis(getInputName());

        if (
            ((int)mAxis) % 2 == 1
            &&
            joyAxis < -InputControl.joystickThreshold
           )
        {
            return -joyAxis;
        }

        if (
            ((int)mAxis) % 2 == 0
            &&
            joyAxis > InputControl.joystickThreshold
           )
        {
            return joyAxis;
        }

        return 0;
    }

    /// <summary>
    /// Returns the name of input in InputManager according to the attributes.
    /// </summary>
    /// <returns>Input name in InputManager.</returns>
    private string getInputName()
    {
        if (mCachedInputName == null)
        {
            string res;

            if (mTarget == Joystick.AllJoysticks)
            {
                res = "Joystick ";
            }
            else
            {
                res = "Joystick " + ((int)mTarget).ToString() + " ";
            }

            if (mAxis != JoystickAxis.None)
            {
                int axisId = (int)mAxis;

                if (axisId % 2 == 0)
                {
                    axisId = (axisId / 2) + 1;
                }
                else
                {
                    axisId = ((axisId - 1) / 2) + 1;
                }

                res += "Axis " + axisId.ToString();
            }
            else
            if (mButton != JoystickButton.None)
            {
                res += "Button " + ((int)mButton + 1).ToString();
            }

            mCachedInputName = res;
        }

        return mCachedInputName;
    }
}
