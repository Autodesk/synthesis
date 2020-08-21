namespace SynthesisCore.Simulation
{
    public class MotorTypes
    {
        public static DCMotor CIMMotor
        {
            get
            {
                return new DCMotor(
                    // From CIM documentation
                    0.091,     // Ohms
                    0.018803,  // N m / Amp
                    0.018803,  // V / rad / sec
                               // From Chief Delphi post
                    7.754E-05, // kg m^2
                               // Trial and error to match speed-torque curve
                    0.00057    // N m s
                );
            }
        }
    }
}
