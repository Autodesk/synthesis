using System;

namespace SynthesisCore.Simulation
{
    /// <summary>
    /// Models a power supply such as a battery
    /// </summary>
    public class PowerSupply
    {
        /// <summary>
        /// Voltage in V
        /// </summary>
        public readonly double Voltage;
        
        // public readonly double Capacity; // TODO

        /// <summary>
        /// Calculates a percent of the voltage of the power supply
        /// </summary>
        /// <param name="percent"></param>
        /// <returns></returns>
        public double VoltagePercent(double percent)
        {
            return Math.Min(Math.Max(percent, -1), 1) * Voltage;
        }

        /// <summary>
        /// Create a new power supply
        /// </summary>
        /// <param name="startingVoltage">The Voltage of the power supply in V</param>
        public PowerSupply(double startingVoltage)
        {
            Voltage = startingVoltage;
        }
    }
}
