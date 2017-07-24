using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotImporter.Joints
{
    struct JointAttribute
    {
        public enum JointType { ROTATIONAL, LINEAR }
        public enum WheelType { NO_WHEEL, NORMAL, OMNI, MECANMUM };
        public enum FrictionType { NONE, HIGH, MEDIUM, LOW };
        public enum InternalDiameterType { ONE, POINT_FIVE, POINT_TWO_FIVE };
        public enum PressureType { PSI_60, PSI_20, PSI_10 };
        public enum StageType { SINGLE_STAGE_ELEVATOR, CASCADING_STAGE_1, CASCADING_STAGE_2, CONTINUOUS_STAGE_1, CONTINUOUS_STAGE_2 };

        /// <summary>
        /// ID of the Joint this attribute is attached to
        /// </summary>
        public readonly UInt32? JointID; 

        /// <summary>
        /// Type of Attribute: 0000-No Driver 0001-Motor 0002-Servo
        /// 0003-Bumper Pnuematic 0004-Relay Pneumatic 0005-Wormscrew 0006-DualMotor 0007-Elevator
        /// </summary>
        public readonly UInt16? AttribType; 

        /// <summary>
        /// True if the port is a CAN, False if the port is a PWM.
        /// </summary>
        public readonly bool? IsCAN; 

        /// <summary>
        /// CAN or PWM Port Number of the Joint
        /// </summary>
        public readonly float? Port1;
        public readonly float? Port2;

        
        public readonly bool? HasJointLimits;
        /// <summary>
        /// There is only Friction is HasJointLimits is true
        /// </summary>
        public readonly UInt16? Friction;

        public readonly bool? IsDriveWheel;
        /// <summary>
        /// Wheel int corresponds to type. See WheelType enum.
        /// </summary>
        public readonly UInt16? Wheel;

        public readonly float? InputGear;
        public readonly float? OutputGear;

        /// <summary>
        /// Internal Diameter of a Pneumatic. InternalDiameter int corresponds to the type. See IntneralDiameter enum.
        /// </summary>
        public readonly UInt16? InternalDiameter;

        /// <summary>
        /// Pressure of a Pneumatic. Pressure int corresponds to the type. See Pressure enum.
        /// </summary>
        public readonly UInt16? Pressure;

        public readonly bool? HasBrake;
        /// <summary>
        /// Stage of an elevator. Elevator int corresponds to the type. See Elevator enum. 
        /// </summary>
        public readonly UInt16? Stages;

        public JointAttribute(UInt32? jointID, UInt16? attribType, bool? isCAN, float? port1, float? port2, bool? hasJointLimits, 
            UInt16? friction, bool? isDriveWheel, UInt16? wheel, float? inputGear, float? outputGear, UInt16? internalDiameter, 
            UInt16? pressure, bool? hasBrake, UInt16? stages)
        {
            JointID = jointID;
            AttribType = attribType;
            IsCAN = isCAN;
            Port1 = port1;
            Port2 = port2;
            HasJointLimits = hasJointLimits;
            Friction = friction;
            IsDriveWheel = isDriveWheel;
            Wheel = wheel;
            InputGear = inputGear;
            OutputGear = outputGear;
            InternalDiameter = internalDiameter;
            Pressure = pressure;
            HasBrake = hasBrake;
            Stages = stages;
        }
    }
}
