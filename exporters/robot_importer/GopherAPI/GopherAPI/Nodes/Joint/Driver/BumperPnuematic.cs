namespace GopherAPI.Nodes.Joint.Driver
{
    public class BumperPnuematic : GopherDriver_Base
    {
        public GopherJoint_Base Joint;

        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public float SolenoidPortOne;
        public float SolenoidPortTwo;

        public bool HasLimits;
        public Friction Friction;

        public InternalDiameter InternalDiameter;
        public Pressure Pressure;

        public override Driver GetDriverType()
        {
            return Driver.BUMPER_PNUEMATIC;
        }

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public BumperPnuematic(GopherJoint_Base joint, float solenoidPortOne, float solenoidPortTwo, bool hasLimits, Friction friction, InternalDiameter internalDiameter, Pressure pressure)
        {
            Joint = joint;
            SolenoidPortOne = solenoidPortOne;
            SolenoidPortTwo = solenoidPortTwo;

            HasLimits = hasLimits;
            Friction = friction;

            InternalDiameter = internalDiameter;
            Pressure = pressure;
        }
    }
}