namespace GopherAPI.Nodes.Joint.Driver
{
    public class NoDriver : GopherDriver_Base
    {
        private new DriverMeta Meta = null;
        public override GopherJoint_Base GetJoint()
        {
            return null;
        }
        public override Driver GetDriverType()
        {
            return Driver.NONE;
        }
        public NoDriver() { }
        
        public override bool GetIsDriveWheel()
        {
            return false;
        }
    }
}