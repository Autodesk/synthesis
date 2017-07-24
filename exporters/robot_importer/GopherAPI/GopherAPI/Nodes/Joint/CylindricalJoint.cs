using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    public class CylindricalJoint : GopherJoint_Base
    {
        /// <summary>
        /// Gets the Joint type
        /// </summary>
        /// <returns></returns>
        public override GopherJointType GetJointType()
        {
            return GopherJointType.CYLINDRICAL;
        }

        /// <summary>
        /// Vector normal to the plane of rotation
        /// </summary>
        public Vec3 NormalVector { get; }

        /// <summary>
        /// Vector parallel to movement
        /// </summary>
        public Vec3 DefiningVector { get; }

        /// <summary>
        /// Points of axes relative to parent part
        /// </summary>
        public Vec3 RelativePoint { get; }

        /// <summary>
        /// Point of connection relative to parent part
        /// </summary>
        public Vec3 ConnectionPoint { get; }

        /// <summary>
        /// Degree of angular freedom
        /// </summary>
        public Vec2 AngularFreedomFactor { get; }

        /// <summary>
        /// cm of freedom relative to the defining vector
        /// </summary>
        public Vec2 LinearFreedomFactor { get; }
    }
}