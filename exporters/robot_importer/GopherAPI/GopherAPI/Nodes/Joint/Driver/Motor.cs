using System;

namespace GopherAPI.Nodes.Joint.Driver
{
    public class Motor : GopherDriver_Base
    {

        public bool IsCAN;
        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }
        public GopherJoint_Base Joint;

        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public float MotorPort;

        public bool HasLimits;
        public Friction Friction;

        public bool IsDriveWheel;
        public Wheel WheelType;

        public UInt16 InputGear;
        public UInt16 OutputGear;

        public override Driver GetDriverType()
        {
            return Driver.MOTOR;
        }

        public override bool GetIsDriveWheel()
        {
            return IsDriveWheel;
        }
        
        public Motor(GopherJoint_Base joint, bool isCAN, float motorPort, bool hasLimits, Friction friction, bool isDriveWheel, Wheel wheelType, UInt16 inputGear, UInt16 outputGear)
        {
            Joint = joint;
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
}