using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;

public class Controls : MonoBehaviour
{
    public enum Control { Forward, Backward, Right, Left, ResetRobot, RobotOrient, CameraToggle, pwm3PLus, pwm3Neg, pwm4Plus, pwm4Neg, pwm5Plus, pwm5Neg, Stats};

    public static KeyCode[] ControlKey = new KeyCode[14];
    public static KeyCode[] ControlKeyDefaults = new KeyCode[14];

    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    void Start()
    {
        ControlKeyDefaults[(int)Control.Forward] = KeyCode.UpArrow;
        ControlKeyDefaults[(int)Control.Backward] = KeyCode.DownArrow;
        ControlKeyDefaults[(int)Control.Right] = KeyCode.RightArrow;
        ControlKeyDefaults[(int)Control.Left] = KeyCode.LeftArrow;
        ControlKeyDefaults[(int)Control.ResetRobot] = KeyCode.R;
        ControlKeyDefaults[(int)Control.RobotOrient] = KeyCode.O;
        ControlKeyDefaults[(int)Control.CameraToggle] = KeyCode.C;
        ControlKeyDefaults[(int)Control.pwm3PLus] = KeyCode.Alpha1;
        ControlKeyDefaults[(int)Control.pwm3Neg] = KeyCode.Alpha2;
        ControlKeyDefaults[(int)Control.pwm4Plus] = KeyCode.Alpha3;
        ControlKeyDefaults[(int)Control.pwm4Neg] = KeyCode.Alpha4;
        ControlKeyDefaults[(int)Control.pwm5Plus] = KeyCode.Alpha5;
        ControlKeyDefaults[(int)Control.pwm5Neg] = KeyCode.Alpha6;
        ControlKeyDefaults[(int)Control.Stats] = KeyCode.S;

        ResetDefaults();
    }

    public static void ResetDefaults()
    {
        System.Array.Copy(ControlKeyDefaults, ControlKey, 9);
    }

    public static bool SetControl(int control, KeyCode key)
    {
        for (int i = 0; i < ControlKey.Length; i++)
        {
            if (i != control && ControlKey[i] == key)
            {
                return false;
            }
        }
        ControlKey[control] = key;
        return true;
    }


}