using UnityEngine;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Controls
{
    public enum Control { Forward, Backward, Right, Left, ResetRobot, CameraToggle, pwm2Plus, pwm2Neg, pwm3Plus, pwm3Neg, pwm4Plus, pwm4Neg, pwm5Plus, pwm5Neg, pwm6Plus, pwm6Neg, PickupPrimary, ReleasePrimary, SpawnPrimary, PickupSecondary, ReleaseSecondary, SpawnSecondary };

    public static KeyCode[] ControlKey = new KeyCode[23];
    public static KeyCode[] BackupKeys = new KeyCode[23];
    public static readonly string[] ControlName = { "Move Forward", "Move Backward", "Turn Right", "Turn Left", "Reset Robot", "Toggle Camera", "PWM 2 Positive", "PWM 2 Negative", "PWM 3 Positive", "PWM 3 Negative", "PWM 4 Positive", "PWM 4 Negative", "PWM 5 Positive", "PWM 5 Negative", "PWM 6 Positive", "PWM 6 Negative", "Pick Up Primary Gamepiece", "Release Primary Gamepiece", "Spawn Primary Gamepiece", "Pick Up Secondary Gamepiece", "Release Secondary Gamepiece", "Spawn Secondary Gamepiece" };

    public static void ResetDefaults()
    {
        ControlKey[(int)Control.Forward] = KeyCode.UpArrow;
        ControlKey[(int)Control.Backward] = KeyCode.DownArrow;
        ControlKey[(int)Control.Right] = KeyCode.RightArrow;
        ControlKey[(int)Control.Left] = KeyCode.LeftArrow;
        ControlKey[(int)Control.ResetRobot] = KeyCode.R;
        ControlKey[(int)Control.CameraToggle] = KeyCode.C;
        ControlKey[(int)Control.pwm2Plus] = KeyCode.Alpha1;
        ControlKey[(int)Control.pwm2Neg] = KeyCode.Alpha2;
        ControlKey[(int)Control.pwm3Plus] = KeyCode.Alpha3;
        ControlKey[(int)Control.pwm3Neg] = KeyCode.Alpha4;
        ControlKey[(int)Control.pwm4Plus] = KeyCode.Alpha5;
        ControlKey[(int)Control.pwm4Neg] = KeyCode.Alpha6;
        ControlKey[(int)Control.pwm5Plus] = KeyCode.Alpha7;
        ControlKey[(int)Control.pwm5Neg] = KeyCode.Alpha8;
        ControlKey[(int)Control.pwm6Plus] = KeyCode.Alpha9;
        ControlKey[(int)Control.pwm6Neg] = KeyCode.Alpha0;
        ControlKey[(int)Control.PickupPrimary] = KeyCode.X;
        ControlKey[(int)Control.ReleasePrimary] = KeyCode.E;
        ControlKey[(int)Control.SpawnPrimary] = KeyCode.Q;
        ControlKey[(int)Control.PickupSecondary] = KeyCode.X;
        ControlKey[(int)Control.ReleaseSecondary] = KeyCode.E;
        ControlKey[(int)Control.SpawnSecondary] = KeyCode.Q;

    }

    public static void LoadControls()
    {
        for (int i = 0; i < ControlKey.Length; i++)
        {
            if (PlayerPrefs.HasKey("ControlKey" + i.ToString())) ControlKey[i] = (KeyCode)PlayerPrefs.GetInt("ControlKey" + i.ToString());
        }
    }

    public static void SaveControls()
    {
        for (int i = 0; i < ControlKey.Length; i++)
        {
            PlayerPrefs.SetInt("ControlKey" + i.ToString(), (int)ControlKey[i]);
        }
        PlayerPrefs.Save();
    }

    public static bool SetControl(int control, KeyCode key)
    {
        ControlKey[control] = key;
        for (int i = 0; i < ControlKey.Length; i++)
        {
            if (i != control && ControlKey[i] == key)
            {
                return false;
            }
        }
        return true;
    }
    public static bool CheckConflict()
    {
        for (int i = 0; i < ControlKey.Length; i++)
        {
            for (int j = 1; j < ControlKey.Length; j++)
            {
                if (j != i && ControlKey[i] == ControlKey[j]) return true;
            }
        }
        return false;
    }

    //public static bool CheckConflict()
    //{
    //    foreach (var button in ControlKeyTest)
    //    {
    //        foreach (var button2 in ControlKeyTest)
    //            if (button == button2) return true;
    //    }
    //    return false;
    //}

    public static void DisableControls()
    {
        ControlKey.CopyTo(BackupKeys, 0);
        for (int i = 0; i < ControlKey.Length; i++)
        {
            ControlKey[i] = KeyCode.None;
        }
    }

    public static void EnableControls()
    {
        BackupKeys.CopyTo(BackupKeys, 0);
    }

    //==================================================Asset Store Collection=========================
    /// <summary>
    /// <see cref="Buttons"/> is a set of user defined buttons.
    /// </summary>
    public struct Buttons
    {
        public KeyMapping forward;
        public KeyMapping backward;
        public KeyMapping left;
        public KeyMapping right;
        public KeyMapping jump;
        public KeyMapping resetRobot;
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



    /// <summary>
    /// Initializes the <see cref="Controls"/> class.
    /// </summary>
    static Controls()
    {
        buttons.forward = InputControl.setKey("Forward", KeyCode.UpArrow, KeyCode.None, new JoystickInput(JoystickAxis.Axis2Negative));
        buttons.backward = InputControl.setKey("Backward", KeyCode.DownArrow, KeyCode.None, new JoystickInput(JoystickAxis.Axis2Positive));
        buttons.left = InputControl.setKey("Left", KeyCode.LeftArrow, KeyCode.None, new JoystickInput(JoystickAxis.Axis4Negative));
        buttons.right = InputControl.setKey("Right", KeyCode.RightArrow, KeyCode.None, new JoystickInput(JoystickAxis.Axis4Positive));
        buttons.jump = InputControl.setKey("Jump", new JoystickInput(JoystickButton.Button1), KeyCode.Space, KeyCode.None);

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
        // It is just an example. You may remove it or modify it if you want
        ReadOnlyCollection<KeyMapping> keys = InputControl.getKeysList();

        foreach (KeyMapping key in keys)
        {
            PlayerPrefs.SetString("Controls." + key.name + ".primary", key.primaryInput.ToString());
            PlayerPrefs.SetString("Controls." + key.name + ".secondary", key.secondaryInput.ToString());
            PlayerPrefs.SetString("Controls." + key.name + ".third", key.thirdInput.ToString());
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load controls.
    /// </summary>
    public static void Load()
    {
        // It is just an example. You may remove it or modify it if you want
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

            inputStr = PlayerPrefs.GetString("Controls." + key.name + ".third");

            if (inputStr != "")
            {
                key.thirdInput = customInputFromString(inputStr);
            }
        }
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