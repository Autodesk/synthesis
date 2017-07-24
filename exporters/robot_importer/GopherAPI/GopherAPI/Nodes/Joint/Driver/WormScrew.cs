namespace GopherAPI.Nodes.Joint.Driver
{
    public class WormScrew : GopherDriver_Base
    {

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

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public override Driver GetDriverType()
        {
            return Driver.WORM_SCREW;
        }

        public WormScrew() { }
    }
}