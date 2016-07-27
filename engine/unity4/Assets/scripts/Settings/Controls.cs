using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;

public class Controls : MonoBehaviour
{
    public enum Control { Forward, Backward, Right, Left, ResetRobot, RobotOrient, CameraToggle, pwm3Plus, pwm3Neg, pwm4Plus, pwm4Neg, pwm5Plus, pwm5Neg, Stats};

    public static KeyCode[] ControlKey = new KeyCode[14];

    public static void ResetDefaults()
    {
        ControlKey[(int)Control.Forward] = KeyCode.UpArrow;
        ControlKey[(int)Control.Backward] = KeyCode.DownArrow;
        ControlKey[(int)Control.Right] = KeyCode.RightArrow;
        ControlKey[(int)Control.Left] = KeyCode.LeftArrow;
        ControlKey[(int)Control.ResetRobot] = KeyCode.R;
        ControlKey[(int)Control.RobotOrient] = KeyCode.O;
        ControlKey[(int)Control.CameraToggle] = KeyCode.C;
        ControlKey[(int)Control.pwm3Plus] = KeyCode.Alpha1;
        ControlKey[(int)Control.pwm3Neg] = KeyCode.Alpha2;
        ControlKey[(int)Control.pwm4Plus] = KeyCode.Alpha3;
        ControlKey[(int)Control.pwm4Neg] = KeyCode.Alpha4;
        ControlKey[(int)Control.pwm5Plus] = KeyCode.Alpha5;
        ControlKey[(int)Control.pwm5Neg] = KeyCode.Alpha6;
        ControlKey[(int)Control.Stats] = KeyCode.S;
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