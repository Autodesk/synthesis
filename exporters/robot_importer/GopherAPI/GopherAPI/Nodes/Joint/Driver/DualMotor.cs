using System;

namespace GopherAPI.Nodes.Joint.Driver
{
    public class DualMotor : GopherDriver_Base
    {


        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public bool IsCAN { get; internal set; }

        public float PortOne { get; internal set; }
        public float PortTwo { get; internal set; }

        public bool HasLimits { get; internal set; }
        public Friction Friction { get; internal set; }

        public bool IsDriveWheel { get; internal set; }
        public Wheel WheelType { get; internal set; }

        public UInt16 InputGear { get; internal set; }
        public UInt16 OutputGear { get; internal set; }

        public override Driver GetDriverType()
        {
            return Driver.DUAL_MOTOR;
        }

        public override bool GetIsDriveWheel()
        {
            return IsDriveWheel;
        }

        public DualMotor() { }

        //public DualMotor(GopherJoint_Base joint, bool isCAN, float motorPort1, float motorPort2, bool hasLimits, Friction friction, bool isDriveWheel, Wheel wheelType, UInt16 inputGear, UInt16 outputGear)
        //{
        //    Joint = joint;
        //    IsCAN = isCAN;
        //    PortOne = motorPort1;
        //    PortTwo = motorPort2;
        //    HasLimits = hasLimits;
        //    if (HasLimits)
        //        Friction = friction;
        //    else
        //        Friction = Friction.NO_LIMITS;
        //    IsDriveWheel = isDriveWheel;
        //    WheelType = wheelType;
        //    InputGear = inputGear;
        //    OutputGear = outputGear;
        //}
    }
}