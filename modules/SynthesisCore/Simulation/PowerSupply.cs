using System;
using System.Collections.Generic;
using System.Text;

namespace SynthesisCore.Simulation
{
    public class PowerSupply
    {
        /// <summary>
        /// Voltage in mV
        /// </summary>
        public readonly double Voltage;
        // public readonly double Capacity;

        public double VoltagePercent(double percent)
        {
            return Math.Min(Math.Max(percent, -1), 1) * Voltage;
        }

        public PowerSupply(double voltage)
        {
            Voltage = voltage;
        }
    }
}
