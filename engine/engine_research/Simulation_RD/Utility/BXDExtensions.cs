using OpenTK;
using System.Linq;
using System.Collections.Generic;

namespace Simulation_RD.Utility
{
    /// <summary>
    /// Things that convert BXD into Bullet/OpenGL
    /// </summary>
    public static class BXDExtensions
    {
        /// <summary>
        /// BXD Vector3 -> OpenGL Vector3
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Vector3 Convert(this BXDVector3 val)
        {
            return new Vector3(val.x, val.y, val.z);
        }

        /// <summary>
        /// BXD Quaternion -> OpenGL Quaternion
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static Quaternion Convert(this BXDQuaternion val)
        {
            return new Quaternion(val.X, val.Y, val.Z, val.W);
        }

        /// <summary>
        /// Gathers all the indices of the surfaces of a submesh
        /// </summary>
        /// <param name="subMesh"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetAllIndices(this BXDAMesh.BXDASubMesh subMesh)
        {
            IEnumerable<int> indices = new List<int>();
            subMesh.surfaces.ForEach((s) => indices = indices.Concat(s.indicies));
            return indices;
        }

        /// <summary>
        /// Gets all of the unordered vertex data from the submesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Vector3[] GetVertexData(this BXDAMesh.BXDASubMesh mesh)
        {
            return MeshUtilities.DataToVector(mesh.verts);
        }

        /// <summary>
        /// Gets all of the vertices referenced by the indices of all the surfaces in the mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Vector3[] GetindexedVertices(this BXDAMesh.BXDASubMesh mesh)
        {
            IEnumerable<int> indices = mesh.GetAllIndices();
            Vector3[] verts = mesh.GetVertexData();
            return (from int i in indices select verts[i]).ToArray();
        }

        /// <summary>
        /// Gets ALL of the collider vertices in a mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static IEnumerable<Vector3> AllColliderVertices(this BXDAMesh mesh)
        {
            return 
                (from BXDAMesh.BXDASubMesh sub in mesh.colliders select sub.GetindexedVertices())
                .Cast<IEnumerable<Vector3>>()
                .Aggregate(Enumerable.Concat);
        }
    }
}
