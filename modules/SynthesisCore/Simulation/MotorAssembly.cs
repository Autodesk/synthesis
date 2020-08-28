using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System.Collections.Generic;

namespace SynthesisCore.Simulation
{
    /// <summary>
    /// A model for a motor assembly, which includes motors and gearing
    /// 
    /// This model can be used to update the phyics simulation
    /// </summary>
    public class MotorAssembly
    {
        public readonly Entity Entity;
        public readonly HingeJoint Joint;
        // public List<DCMotor> Motors { get; private set; }
        public DCMotor Motor; // TODO replace with Motors list above (allow multiple motors in an assembly)

        private uint motorCount;

        public uint MotorCount
        {
            get => motorCount;
            set
            {
                motorCount = System.Math.Max(value, 1);
            }
        }
        public double GearReduction;

        /// <summary>
        /// Set angular velocity in rad/s
        /// </summary>
        public double AngularVelocity => Motor.AngularSpeed.RadiansPerSec / GearReduction;
        /// <summary>
        /// Actual angular velocity in rad/s
        /// </summary>
        public double ActualAngularVelocity => Simulation.AngularVelocity.FromDegreesPerSec(Joint.Velocity).RadiansPerSec;
        /// <summary>
        /// Torque in N m
        /// </summary>
        public double Torque => Motor.Torque * GearReduction * MotorCount;


        private double voltage;

        public bool FreeSpin
        {
            get => Joint.Motor.FreeSpin;
            set => Joint.Motor.FreeSpin = value;
        }

        public MotorAssembly(Entity e, HingeJoint joint)
        {
            Entity = e;
            Joint = joint;
            Joint.UseMotor = true;

            // Motors = new List<DCMotor>();
            GearReduction = 1;
            MotorCount = 1;
        }

        public void Configure(DCMotor motor, uint motorCount = 1, double gearReduction = 1)
        {
            Motor = motor;
            MotorCount = motorCount;
            GearReduction = gearReduction;
        }

        public void SetVoltage(double sourceVoltage)
        {
            voltage = sourceVoltage;
        }

        /// <summary>
        /// Update the motor velocity
        /// </summary>
        public void Update()
        {
            Update(Motor.OptionalConstantLoadTorque);
        }

        private double CalculatePerMotorTorque(double loadTorque)
        {
            return loadTorque / (GearReduction * MotorCount);
        }

        /// <summary>
        /// Update the motor velocity given a provided torque
        /// </summary>
        /// <param name="loadTorque">The load on the motor in N m</param>
        public void Update(double loadTorque)
        {
            Motor.Update(voltage, CalculatePerMotorTorque(loadTorque));
            var motor = Joint.Motor;
            motor.TargetVelocity = (float)(Motor.AngularSpeed.DegreesPerSec / GearReduction);
            motor.Force = float.PositiveInfinity;
            Joint.Motor = motor;
        }
    }
}
