using SynthesisAPI.Utilities;
using System.Collections.Generic;

namespace SynthesisCore.Simulation
{
    /// <summary>
    /// A container for types of motors
    /// </summary>
    public static class MotorTypes
    {
        public static List<string> AllTypeNames { get; private set; } = new List<string>();
        private static Dictionary<string, DCMotor> Types = new Dictionary<string, DCMotor>();

        public static bool Contains(string name)
        {
            return Types.ContainsKey(name);
        }

        public static DCMotor Get(string name)
        {
            return Types[name].Clone();
        }

        public static void Add(string name, DCMotor type)
        {
            if (Types.ContainsKey(name))
            {
                throw new SynthesisException($"Cannot add new motor type with existing name {name}");
            }
            AllTypeNames.Add(name);
            type.Name = name;
            Types[name] = type;
        }

        static MotorTypes()
        {
            Add("CIM", new DCMotor(
                    // From CIM documentation
                    0.091,     // Ohms
                    0.018803,  // N m / Amp
                    0.018803,  // V / rad / sec
                    // From Chief Delphi post
                    7.754E-05, // kg m^2
                    // Trial and error to match speed-torque curve
                    0.00057,   // N m s  //TODO maybe 8.91E-05
                    0.605      // Determined experimentally in Synthesis
                ));
            // Remainder from https://www.chiefdelphi.com/uploads/default/original/3X/d/2/d2495ff57e402e93bb479171750bb6b05ec8e594.pdf
            Add("mini-CIM", new DCMotor(
                    1.34E-01, // Ohms
                    1.74E-02, // N m / Amp
                    1.74E-02, // V / rad / sec
                    5.54E-05, // kg m^2
                    7.70E-05, // N m s
                    0.35      // Determined experimentally in Synthesis
                ));
            Add("BAG", new DCMotor(
                    2.28E-01, // Ohms
                    8.29E-03, // N m / Amp
                    8.29E-03, // V / rad / sec
                    7.67E-06, // kg m^2
                    1.07E-05, // N m s
                    2.2       // Determined experimentally in Synthesis
                ));
            Add("775pro", new DCMotor(
                    8.98E-02, // Ohms
                    5.30E-03, // N m / Amp
                    5.69E-03, // V / rad / sec
                    1.04E-05, // kg m^2
                    1.89E-06, // N m s
                    5         // Determined experimentally in Synthesis
                ));
        }
    }
}
