using GopherAPI.Other;

namespace GopherAPI.Nodes.Joint
{
    public class BallJoint : GopherJoint_Base
    {
        /// <summary>
        /// Gets the Joint type
        /// </summary>
        /// <returns></returns>
        public override GopherJointType GetJointType()
        {
            return GopherJointType.BALL;
        }

        /// <summary>
        /// Point of connection relative to the parent part
        /// </summary>
        public Vec3 ConnectionPoint { get; }
        
        /// <summary>
        /// Angular freedom of the joint
        /// </summary>
        Vec2 AngularFreedom1 { get; }

        /// <summary>
        /// Angular freedom of the joint
        /// </summary>
        Vec2 AngularFreedom2 { get; }

        /// <summary>
        /// Angular freedom of the joint
        /// </summary>
        Vec2 AngularFreedom3 { get; }
    }
}