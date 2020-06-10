using System;

namespace Synthesis.Simulator.Input
{
    /// <summary>
    /// Used for giving a placeholder axis that always returns 0 for the value
    /// </summary>
    public class DeadAxis : IAxisInput
    {
        private static DeadAxis instance;
        public static DeadAxis Instance {
            get {
                if (instance == null) instance = new DeadAxis();
                return instance;
            }
        }

        private DeadAxis() { }

        public float GetValue(bool positiveOnly = false) => 0.0f;
    }
}