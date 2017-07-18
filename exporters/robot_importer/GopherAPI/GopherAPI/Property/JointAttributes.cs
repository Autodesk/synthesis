using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GopherAPI.Properties
{
    public enum Wheel { NON_WHEEL, NORMAL, OMNI, MECANUM }
    /// <summary>
    /// NONE is for limited joints without friction | NO_LIMITS is for non-limited joints 
    /// </summary>
    public enum Friction { NONE, HIGH, MEDIUM, LOW, NO_LIMITS }
    public enum InternalDiameter { ONE, HALF, QUARTER }
    public enum Pressure { SIXTY_PSI, TWENTY_PSI, TEN_PSI }
    public enum Stages { SINGLE_STAGE_ELEVATOR, CASCADING_STAGE_ONE, CASCADING_STAGE_TWO, CONTINUOUS_STAGE_ONE, CONTINUOUS_STAGE_TWO }
    public enum JAType { NONE, MOTOR, SERVO, BUMPER_PNUEMATIC, RELAY_PNUEMATIC, WORM_SCREW, DUAL_MOTOR, ELEVATOR }

    public interface IJointAttribute
    {
        /// <summary>
        /// Returns the Joint STLAttribute type
        /// </summary>
        /// <returns></returns>
        JAType GetJAType();
        bool GetIsDriveWheel();
        uint GetJointID();
    }

    public struct NoDriver : IJointAttribute
    {
        public uint GetJointID()
        {
            return JointID;
        }
        public readonly uint JointID;
        public JAType GetJAType()
        {
            return JAType.NONE;
        }
        public NoDriver(uint jointID)
        {
            JointID = jointID;
        }
        public bool GetIsDriveWheel()
        {
            return false;
        }
    }

    public struct Motor : IJointAttribute
    {
        public readonly uint JointID;
        public readonly bool IsCAN;
        public uint GetJointID()
        {
            return JointID;
        }

        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public readonly float MotorPort;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public readonly bool IsDriveWheel;
        public readonly Wheel WheelType;

        public readonly UInt16 InputGear;
        public readonly UInt16 OutputGear;

        public JAType GetJAType()
        {
            return JAType.MOTOR;
        }

        public bool GetIsDriveWheel()
        {
            return IsDriveWheel;
        }
        
        public Motor(uint jointID, bool isCAN, float motorPort, bool hasLimits, Friction friction, bool isDriveWheel, Wheel wheelType, UInt16 inputGear, UInt16 outputGear)
        {
            JointID = jointID;
            IsCAN = isCAN;
            MotorPort = motorPort;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;
            IsDriveWheel = isDriveWheel;
            WheelType = wheelType;
            InputGear = inputGear;
            OutputGear = outputGear;
        }
    }

    public struct Servo : IJointAttribute
    {
        public readonly uint JointID;

        /// <summary>
        /// Always a CAN
        /// </summary>
        public readonly float MotorPort;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public JAType GetJAType()
        {
            return JAType.SERVO;
        }

        public bool GetIsDriveWheel()
        {
            return false;
        }

        public uint GetJointID()
        {
            return JointID;
        }

        public Servo(uint jointID, float motorPort, bool hasLimits, Friction friction)
        {
            JointID = jointID;
            MotorPort = motorPort;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;
        }
    }

    public struct BumperPnuematic : IJointAttribute
    {
        public readonly uint JointID;

        public uint GetJointID()
        {
            return JointID;
        }

        public readonly float SolenoidPortOne;
        public readonly float SolenoidPortTwo;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public readonly InternalDiameter InternalDiameter;
        public readonly Pressure Pressure;

        public JAType GetJAType()
        {
            return JAType.BUMPER_PNUEMATIC;
        }

        public bool GetIsDriveWheel()
        {
            return false;
        }

        public BumperPnuematic(uint jointID, float solenoidPortOne, float solenoidPortTwo, bool hasLimits, Friction friction, InternalDiameter internalDiameter, Pressure pressure)
        {
            JointID = jointID;
            SolenoidPortOne = solenoidPortOne;
            SolenoidPortTwo = solenoidPortTwo;

            HasLimits = hasLimits;
            Friction = friction;

            InternalDiameter = internalDiameter;
            Pressure = pressure;
        }
    }

    public struct RelayPnuematic : IJointAttribute
    {
        public readonly uint JointID;
        public uint GetJointID()
        {
            return JointID;
        }


        public readonly float RelayPort;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public readonly InternalDiameter InternalDiameter;
        public readonly Pressure Pressure;

        public JAType GetJAType()
        {
            return JAType.RELAY_PNUEMATIC;
        }

        public bool GetIsDriveWheel()
        {
            return false;
        }

        public RelayPnuematic(uint jointID, float relayPort, bool hasLimits, Friction friction, InternalDiameter internalDiameter, Pressure pressure)
        {
            JointID = jointID;
            RelayPort = relayPort;

            HasLimits = hasLimits;
            Friction = friction;

            InternalDiameter = internalDiameter;
            Pressure = pressure;
        }
    }

    public struct WormScrew : IJointAttribute
    {
        public readonly uint JointID;
        public uint GetJointID()
        {
            return JointID;
        }


        public readonly bool IsCAN;
        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public readonly float MotorPort;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public bool GetIsDriveWheel()
        {
            return false;
        }
        
        public JAType GetJAType()
        {
            return JAType.WORM_SCREW;
        }
        public WormScrew(uint jointID, bool isCAN, float motorPort, bool hasLimits, Friction friction)
        {
            JointID = jointID;
            IsCAN = isCAN;
            MotorPort = motorPort;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;
        }
    }

    public struct DualMotor : IJointAttribute
    {
        public readonly uint JointID;

        public uint GetJointID()
        {
            return JointID;
        }

        public readonly bool IsCAN;

        public readonly float PortOne;
        public readonly float PortTwo;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public readonly bool IsDriveWheel;
        public readonly Wheel WheelType;

        public readonly UInt16 InputGear;
        public readonly UInt16 OutputGear;

        public JAType GetJAType()
        {
            return JAType.DUAL_MOTOR;
        }

        public bool GetIsDriveWheel()
        {
            return IsDriveWheel;
        }


        public DualMotor(uint jointID, bool isCAN, float motorPort1, float motorPort2, bool hasLimits, Friction friction, bool isDriveWheel, Wheel wheelType, UInt16 inputGear, UInt16 outputGear)
        {
            JointID = jointID;
            IsCAN = isCAN;
            PortOne = motorPort1;
            PortTwo = motorPort2;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;
            IsDriveWheel = isDriveWheel;
            WheelType = wheelType;
            InputGear = inputGear;
            OutputGear = outputGear;
        }
    }

    public struct Elevator : IJointAttribute
    {
        public readonly uint JointID;

        public uint GetJointID()
        {
            return JointID;
        }

        public readonly bool IsCAN;
        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public readonly float MotorPort;

        public readonly bool HasLimits;
        public readonly Friction Friction;

        public readonly bool HasBrake;
        public readonly float BrakePortOne;
        public readonly float BrakePortTwo;

        public readonly Stages Stages;
        public readonly float InputGear;
        public readonly float OutputGear;

        public JAType GetJAType()
        {
            return JAType.ELEVATOR;
        }

        public Elevator(uint jointID, bool isCAN, float motorPort, bool hasLimits, Friction friction, bool hasBrake, float brakePortOne, float brakePortTwo, Stages stages, float inputGear, float outputGear)
        {
            JointID = jointID;
            IsCAN = isCAN;
            MotorPort = motorPort;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;

            HasBrake = hasBrake;
            BrakePortOne = brakePortOne;
            BrakePortTwo = brakePortTwo;

            Stages = stages;

            InputGear = inputGear;
            OutputGear = outputGear;
        }

    }
}