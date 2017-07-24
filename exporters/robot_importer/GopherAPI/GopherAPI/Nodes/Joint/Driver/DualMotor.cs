using System;

namespace GopherAPI.Nodes.Joint.Driver
{
    public class DualMotor : GopherDriver_Base
    {
        public GopherJoint_Base Joint;

        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public bool IsCAN;

        public float PortOne;
        public float PortTwo;

        public bool HasLimits;
        public Friction Friction;

        public bool IsDriveWheel;
        public Wheel WheelType;

        public UInt16 InputGear;
        public UInt16 OutputGear;

        public override Driver GetDriverType()
        {
            return Driver.DUAL_MOTOR;
        }

        public override bool GetIsDriveWheel()
        {
            return IsDriveWheel;
        }


        public DualMotor(GopherJoint_Base joint, bool isCAN, float motorPort1, float motorPort2, bool hasLimits, Friction friction, bool isDriveWheel, Wheel wheelType, UInt16 inputGear, UInt16 outputGear)
        {
            Joint = joint;
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
}