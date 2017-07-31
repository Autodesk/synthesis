using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Controls
{
    #region Old Controls: 2017 and Older
    //public enum Control
    //{
    //    Forward, Backward, Right, Left, ResetRobot, CameraToggle, pwm2Plus, pwm2Neg, pwm3Plus, pwm3Neg, pwm4Plus,
    //    pwm4Neg, pwm5Plus, pwm5Neg, pwm6Plus, pwm6Neg, PickupPrimary, ReleasePrimary, SpawnPrimary, PickupSecondary,
    //    ReleaseSecondary, SpawnSecondary
    //};

    //public static KeyCode[] ControlKey = new KeyCode[23];
    //public static KeyCode[] BackupKeys = new KeyCode[23];
    //public static readonly string[] ControlName = { "Move Forward", "Move Backward", "Turn Right", "Turn Left", "Reset Robot",
    //                                                "Toggle Camera", "PWM 2 Positive", "PWM 2 Negative", "PWM 3 Positive",
    //                                                "PWM 3 Negative", "PWM 4 Positive", "PWM 4 Negative", "PWM 5 Positive",
    //                                                "PWM 5 Negative", "PWM 6 Positive", "PWM 6 Negative", "Pick Up Primary Gamepiece",
    //                                                "Release Primary Gamepiece", "Spawn Primary Gamepiece", "Pick Up Secondary Gamepiece",
    //                                                "Release Secondary Gamepiece", "Spawn Secondary Gamepiece"
    //};
    #endregion

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

        public Axis tankVertical;
    }

    /// <summary>
    /// Set of buttons.
    /// </summary>
    public static Buttons buttons;

    /// <summary>
    /// Set of axes.
    /// </summary>
    public static Axes axes;

    /// <summary>
    /// Initializes the <see cref="Controls"/> class.
    /// </summary>
    static Controls()
    {
        //Basic robot controls
        buttons.forward = InputControl.setKey("Forward", KeyCode.UpArrow, new JoystickInput(JoystickAxis.Axis2Negative));
        buttons.backward = InputControl.setKey("Backward", KeyCode.DownArrow, new JoystickInput(JoystickAxis.Axis2Positive));
        buttons.left = InputControl.setKey("Left", KeyCode.LeftArrow, new JoystickInput(JoystickAxis.Axis4Negative));
        buttons.right = InputControl.setKey("Right", KeyCode.RightArrow, new JoystickInput(JoystickAxis.Axis4Positive));

        //Remaining PWM controls
        buttons.pwm2Plus = InputControl.setKey("PWM 2 Positive", KeyCode.Alpha1, new JoystickInput(JoystickAxis.Axis3Positive));
        buttons.pwm2Neg = InputControl.setKey("PWM 2 Negative", KeyCode.Alpha2, new JoystickInput(JoystickAxis.Axis3Negative));
        buttons.pwm3Plus = InputControl.setKey("PWM 3 Positive", KeyCode.Alpha3, new JoystickInput(JoystickAxis.Axis5Positive));
        buttons.pwm3Neg = InputControl.setKey("PWM 3 Negative", KeyCode.Alpha4, new JoystickInput(JoystickAxis.Axis5Negative));
        buttons.pwm4Plus = InputControl.setKey("PWM 4 Positive", KeyCode.Alpha5, new JoystickInput(JoystickAxis.Axis6Positive));
        buttons.pwm4Neg = InputControl.setKey("PWM 4 Negative", KeyCode.Alpha6, new JoystickInput(JoystickAxis.Axis6Negative));
        buttons.pwm5Plus = InputControl.setKey("PWM 5 Positive", KeyCode.Alpha7, new JoystickInput(JoystickAxis.Axis7Positive));
        buttons.pwm5Neg = InputControl.setKey("PWM 5 Negative", KeyCode.Alpha8, new JoystickInput(JoystickAxis.Axis7Negative));
        buttons.pwm6Plus = InputControl.setKey("PWM 6 Positive", KeyCode.Alpha9, new JoystickInput(JoystickAxis.Axis8Positive));
        buttons.pwm6Neg = InputControl.setKey("PWM 6 Negative", KeyCode.Alpha0, new JoystickInput(JoystickAxis.Axis8Negative));

        //Other Controls
        buttons.resetRobot = InputControl.setKey("Reset Robot", KeyCode.R, new JoystickInput(JoystickButton.Button1));
        buttons.cameraToggle = InputControl.setKey("Camera Toggle", KeyCode.C, new JoystickInput(JoystickButton.Button2));
        buttons.pickupPrimary = InputControl.setKey("Pick Up Primary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3));
        buttons.releasePrimary = InputControl.setKey("Release Primary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4));
        buttons.spawnPrimary = InputControl.setKey("Spawn Primary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5));
        buttons.pickupSecondary = InputControl.setKey("Pick Up Secondary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3));
        buttons.releaseSecondary = InputControl.setKey("Release Secondary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4));
        buttons.spawnSecondary = InputControl.setKey("Spawn Secondary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5));

        //Set axes
        axes.horizontal = InputControl.setAxis("Horizontal", buttons.left, buttons.right);
        axes.vertical = InputControl.setAxis("Vertical", buttons.backward, buttons.forward);

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

    /// <summary>
    /// Resets to default controls.
    /// </summary>
    public static void Reset()
    {
        //Basic robot controls
        buttons.forward = InputControl.setKey("Forward", KeyCode.UpArrow, new JoystickInput(JoystickAxis.Axis2Negative));
        buttons.backward = InputControl.setKey("Backward", KeyCode.DownArrow, new JoystickInput(JoystickAxis.Axis2Positive));
        buttons.left = InputControl.setKey("Left", KeyCode.LeftArrow, new JoystickInput(JoystickAxis.Axis4Negative));
        buttons.right = InputControl.setKey("Right", KeyCode.RightArrow, new JoystickInput(JoystickAxis.Axis4Positive));

        //Remaining PWM controls
        buttons.pwm2Plus = InputControl.setKey("PWM 2 Positive", KeyCode.Alpha1, new JoystickInput(JoystickAxis.Axis3Positive));
        buttons.pwm2Neg = InputControl.setKey("PWM 2 Negative", KeyCode.Alpha2, new JoystickInput(JoystickAxis.Axis3Negative));
        buttons.pwm3Plus = InputControl.setKey("PWM 3 Positive", KeyCode.Alpha3, new JoystickInput(JoystickAxis.Axis5Positive));
        buttons.pwm3Neg = InputControl.setKey("PWM 3 Negative", KeyCode.Alpha4, new JoystickInput(JoystickAxis.Axis5Negative));
        buttons.pwm4Plus = InputControl.setKey("PWM 4 Positive", KeyCode.Alpha5, new JoystickInput(JoystickAxis.Axis6Positive));
        buttons.pwm4Neg = InputControl.setKey("PWM 4 Negative", KeyCode.Alpha6, new JoystickInput(JoystickAxis.Axis6Negative));
        buttons.pwm5Plus = InputControl.setKey("PWM 5 Positive", KeyCode.Alpha7, new JoystickInput(JoystickAxis.Axis7Positive));
        buttons.pwm5Neg = InputControl.setKey("PWM 5 Negative", KeyCode.Alpha8, new JoystickInput(JoystickAxis.Axis7Negative));
        buttons.pwm6Plus = InputControl.setKey("PWM 6 Positive", KeyCode.Alpha9, new JoystickInput(JoystickAxis.Axis8Positive));
        buttons.pwm6Neg = InputControl.setKey("PWM 6 Negative", KeyCode.Alpha0, new JoystickInput(JoystickAxis.Axis8Negative));

        //Other Controls
        buttons.resetRobot = InputControl.setKey("Reset Robot", KeyCode.R, new JoystickInput(JoystickButton.Button1));
        buttons.cameraToggle = InputControl.setKey("Camera Toggle", KeyCode.C, new JoystickInput(JoystickButton.Button2));
        buttons.pickupPrimary = InputControl.setKey("Pick Up Primary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3));
        buttons.releasePrimary = InputControl.setKey("Release Primary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4));
        buttons.spawnPrimary = InputControl.setKey("Spawn Primary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5));
        buttons.pickupSecondary = InputControl.setKey("Pick Up Secondary Gamepiece", KeyCode.X, new JoystickInput(JoystickButton.Button3));
        buttons.releaseSecondary = InputControl.setKey("Release Secondary Gamepiece", KeyCode.E, new JoystickInput(JoystickButton.Button4));
        buttons.spawnSecondary = InputControl.setKey("Spawn Secondary Gamepiece", KeyCode.Q, new JoystickInput(JoystickButton.Button5));

        //Set axes
        axes.horizontal = InputControl.setAxis("Joystick Horizontal", buttons.left, buttons.right);
        axes.vertical = InputControl.setAxis("Joystick Vertical", buttons.backward, buttons.forward);

        GameObject.Find("SettingsMode").GetComponent<SettingsMode>().UpdateAllText();
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
#region Old Controls: 2016 and Older

//public static void ResetDefaults()
//{
//    ControlKey[(int)Control.Forward] = KeyCode.UpArrow;
//    ControlKey[(int)Control.Backward] = KeyCode.DownArrow;
//    ControlKey[(int)Control.Right] = KeyCode.RightArrow;
//    ControlKey[(int)Control.Left] = KeyCode.LeftArrow;
//    ControlKey[(int)Control.ResetRobot] = KeyCode.R;
//    ControlKey[(int)Control.CameraToggle] = KeyCode.C;
//    ControlKey[(int)Control.pwm2Plus] = KeyCode.Alpha1;
//    ControlKey[(int)Control.pwm2Neg] = KeyCode.Alpha2;
//    ControlKey[(int)Control.pwm3Plus] = KeyCode.Alpha3;
//    ControlKey[(int)Control.pwm3Neg] = KeyCode.Alpha4;
//    ControlKey[(int)Control.pwm4Plus] = KeyCode.Alpha5;
//    ControlKey[(int)Control.pwm4Neg] = KeyCode.Alpha6;
//    ControlKey[(int)Control.pwm5Plus] = KeyCode.Alpha7;
//    ControlKey[(int)Control.pwm5Neg] = KeyCode.Alpha8;
//    ControlKey[(int)Control.pwm6Plus] = KeyCode.Alpha9;
//    ControlKey[(int)Control.pwm6Neg] = KeyCode.Alpha0;
//    ControlKey[(int)Control.PickupPrimary] = KeyCode.X;
//    ControlKey[(int)Control.ReleasePrimary] = KeyCode.E;
//    ControlKey[(int)Control.SpawnPrimary] = KeyCode.Q;
//    ControlKey[(int)Control.PickupSecondary] = KeyCode.X;
//    ControlKey[(int)Control.ReleaseSecondary] = KeyCode.E;
//    ControlKey[(int)Control.SpawnSecondary] = KeyCode.Q;
//}

//public static void LoadControls()
//{
//    for (int i = 0; i < ControlKey.Length; i++)
//    {
//        if (PlayerPrefs.HasKey("ControlKey" + i.ToString())) ControlKey[i] = (KeyCode)PlayerPrefs.GetInt("ControlKey" + i.ToString());
//    }
//}

//public static void SaveControls()
//{
//    for (int i = 0; i < ControlKey.Length; i++)
//    {
//        PlayerPrefs.SetInt("ControlKey" + i.ToString(), (int)ControlKey[i]);
//    }
//    PlayerPrefs.Save();
//}

//public static bool SetControl(int control, KeyCode key)
//{
//    ControlKey[control] = key;
//    for (int i = 0; i < ControlKey.Length; i++)
//    {
//        if (i != control && ControlKey[i] == key)
//        {
//            return false;
//        }
//    }
//    return true;
//}

//public static bool CheckConflict()
//{
//    for (int i = 0; i < ControlKey.Length; i++)
//    {
//        for (int j = 1; j < ControlKey.Length; j++)
//        {
//            if (j != i && ControlKey[i] == ControlKey[j]) return true;
//        }
//    }
//    return false;
//}
#endregion