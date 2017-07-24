namespace GopherAPI.Nodes.Joint.Driver
{
    public class WormScrew : GopherDriver_Base
    {
        public GopherJoint_Base Joint;
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

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public override Driver GetDriverType()
        {
            return Driver.WORM_SCREW;
        }
        public WormScrew(GopherJoint_Base joint, bool isCAN, float motorPort, bool hasLimits, Friction friction)
        {
            Joint = joint;
            IsCAN = isCAN;
            MotorPort = motorPort;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;
        }
    }
}