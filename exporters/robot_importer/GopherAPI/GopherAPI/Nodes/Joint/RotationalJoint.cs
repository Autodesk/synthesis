using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    public class RotationalJoint : GopherJoint_Base
    {
        /// <summary>
        /// Gets the Joint type
        /// </summary>
        /// <returns></returns>
        public override GopherJointType GetJointType()
        {
            return GopherJointType.ROTATIONAL;
        }

        /// <summary>
        /// Vector normal to the plane of rotation
        /// </summary>
        public Vec3 NormalVector { get; internal set; }

        /// <summary>
        /// Point of axes relative to parent part
        /// </summary>
        public Vec3 RelativePoint { get; internal set; }

        /// <summary>
        /// Degree of angular freedom
        /// </summary>
        public Vec2 AngularFreedomFactor { get; internal set; }
    }
}