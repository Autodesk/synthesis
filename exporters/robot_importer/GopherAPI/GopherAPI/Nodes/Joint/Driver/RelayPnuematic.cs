namespace GopherAPI.Nodes.Joint.Driver
{
    public class RelayPnuematic : GopherDriver_Base
    {
        public GopherJoint_Base Joint;
        public override GopherJoint_Base GetJoint()
        {
            return Joint;
        }


        public float RelayPort;

        public bool HasLimits;
        public Friction Friction;

        public InternalDiameter InternalDiameter;
        public Pressure Pressure;

        public override Driver GetDriverType()
        {
            return Driver.RELAY_PNUEMATIC;
        }

        public override bool GetIsDriveWheel()
        {
            return false;
        }

        public RelayPnuematic(GopherJoint_Base joint, float relayPort, bool hasLimits, Friction friction, InternalDiameter internalDiameter, Pressure pressure)
        {
            Joint = joint;
            RelayPort = relayPort;

            HasLimits = hasLimits;
            Friction = friction;

            InternalDiameter = internalDiameter;
            Pressure = pressure;
        }
    }
}