using UnityEngine;

namespace Synthesis.Simulator.Input
{

    /// <summary>
    /// Used for turning 2 single direction axis inputs into a dual direction axis input
    /// </summary>
    public class DualAxis : IAxisInput
    {
        public static explicit operator DualAxis(string axisName) => new DualAxis(axisName);
        public static explicit operator DualAxis((IAxisInput pos, IAxisInput neg) axes) => new DualAxis(axes.pos, axes.neg);

        public IAxisInput PositiveAxis { get; private set; }
        public IAxisInput NegativeAxis { get; private set; }

        public DualAxis(IAxisInput pos, IAxisInput neg) {
            PositiveAxis = pos;
            NegativeAxis = neg;
        }

        public DualAxis(string axisName) {
            PositiveAxis = (UnityAxis)(axisName + " +");
            NegativeAxis = (UnityAxis)(axisName + " -");
        }

        public float GetValue(bool positiveOnly = false) {
            float val = PositiveAxis.GetValue(true) - NegativeAxis.GetValue(true);
            if (!positiveOnly) return val;
            else return Mathf.Abs(val);
        }

        public override string ToString()
        {
            return PositiveAxis.ToString() + "/-";
        }
    }

}