using GopherAPI.Other;

namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// A collider based on a box of a defined size
    /// </summary>
    public class BoxCollider : GopherCollider_Base
    {
        private Vec3 scale;

        /// <summary>
        /// A Vec3 containing the X, Y, and Z Scale of the Collider
        /// </summary>
        public Vec3 Scale => scale;
        public override AttribType GetAttribType()
        {
            return AttribType.BOX_COLLIDER;
        }
    }
}
