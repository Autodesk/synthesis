using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Simulator.Input
{
    /// <summary>
    /// Used for axes on gamepads
    /// </summary>
    public class JoystickAxis : IAxisInput
    {
        public static explicit operator JoystickAxis(string info) => new JoystickAxis(info);

        public int JoystickID { get; private set; }
        public int AxisID { get; private set; }
        public bool Positive { get; private set; }

        public JoystickAxis(string axisInfo)
        {
            string[] split = axisInfo.Split(' ');
            JoystickID = int.Parse(split[1]);
            AxisID = int.Parse(split[3]);
            Positive = split[4].Equals("+") ? true : false;
        }

        public float GetValue(bool positiveOnly = false)
        {
            float rawVal = UnityEngine.Input.GetAxis("Joystick " + JoystickID + " Axis " + AxisID);

            // Checks to see if it needs special parsing
            if (InputHandler.ControllerRegistry[JoystickID] == InputHandler.ControllerType.Ps4 && (AxisID == 5 || AxisID == 4))
            {
                return Positive ? (rawVal + 1) / 2 : 0; // Axis 4 & 5 on a ps4 controller shouldn't ever get their negatives assign, but just in case.
            }

            if (!positiveOnly) return Positive ? Mathf.Clamp(rawVal, 0, float.MaxValue) : Mathf.Clamp(rawVal, float.MinValue, 0);
            else return Mathf.Abs(Positive ? Mathf.Clamp(rawVal, 0, float.MaxValue) : Mathf.Clamp(rawVal, float.MinValue, 0));
        }

        public override string ToString()
        {
            return "Joystick " + JoystickID + " Axis " + AxisID + " " + (Positive ? "+" : "-");
        }

        public static JoystickAxis GetCurrentlyActiveJoystickAxis(params string[] axesToIgnore)
        {
            float v = 0;
            for (int joy = 1; joy <= 11; joy++)
            {
                for (int ax = 1; ax <= 20; ax++)
                {
                    if (Array.Exists(axesToIgnore, x => x.Equals("Joystick " + joy + " Axis " + ax))) continue;

                    v = UnityEngine.Input.GetAxis("Joystick " + joy + " Axis " + ax);

                    bool h = false;

                    // Account for Ps4 weirdness
                    if (InputHandler.ControllerRegistry[joy] == InputHandler.ControllerType.Ps4 && (ax == 5 || ax == 4))
                    {
                        v = (v + 1) / 2;
                        h = true;
                    }

                    if (v > 0.5)
                    {
                        return (JoystickAxis)("Joystick " + joy + " Axis " + ax + " +");
                    }
                    else if (v < -0.5)
                    {
                        if (h) Debug.Log("That really shouldn't happen");
                        return (JoystickAxis)("Joystick " + joy + " Axis " + ax + " -");
                    }
                }
            }
            return null;
        }
    }
}
