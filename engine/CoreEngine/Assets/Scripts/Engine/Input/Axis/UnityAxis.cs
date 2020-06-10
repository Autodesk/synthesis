using UnityEngine;
using System;

namespace Synthesis.Simulator.Input
{
    /// <summary>
    /// Used for axes default to Unity
    /// </summary>
    public class UnityAxis : IAxisInput
    {
        public static implicit operator UnityAxis((string axisName, bool usePos) info) => new UnityAxis(info.axisName, info.usePos);
        public static explicit operator UnityAxis(string info) => FromString(info);

        public static readonly string[] DEFAULT_AXES =
        {
            "Mouse X", "Mouse Y", "Mouse ScrollWheel", "Horizontal", "Vertical"
        };

        public string AxisName { get; private set; }
        public bool UsePositive { get; private set; }

        public UnityAxis(string axis, bool usePositive)
        {
            AxisName = axis;
            UsePositive = usePositive;
        }

        public float GetValue(bool positiveOnly = false)
        {
            if (UsePositive)
            {
                return Mathf.Clamp(UnityEngine.Input.GetAxis(AxisName), 0, float.MaxValue);
            }
            else
            {
                if (!positiveOnly) return Mathf.Clamp(UnityEngine.Input.GetAxis(AxisName), float.MinValue, 0);
                else return Mathf.Abs(Mathf.Clamp(UnityEngine.Input.GetAxis(AxisName), float.MinValue, 0));
            }
        }

        public static UnityAxis GetCurrentlyActiveUnityAxis(params string[] axesToIgnore)
        {
            foreach (string s in DEFAULT_AXES)
            {
                if (Array.Exists(axesToIgnore, x => x.Equals(s))) continue;

                if (UnityEngine.Input.GetAxis(s) >= 0.5) return FromString(s + " +");
                else if (UnityEngine.Input.GetAxis(s) <= -0.5) return FromString(s + " -");
            }

            return null;
        }

        public override string ToString()
        {
            return AxisName + (UsePositive ? " +" : " -");
        }

        public static UnityAxis FromString(string a)
        {
            if (a.Length < 2) return null;
            bool pos = a[a.Length - 1].Equals('+');
            return new UnityAxis(a.Substring(0, a.Length - 2), pos);
        }

    }

}