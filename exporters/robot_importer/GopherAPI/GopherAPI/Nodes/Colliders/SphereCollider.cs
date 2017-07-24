namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// A collider based on a scaled sphere
    /// </summary>
    public class SphereCollider : GopherCollider_Base
    {
        private float scale;

        public override AttribType GetAttribType()
        {
            return AttribType.SPHERE_COLLIDER;
        }
        /// <summary>
        /// The scale of the SphereCollider
        /// </summary>
        public float Scale => scale;
    }
}
