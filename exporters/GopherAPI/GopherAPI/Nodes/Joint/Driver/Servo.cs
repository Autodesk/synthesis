namespace GopherAPI.Nodes.Joint.Driver
{
    public class Servo : GopherDriver_Base
    {


        /// <summary>
        /// Always a CAN
        /// </summary>
        public float MotorPort { get; internal set; }

        public bool HasLimits { get; internal set; }
        public Friction Friction { get; internal set; }

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

        public Servo() { }

        //public Servo(GopherJoint_Base joint, float motorPort, bool hasLimits, Friction friction)
        //{
        //    Joint = joint;
        //    MotorPort = motorPort;
        //    HasLimits = hasLimits;
        //    if (HasLimits)
        //        Friction = friction;
        //    else
        //        Friction = Friction.NO_LIMITS;
        //}
    }
}