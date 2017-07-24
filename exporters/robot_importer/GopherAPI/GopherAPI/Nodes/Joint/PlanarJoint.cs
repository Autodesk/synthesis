using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    public class PlanarJoint : GopherJoint_Base
    {
        public override GopherJointType GetJointType()
        {
            return base.GetJointType();
        }

        /// <summary>
        /// Vector parallel to the plane
        /// </summary>
        public Vec3 PlanarVector { get; }

        /// <summary>
        /// Point of connection relative to the parent part
        /// </summary>
        public Vec3 ConnectionPoint { get; }

        /// <summary>
        /// Degree of angular freedom 
        /// </summary>
        public Vec2 AngularFreedomFactor { get; }

        /// <summary>
        /// cm of freedom parallel to the vector
        /// </summary>
        public Vec2 TransFreedomFactorPar { get; }

        /// <summary>
        /// cm of freedom perpendicular to the vector
        /// </summary>
        public Vec2 TransFreedomFactorPer { get; }
    }
}