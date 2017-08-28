/// <summary>
/// <see cref="KeyMapping"/> is a named handler for 3 <see cref="CustomInput"/>.
/// Source: https://github.com/Gris87/InputControl
/// </summary>
public class KeyMapping
{
    private string      mName;
    private CustomInput mPrimaryInput;
    private CustomInput mSecondaryInput;
    private CustomInput mThirdInput;



    #region Properties

    #region name
    /// <summary>
    /// Gets the <see cref="KeyMapping"/> name.
    /// </summary>
    /// <value>KeyMapping name.</value>
    public string name
    {
        get
        {
            return mName;
        }
    }
    #endregion

    #region primaryInput
    /// <summary>
    /// Gets or sets the primary input. Please note that if you set null value it will create KeyboardInput with KeyCode.None
    /// </summary>
    /// <value>Primary input.</value>
    public CustomInput primaryInput
    {
        get
        {
            return mPrimaryInput;
        }

        set
        {
            if (value == null)
            {
                mPrimaryInput = new KeyboardInput();
            }
            else
            {
                mPrimaryInput = value;
            }
        }
    }
    #endregion

    #region secondaryInput
    /// <summary>
    /// Gets or sets the secondary input. Please note that if you set null value it will create KeyboardInput with KeyCode.None
    /// </summary>
    /// <value>Secondary input.</value>
    public CustomInput secondaryInput
    {
        get
        {
            return mSecondaryInput;
        }

        set
        {
            if (value == null)
            {
                mSecondaryInput = new KeyboardInput();
            }
            else
            {
                mSecondaryInput = value;
            }
        }
    }
    #endregion

    #region thirdInput
    /// <summary>
    /// Gets or sets the third input. Please note that if you set null value it will create KeyboardInput with KeyCode.None
    /// </summary>
    /// <value>Third input.</value>
    public CustomInput thirdInput
    {
        get
        {
            return mThirdInput;
        }

        set
        {
            if (value == null)
            {
                mThirdInput = new KeyboardInput();
            }
            else
            {
                mThirdInput = value;
            }
        }
    }
    #endregion

    #endregion



    /// <summary>
    /// Create a new instance of <see cref="KeyMapping"/> with 3 specified <see cref="CustomInput"/>.
    /// </summary>
    /// <param name="name">KeyMapping name.</param>
    /// <param name="primaryCustomInput">Primary input.</param>
    /// <param name="secondaryCustomInput">Secondary input.</param>
    /// <param name="thirdCustomInput">Third input.</param>
    public KeyMapping(string name = "", CustomInput primaryCustomInput = null, CustomInput secondaryCustomInput = null, CustomInput thirdCustomInput = null)
    {
        mName          = name;
        primaryInput   = primaryCustomInput;
        secondaryInput = secondaryCustomInput;
        thirdInput     = thirdCustomInput;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyMapping"/> class based on another instance.
    /// </summary>
    /// <param name="another">Another KeyMapping instance.</param>
    public KeyMapping(KeyMapping another)
    {
        mName = another.mName;

        set(another);
    }

    /// <summary>
    /// Set the same <see cref="CustomInput"/> as in another instance.
    /// </summary>
    /// <param name="another">Another KeyMapping instance.</param>
    public void set(KeyMapping another)
    {
        mPrimaryInput   = another.mPrimaryInput;
        mSecondaryInput = another.mSecondaryInput;
        mThirdInput     = another.mThirdInput;
    }

    /// <summary>
    /// Returns input value while the user holds down the key.
    /// </summary>
    /// <returns>Input value if button or axis is still active.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public float getValue(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        float res = 0;
        float cur;

        cur = mPrimaryInput.getInput(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        cur = mSecondaryInput.getInput(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        cur = mThirdInput.getInput(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        return res;
    }

    /// <summary>
    /// Returns input value during the frame the user starts pressing down the key.
    /// </summary>
    /// <returns>Input value if button or axis become active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public float getValueDown(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        float res = 0;
        float cur;

        cur = mPrimaryInput.getInputDown(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        cur = mSecondaryInput.getInputDown(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        cur = mThirdInput.getInputDown(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        return res;
    }

    /// <summary>
    /// Returns input value during the frame the user releases the key.
    /// </summary>
    /// <returns>Input value if button or axis stopped being active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="axis">Specific actions for axis (Empty by default).</param>
    /// <param name="device">Preferred input device.</param>
    public float getValueUp(bool exactKeyModifiers = false, string axis = "", InputDevice device = InputDevice.Any)
    {
        float res = 0;
        float cur;

        cur = mPrimaryInput.getInputUp(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        cur = mSecondaryInput.getInputUp(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        cur = mThirdInput.getInputUp(exactKeyModifiers, axis, device);

        if (cur > res)
        {
            res = cur;
        }

        return res;
    }

    /// <summary>
    /// Returns true while the user holds down the key.
    /// </summary>
    /// <returns>True if button or axis is still active.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="device">Preferred input device.</param>
    public bool isPressed(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
    {
        return getValue(exactKeyModifiers, "", device) != 0;
    }

    /// <summary>
    /// Returns true during the frame the user starts pressing down the key.
    /// </summary>
    /// <returns>True if button or axis become active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="device">Preferred input device.</param>
    public bool isPressedDown(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
    {
        return getValueDown(exactKeyModifiers, "", device) != 0;
    }

    /// <summary>
    /// Returns true during the frame the user releases the key.
    /// </summary>
    /// <returns>True if button or axis stopped being active during this frame.</returns>
    /// <param name="exactKeyModifiers">If set to <c>true</c> check that only specified key modifiers are active, otherwise check that at least specified key modifiers are active.</param>
    /// <param name="device">Preferred input device.</param>
    public bool isPressedUp(bool exactKeyModifiers = false, InputDevice device = InputDevice.Any)
    {
        return getValueUp(exactKeyModifiers, "", device) != 0;
    }
}
