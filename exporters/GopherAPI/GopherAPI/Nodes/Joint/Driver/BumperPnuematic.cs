namespace GopherAPI.Nodes.Joint.Driver
{
    public class BumperPnuematic : GopherDriver_Base
    {


        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }

        public float SolenoidPortOne { get; internal set; }
        public float SolenoidPortTwo { get; internal set; }

        public bool HasLimits { get; internal set; }
        public Friction Friction { get; internal set; }

        public InternalDiameter InternalDiameter { get; internal set; }
        public Pressure Pressure { get; internal set; }

        public override Driver GetDriverType()
        {
            return Driver.BUMPER_PNUEMATIC;
        }

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public BumperPnuematic() { }

        //public BumperPnuematic(GopherJoint_Base joint, float solenoidPortOne, float solenoidPortTwo, bool hasLimits, Friction friction, InternalDiameter internalDiameter, Pressure pressure)
        //{
        //    Joint = joint;
        //    SolenoidPortOne = solenoidPortOne;
        //    SolenoidPortTwo = solenoidPortTwo;

        //    HasLimits = hasLimits;
        //    Friction = friction;

        //    InternalDiameter = internalDiameter;
        //    Pressure = pressure;
    }
}