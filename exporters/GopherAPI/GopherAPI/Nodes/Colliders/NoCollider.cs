namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// An empty collider. Used to group all nodes with no colliders.
    /// </summary>
    public class NoCollider : GopherCollider_Base
    {
        new private float Friction;
        new private bool IsDynamic;
        new private float Mass;

        public override ColliderType GetAttribType()
        {
            return ColliderType.NO_COLLIDER;
        }

        public NoCollider()
        {
            Meta = new ColliderMeta(ColliderType.NO_COLLIDER, uint.MaxValue);
        }
    }
}
