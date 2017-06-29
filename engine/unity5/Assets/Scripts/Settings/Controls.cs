using UnityEngine;
using System.Collections.Generic;

public class Controls : MonoBehaviour
{
    public enum Control { Forward, Backward, Right, Left, ResetRobot, CameraToggle, pwm2Plus, pwm2Neg, pwm3Plus, pwm3Neg, pwm4Plus, pwm4Neg, pwm5Plus, pwm5Neg, pwm6Plus, pwm6Neg, Pickup, Release, Spawn};

    public static KeyCode[] ControlKey = new KeyCode[20];
    public static readonly string[] ControlName = { "Move Forward", "Move Backward", "Turn Right", "Turn Left", "Reset Robot", "Toggle Camera", "PWM 2 Positive", "PWM 2 Negative", "PWM 3 Positive", "PWM 3 Negative", "PWM 4 Positive", "PWM 4 Negative", "PWM 5 Positive", "PWM 5 Negative", "PWM 6 Positive", "PWM 6 Negative", "Pick Up Gamepiece", "Release Gamepiece", "Spawn Gamepiece"};

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
        ControlKey[(int)Control.Pickup] = KeyCode.Space;
        ControlKey[(int)Control.Release] = KeyCode.E;
        ControlKey[(int)Control.Spawn] = KeyCode.Q;
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
}