using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;

public class Controls : MonoBehaviour
{
    enum Control { Forward, Backward, Right, Left, PWMPositive, PWMNegative, ResetRobot, RobotOrient, CameraToggle };

    public static KeyCode[] ControlKey = new KeyCode[9];
    public static KeyCode[] ControlKeyDefaults = new KeyCode[9];

    void Start()
    {
        ControlKeyDefaults[(int)Control.Forward] = KeyCode.UpArrow;
        ControlKeyDefaults[(int)Control.Backward] = KeyCode.DownArrow;
        ControlKeyDefaults[(int)Control.Right] = KeyCode.RightArrow;
        ControlKeyDefaults[(int)Control.Left] = KeyCode.LeftArrow;
        ControlKeyDefaults[(int)Control.PWMPositive] = KeyCode.Alpha1;
        ControlKeyDefaults[(int)Control.PWMNegative] = KeyCode.Alpha2;
        ControlKeyDefaults[(int)Control.ResetRobot] = KeyCode.R;
        ControlKeyDefaults[(int)Control.RobotOrient] = KeyCode.O;
        ControlKeyDefaults[(int)Control.CameraToggle] = KeyCode.C;

        ResetDefaults();
    }

    public static void ResetDefaults()
    {
        System.Array.Copy(ControlKeyDefaults, ControlKey, 9);
    }


}