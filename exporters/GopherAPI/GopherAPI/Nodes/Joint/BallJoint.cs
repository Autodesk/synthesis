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
        public Vec3 ConnectionPoint { get; internal set; }

        /// <summary>
        /// Angular freedom of the joint. There are three because a ball joint can move in all 3 directions. Each AngularFreedom object is a direction relative to the parent part.
        /// </summary>
        public Vec2 AngularFreedom1 { get; internal set; }

        /// <summary>
        /// Angular freedom of the joint. There are three because a ball joint can move in all 3 directions. Each AngularFreedom object is a direction relative to the parent part.
        /// </summary>
        public Vec2 AngularFreedom2 { get; internal set; }

        /// <summary>
        /// Angular freedom of the joint. There are three because a ball joint can move in all 3 directions. Each AngularFreedom object is a direction relative to the parent part.
        /// </summary>
        public Vec2 AngularFreedom3 { get; internal set; }
    }
}