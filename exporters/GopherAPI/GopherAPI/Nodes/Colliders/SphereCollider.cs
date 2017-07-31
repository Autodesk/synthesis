namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// A collider based on a scaled sphere
    /// </summary>
    public class SphereCollider : GopherCollider_Base
    {
        public override ColliderType GetAttribType()
        {
            return ColliderType.SPHERE_COLLIDER;
        }
        /// <summary>
        /// The scale of the SphereCollider
        /// </summary>
        public float Scale { get; internal set; }
    }
}
