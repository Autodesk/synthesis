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

        public override AttribType GetAttribType()
        {
            return AttribType.NO_COLLIDER;
        }
    }
}
