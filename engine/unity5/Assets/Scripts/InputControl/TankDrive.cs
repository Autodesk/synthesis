using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class TankDrive
{
    private static bool tankDrive = true;
    /// <summary>
    /// <see cref="Buttons"/> is a set of user defined buttons.
    /// </summary>
    public struct Buttons
    {
        //Basic robot controls
        public KeyMapping forward;
        public KeyMapping backward;
        public KeyMapping left;
        public KeyMapping right;

        //Tank drive controls
        public KeyMapping tankFrontLeft;
        public KeyMapping tankBackLeft;
        public KeyMapping tankFrontRight;
        public KeyMapping tankBackRight;

        //Remaining PWM Controls
        public KeyMapping pwm2Plus;
        public KeyMapping pwm2Neg;
        public KeyMapping pwm3Plus;
        public KeyMapping pwm3Neg;
        public KeyMapping pwm4Plus;
        public KeyMapping pwm4Neg;
        public KeyMapping pwm5Plus;
        public KeyMapping pwm5Neg;
        public KeyMapping pwm6Plus;
        public KeyMapping pwm6Neg;

        //Other controls
        public KeyMapping resetRobot;
        public KeyMapping cameraToggle;
        public KeyMapping pickupPrimary;
        public KeyMapping releasePrimary;
        public KeyMapping spawnPrimary;
        public KeyMapping pickupSecondary;
        public KeyMapping releaseSecondary;
        public KeyMapping spawnSecondary;
    }

    /// <summary>
    /// <see cref="Axes"/> is a set of user defined axes.
    /// </summary>
    public struct Axes
    {
        public Axis vertical;
        public Axis horizontal;
    }

    /// <summary>
    /// Set of buttons.
    /// </summary>
    public static Buttons buttons;

    /// <summary>
    /// Set of axes.
    /// </summary>
    public static Axes axes;

    static TankDrive()
    {
        //Tank Drive
        //buttons.tankFrontLeft = InputControl.setKey("Tank Front Left", new JoystickInput(JoystickAxis.Axis9Negative));
        //buttons.tankBackLeft = InputControl.setKey("Tank Back Left", new JoystickInput(JoystickAxis.Axis9Positive));
        //buttons.tankFrontRight = InputControl.setKey("Tank Front Right", new JoystickInput(JoystickAxis.Axis10Negative));
        //buttons.tankBackRight = InputControl.setKey("Tank Back Right", new JoystickInput(JoystickAxis.Axis10Positive));

 

        Load();
    }

    /// <summary>
    /// Nothing. It just call static constructor if needed.
    /// </summary>
    public static void Init()
    {
        // Nothing. It just call static constructor if needed
    }

    /// <summary>
    /// Save controls.
    /// </summary>
    public static void Save()
    {
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach (KeyMapping key in keys)
        {
            PlayerPrefs.SetString("Controls." + key.name + ".primary", key.primaryInput.ToString());
            PlayerPrefs.SetString("Controls." + key.name + ".secondary", key.secondaryInput.ToString());
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load controls.
    /// </summary>
    public static void Load()
    {
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach (KeyMapping key in keys)
        {
            string inputStr;

            inputStr = PlayerPrefs.GetString("Controls." + key.name + ".primary");

            if (inputStr != "")
            {
                key.primaryInput = customInputFromString(inputStr);
            }

            inputStr = PlayerPrefs.GetString("Controls." + key.name + ".secondary");

            if (inputStr != "")
            {
                key.secondaryInput = customInputFromString(inputStr);
            }
        }
    }

    public static void StartTankDrive()
    {
        //Tank Drive
        buttons.tankFrontLeft = InputControl.setKey("Tank Front Left", KeyCode.None, new JoystickInput(JoystickAxis.Axis9Negative));
        buttons.tankBackLeft = InputControl.setKey("Tank Back Left", KeyCode.None, new JoystickInput(JoystickAxis.Axis9Positive));
        buttons.tankFrontRight = InputControl.setKey("Tank Front Right", KeyCode.None, new JoystickInput(JoystickAxis.Axis10Negative));
        buttons.tankBackRight = InputControl.setKey("Tank Back Right", KeyCode.None, new JoystickInput(JoystickAxis.Axis10Positive));

        GameObject.Find("TankMode").GetComponent<TankMode>().UpdateAllText();
    }

    /// <summary>
    /// Converts string representation of CustomInput to CustomInput.
    /// </summary>
    /// <returns>CustomInput from string.</returns>
    /// <param name="value">String representation of CustomInput.</param>
    private static CustomInput customInputFromString(string value)
    {
        CustomInput res;

        res = JoystickInput.FromString(value);

        if (res != null)
        {
            return res;
        }

        res = MouseInput.FromString(value);

        if (res != null)
        {
            return res;
        }

        res = KeyboardInput.FromString(value);

        if (res != null)
        {
            return res;
        }

        return null;
    }
}
