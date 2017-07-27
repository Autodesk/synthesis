using System;

namespace GopherAPI.Nodes.Joint.Driver
{
    public class Motor : GopherDriver_Base
    {

        public bool IsCAN { get; internal set; }
        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }


        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public float MotorPort { get; internal set; }

        public bool HasLimits { get; internal set; }
        public Friction Friction { get; internal set; }

        public bool IsDriveWheel { get; internal set; }
        public Wheel WheelType { get; internal set; }

        public UInt16 InputGear { get; internal set; }
        public UInt16 OutputGear { get; internal set; }

        public override Driver GetDriverType()
        {
            return Driver.MOTOR;
        }

        public override bool GetIsDriveWheel()
        {
            return IsDriveWheel;
        }

        public Motor() { }
        
        //public Motor(GopherJoint_Base joint, bool isCAN, float motorPort, bool hasLimits, Friction friction, bool isDriveWheel, Wheel wheelType, UInt16 inputGear, UInt16 outputGear)
        //{
        //    Joint = joint;
        //    IsCAN = isCAN;
        //    MotorPort = motorPort;
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