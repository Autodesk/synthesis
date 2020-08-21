using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System.Collections.Generic;

namespace SynthesisCore.Simulation
{
    public class MotorAssembly
    {
        public readonly Entity Entity;
        public readonly HingeJoint Joint;
        public List<DCMotor> Motors { get; private set; }
        private DCMotor Motor;

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
        public double Torque => Motor.Torque * GearReduction;


        private double voltage;
        private double constantLoadTorque;

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

            Motors = new List<DCMotor>();
            GearReduction = 1;
        }

        public void Configure(DCMotor motor,/*IEnumerable<DCMotor> motors, */ double gearReduction = 1)
        {
            // Motors = (List<DCMotor>) motors;
            Motor = motor;
            GearReduction = gearReduction;
        }

        public void SetVoltage(double sourceVoltage)
        {
            voltage = sourceVoltage;
        }

        /// <summary>
        /// Set a constant load torque in N m used to calculate motor performace
        /// 
        /// Recommended to provide half of the motor's maximum torque
        /// 
        /// This is used when updating the motor velocity in place of an actual load torque on the motor,
        /// which currently cannot be calculated easily from Unity's physics
        /// </summary>
        /// <param name="loadTorque"></param>
        public void SetConstantLoadTorque(double loadTorque)
        {
            constantLoadTorque = loadTorque; // TODO make this unnecessary. Will require significant physics changes.
        }

        /// <summary>
        /// Update the motor velocity
        /// </summary>
        public void Update()
        {
            Update(constantLoadTorque);
        }

        /// <summary>
        /// Update the motor velocity given a provided torque
        /// </summary>
        /// <param name="loadTorque">The load on the motor in N m</param>
        public void Update(double loadTorque)
        {
            Motor.Update(voltage, loadTorque / GearReduction);
            var motor = Joint.Motor;
            motor.TargetVelocity = (float)(Motor.AngularSpeed.DegreesPerSec / GearReduction);
            motor.Force = float.PositiveInfinity;
            Joint.Motor = motor;
        }
    }
}
