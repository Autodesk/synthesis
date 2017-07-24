namespace GopherAPI.Nodes.Joint.Driver
{
    public class Servo : GopherDriver_Base
    {
        public GopherJoint_Base Joint;

        /// <summary>
        /// Always a CAN
        /// </summary>
        public float MotorPort;

        public bool HasLimits;
        public Friction Friction;

        public override Driver GetDriverType()
        {
            return Driver.SERVO;
        }

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public Servo(GopherJoint_Base joint, float motorPort, bool hasLimits, Friction friction)
        {
            Joint = joint;
            MotorPort = motorPort;
            HasLimits = hasLimits;
            if (HasLimits)
                Friction = friction;
            else
                Friction = Friction.NO_LIMITS;
        }
    }
}