using SynthesisAPI.Utilities;
using System;
using Math = System.Math;

namespace SynthesisCore.Simulation
{
    public class DCMotor
    {
        public readonly double InternalResistance;
        public readonly double TorqueConstant;
        public readonly double ElectricalConstant;
        public readonly double MomentOfInertia;
        public readonly double FrictionalCoefficient;

        private readonly double K;
        private readonly double VoltageScalar;
        private readonly double LoadTorqueScalar;
        private readonly double VelocityScalar;

        private AngularVelocity lastAngularVelocity;
        private double lastTorque;
        private double lastCurrent;
        private double lastTime;

        /// <summary>
        /// Torque in Nm
        /// </summary>
        public double Torque => lastTorque;
        /// <summary>
        /// Current angular speed
        /// </summary>
        public AngularVelocity AngularSpeed => lastAngularVelocity;
        /// <summary>
        /// Current in Amps
        /// </summary>
        public double Current => lastCurrent;

        public DCMotor(double internalResistance, double torqueConstant, double electricalConstant, double momentOfInertia, double frictionalCoefficient)
        {
            InternalResistance = internalResistance;
            TorqueConstant = torqueConstant;
            ElectricalConstant = electricalConstant;
            MomentOfInertia = momentOfInertia;
            FrictionalCoefficient = frictionalCoefficient;

            if (Math.Abs(ElectricalConstant - TorqueConstant) > 0.001)
            {
                Logger.Log($"Electrical constant and torque contant are the same for DC motors, but assigning them to {ElectricalConstant} and {TorqueConstant} respectively", LogLevel.Error);
                ElectricalConstant = TorqueConstant;
            }

            K = ElectricalConstant; // TODO  ElectricalConstant and TorqueConstant are same for DC motors
            VoltageScalar = K / (MomentOfInertia * InternalResistance);
            LoadTorqueScalar = 1d / MomentOfInertia;
            VelocityScalar = FrictionalCoefficient / MomentOfInertia + (K * K) / (MomentOfInertia * InternalResistance);

            lastAngularVelocity = AngularVelocity.FromRadiansPerSec(0);
            lastTorque = 0;
            lastCurrent = 0;
            lastTime = 0;
        }

        private double BackEMF() => ElectricalConstant * lastAngularVelocity.RadiansPerSec;

        private void UpdateCurrent(double sourceVoltage)
        {
            lastCurrent = (sourceVoltage - BackEMF()) / InternalResistance;
        }

        private void UpdateTorque(double sourceVoltage)
        {
            UpdateCurrent(sourceVoltage);
            lastTorque = TorqueConstant * lastCurrent;
        }

        private void UpdateAngularVelocity(double sourceVoltage, double loadTorqueMagnitude)
        {
            double time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            double timeDelta = time - lastTime;
            double angularAcceleration = lastAngularVelocity.RadiansPerSec / timeDelta;

            double signedLoadTorque = loadTorqueMagnitude * Math.Sign(sourceVoltage);

            // Calculated using this DC motor state function http://ocw.nctu.edu.tw/course/dssi032/DSSI_2.pdf
            double angularVelocity = (VoltageScalar * sourceVoltage - LoadTorqueScalar * signedLoadTorque - angularAcceleration) / VelocityScalar;

            if (double.IsNaN(angularVelocity) || double.IsInfinity(angularVelocity) || Math.Sign(angularVelocity) != Math.Sign(sourceVoltage)) // Stall motor instead of produing negative angular speed
            {
                angularVelocity = 0;
            }

            lastAngularVelocity.RadiansPerSec = angularVelocity;
            lastTime = time;
        }

        public void Update(double sourceVoltage, double loadTorque)
        {
            UpdateAngularVelocity(sourceVoltage, Math.Abs(loadTorque));
            UpdateTorque(sourceVoltage);
        }
    }
}