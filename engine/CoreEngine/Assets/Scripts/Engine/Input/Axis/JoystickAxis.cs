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
            float val = UnityEngine.Input.GetAxis("Joystick " + JoystickID + " Axis " + AxisID);
            if (!positiveOnly) return Positive ? Mathf.Clamp(val, 0, float.MaxValue) : Mathf.Clamp(val, float.MinValue, 0);
            else return Mathf.Abs(Positive ? Mathf.Clamp(val, 0, float.MaxValue) : Mathf.Clamp(val, float.MinValue, 0));
        }

        public override string ToString()
        {
            return "Joystick " + JoystickID + " Axis " + AxisID + " " + (Positive ? "+" : "-");
        }

        public static JoystickAxis GetCurrentlyActiveJoystickAxis(params string[] axesToIgnore)
        {
            float v = 0;
            for (int joy = 1; joy < 11; joy++)
            {
                for (int ax = 1; ax < 20; ax++)
                {
                    if (Array.Exists(axesToIgnore, x => x.Equals("Joystick " + joy + " Axis " + ax))) continue;
                    v = UnityEngine.Input.GetAxis("Joystick " + joy + " Axis " + ax);
                    if (v > 0.5)
                    {
                        return (JoystickAxis)("Joystick " + joy + " Axis " + ax + " +");
                    }
                    else if (v < -0.5)
                    {
                        return (JoystickAxis)("Joystick " + joy + " Axis " + ax + " -");
                    }
                }
            }
            return null;
        }
    }
}
