using UnityEngine;
using System.Collections.Generic;
using ExceptionHandling;

/// <summary>
/// The class that takes care of all the control inputs. It has a public static list of the all the input settings so that it can be acessed from any script.
/// </summary>
public class Controls : MonoBehaviour
{
    //Update these variables if there is a need to add more inputs.
    public enum Control { Forward, Backward, Right, Left, ResetRobot, RobotOrient, CameraToggle, pwm3Plus, pwm3Neg, pwm4Plus, pwm4Neg, pwm5Plus, pwm5Neg, Stats};
    public static KeyCode[] ControlKey = new KeyCode[14];

    //Resets all the input settings to default.
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

    //Loads all the control settings from player prefs (Only called in the initialization of the simluation at the start).
    public static void LoadControls()
    {
        for (int i = 0; i < ControlKey.Length; i++)
        {
            if (PlayerPrefs.HasKey("ControlKey" + i.ToString())) ControlKey[i] = (KeyCode)PlayerPrefs.GetInt("ControlKey" + i.ToString());
        }
    }

    //Saves all the control settings to player prefs (So that the settings are saved even after resetting the simulation).
    public static void SaveControls()
    {
        for (int i = 0; i < ControlKey.Length; i++)
        {
            PlayerPrefs.SetInt("ControlKey" + i.ToString(), (int)ControlKey[i]);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Sets a control in the array to a new key.
    /// </summary>
    /// <param name="control">The index of the control key that is to be changed </param>
    /// <param name="key">The input key to be switched to </param>
    /// <returns>false if there is a conflict with the keys.</returns>
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
    /// <summary>
    /// Check if there is a conflict with any controls
    /// </summary>
    /// <returns>true if there is a conflict.</returns>
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