namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// A collider based on the mesh of its attached node(s)
    /// </summary>
    public class MeshCollider : GopherCollider_Base
    {
        public override ColliderType GetAttribType()
        {
            return ColliderType.MESH_COLLIDER;
        }
    }
}
