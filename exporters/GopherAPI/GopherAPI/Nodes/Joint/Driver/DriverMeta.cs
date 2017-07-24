namespace GopherAPI.Nodes.Joint.Driver
{
    /// <summary>
    /// A class containing metadata for the joint drivers
    /// </summary>
    public class DriverMeta
    {
        public uint JointID { get; }
        internal DriverMeta(uint JointID)
        {
            this.JointID = JointID;
        }
    }
}