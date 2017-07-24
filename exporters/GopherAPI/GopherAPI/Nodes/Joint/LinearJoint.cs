using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    public class LinearJoint : GopherJoint_Base
    {
        /// <summary>
        /// Gets the Joint type
        /// </summary>
        /// <returns></returns>
        public override GopherJointType GetJointType()
        {
            return GopherJointType.LINEAR;
        }

        /// <summary>
        /// Vector parallel to movement
        /// </summary>
        public Vec3 DefiningVector { get; internal set; }

        /// <summary>
        /// Point of connection relative to parent part
        /// </summary>
        public Vec3 ConnectionPoint { get; internal set; }

        /// <summary>
        /// cm of freedom relative to the defining vector
        /// </summary>
        public Vec2 LinearFreedomFactor { get; internal set; }
}
}