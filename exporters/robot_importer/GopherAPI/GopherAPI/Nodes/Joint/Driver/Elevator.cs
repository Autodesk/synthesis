namespace GopherAPI.Nodes.Joint.Driver
{
    public class Elevator : GopherDriver_Base
    {


        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public bool IsCAN { get; internal set; }
        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public float MotorPort { get; internal set; }

        public bool HasLimits { get; internal set; }
        public Friction Friction { get; internal set; }

        public bool HasBrake { get; internal set; }
        public float BrakePortOne { get; internal set; }
        public float BrakePortTwo { get; internal set; }

        public Stages Stages { get; internal set; }
        public float InputGear { get; internal set; }
        public float OutputGear { get; internal set; }

        public override Driver GetDriverType()
        {
            return Driver.ELEVATOR;
        }

        public Elevator() { }

        //public Elevator(GopherJoint_Base joint, bool isCAN, float motorPort, bool hasLimits, Friction friction, bool hasBrake, float brakePortOne, float brakePortTwo, Stages stages, float inputGear, float outputGear)
        //{
        //    Joint = joint;
        //    IsCAN = isCAN;
        //    MotorPort = motorPort;
        //    HasLimits = hasLimits;
        //    if (HasLimits)
        //        Friction = friction;
        //    else
        //        Friction = Friction.NO_LIMITS;

        //    HasBrake = hasBrake;
        //    BrakePortOne = brakePortOne;
        //    BrakePortTwo = brakePortTwo;

        //    Stages = stages;

        //    InputGear = inputGear;
        //    OutputGear = outputGear;
        //}

    }
}