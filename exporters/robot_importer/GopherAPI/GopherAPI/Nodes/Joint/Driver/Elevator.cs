namespace GopherAPI.Nodes.Joint.Driver
{
    public class Elevator : GopherDriver_Base
    {
        public GopherJoint_Base Joint;

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public bool IsCAN;
        /// <summary>
        /// If IsCAN is false, this is a PWM port. Otherwise, its a CAN port.
        /// </summary>
        public float MotorPort;

        public bool HasLimits;
        public Friction Friction;

        public bool HasBrake;
        public float BrakePortOne;
        public float BrakePortTwo;

        public Stages Stages;
        public float InputGear;
        public float OutputGear;

        public override Driver GetDriverType()
        {
            return Driver.ELEVATOR;
        }

        public Elevator(GopherJoint_Base joint, bool isCAN, float motorPort, bool hasLimits, Friction friction, bool hasBrake, float brakePortOne, float brakePortTwo, Stages stages, float inputGear, float outputGear)
        {
            Joint = joint;
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