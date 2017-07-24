namespace GopherAPI.Nodes.Joint.Driver
{
    public class RelayPnuematic : GopherDriver_Base
    {

        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }


        public float RelayPort { get; internal set; }

        public bool HasLimits { get; internal set; }
        public Friction Friction { get; internal set; }

        public InternalDiameter InternalDiameter { get; internal set; }
        public Pressure Pressure { get; internal set; }

        public override Driver GetDriverType()
        {
            return Driver.RELAY_PNUEMATIC;
        }

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public RelayPnuematic() { }

        //public RelayPnuematic(GopherJoint_Base joint, float relayPort, bool hasLimits, Friction friction, InternalDiameter internalDiameter, Pressure pressure)
        //{
        //    Joint = joint;
        //    RelayPort = relayPort;

        //    HasLimits = hasLimits;
        //    Friction = friction;

        //    InternalDiameter = internalDiameter;
        //    Pressure = pressure;
        //}
    }
}