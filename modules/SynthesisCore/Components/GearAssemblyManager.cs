using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Utilities;
using System.Collections.Generic;

using Math = System.Math;

namespace SynthesisCore.Components
{
    public class GearAssemblyManager : Component {

        public void AddMotor(MotorAssembly controller)
        {
            AllGearBoxes.Add(controller);
        }

        public void RemoveMotor(int index)
        {
            AllGearBoxes.RemoveAt(index);
        }

        private List<MotorAssembly> _allGearBoxes = null;

        public List<MotorAssembly> AllGearBoxes
        {
            get
            {
                if (_allGearBoxes == null)
                {
                    _allGearBoxes = new List<MotorAssembly>();

                    foreach (var joints in EnvironmentManager.GetComponentsWhere<Joints>(c => IsDescendant(Entity.Value, c.Entity.Value)))
                    {
                        foreach (var j in joints.AllJoints)
                        {
                            if (j is HingeJoint hingeJoint)
                            {
                                _allGearBoxes.Add(new MotorAssembly(hingeJoint, MotorFactory.CIMMotor(), 9.29, 1));
                            }
                        }
                    }
                }
                return _allGearBoxes;
            }
            set => _allGearBoxes = value;
        }

        private bool IsDescendant(Entity ancestor, Entity test)
        {
            if (test == ancestor)
                return true;
            var parent = test.GetComponent<Parent>().ParentEntity;
            return parent != null && IsDescendant(ancestor, parent);
        }

        public GearAssemblyManager()
        {
            Joints.GlobalAddJoint += j =>
            {
                if (j is HingeJoint hingeJoint)
                {
                    AllGearBoxes.Add(new MotorAssembly(hingeJoint, MotorFactory.CIMMotor(), 9.29, 1));
                }
            };
        }
    }

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

            if(Math.Abs(ElectricalConstant - TorqueConstant) > 0.001)
            {
                Logger.Log($"Electrical constant and torque contant are the same for DC motors, but assigning them to {ElectricalConstant} and {TorqueConstant} respectively", LogLevel.Error);
                ElectricalConstant = TorqueConstant;
            }

            K = ElectricalConstant; // TODO  ElectricalConstant and TorqueConstant are same for DC motors
            VoltageScalar = K / (MomentOfInertia * InternalResistance);
            LoadTorqueScalar = -1d / MomentOfInertia;
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

        private void UpdateAngularVelocity(double sourceVoltage, double loadTorque)
        {
            double time = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            double timeDelta = time - lastTime;
            double angularAcceleration = lastAngularVelocity.RadiansPerSec / timeDelta;

            // Calculated using this DC motor state function http://ocw.nctu.edu.tw/course/dssi032/DSSI_2.pdf
            double angularVelocity = (VoltageScalar * sourceVoltage + LoadTorqueScalar * loadTorque - angularAcceleration) / VelocityScalar;
            if(Math.Sign(angularVelocity) != Math.Sign(sourceVoltage)) // Stall motor instead of produing negative angular speed
            {
                angularVelocity = 0;
            }

            lastAngularVelocity.RadiansPerSec = angularVelocity;
            lastTime = time;
        }

        public void Update(double sourceVoltage, double loadTorque)
        {
            UpdateAngularVelocity(sourceVoltage, loadTorque);
            UpdateTorque(sourceVoltage);
        }
    }

    public class MotorAssembly
    {
        public HingeJoint Joint;
        public readonly DCMotor Motor;

        public double GearReduction;
        public readonly int MotorCount;

        public bool FreeSpin
        {
            get => Joint.Motor.FreeSpin;
            set => Joint.Motor.FreeSpin = value;
        }

        public MotorAssembly(HingeJoint joint, DCMotor motor, double gearReduction = 1, int motorCount = 1)
        {
            Joint = joint;
            joint.UseMotor = true;

            Motor = motor;
            GearReduction = gearReduction;
            if (motorCount < 1)
            {
                Logger.Log($"Motor count cannot be less than 1 but is {motorCount}", LogLevel.Warning);
                motorCount = 1;
            }
            MotorCount = motorCount;
        }

        public void Update(double sourceVoltage, double loadTorque)
        {
            Motor.Update(sourceVoltage, loadTorque / GearReduction);
            var motor = Joint.Motor;
            motor.TargetVelocity = (float)(Motor.AngularSpeed.DegreesPerSec / GearReduction);
            motor.Force = float.PositiveInfinity;
            Joint.Motor = motor;
        }
    }

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

    public class MotorFactory
    {
        public static DCMotor CIMMotor()
        {
            return new DCMotor(
                // From CIM documentation
                0.091,     // Ohms
                0.018803,  // Nm / Amp
                0.018803,  // V / rad / sec
                // From Chief Delphi post
                7.754E-05, // kg m^2
                // Trial and error to match speed-torque curve
                0.00057    // N.m.s
            );
        }
    }

    public class AngularVelocity
    {
        private double radiansPerSec;

        public double RadiansPerSec
        {
            get => radiansPerSec;
            set => radiansPerSec = value;
        }

        public double DegreesPerSec
        {
            get => radiansPerSec * (180d / Math.PI);
            set => radiansPerSec = value * (Math.PI / 180d);
        }

        public double RotationsPerMin
        {
            get => radiansPerSec * (30d / Math.PI);
            set => radiansPerSec = value * (Math.PI / 30d);
        }

        public static AngularVelocity FromRadiansPerSec(double radiansPerSec)
        {
            return new AngularVelocity
            {
                radiansPerSec = radiansPerSec
            };
        }

        public static AngularVelocity FromDegreesPerSec(double degreesPerSec)
        {
            return new AngularVelocity
            {
                DegreesPerSec = degreesPerSec
            };
        }

        public static AngularVelocity FromRorationsPerMin(double rotationsPerMin)
        {
            return new AngularVelocity
            {
                RotationsPerMin = rotationsPerMin
            };
        }

        public override string ToString()
        {
            return radiansPerSec.ToString() + " rad/sec";
        }

        private AngularVelocity()
        {
            radiansPerSec = 0;
        }
    }
}