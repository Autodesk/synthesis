using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using OpenTK;

namespace Simulation_RD.Utility
{
    /// <summary>
    /// Provides some static functions for manipulation of triangle meshes
    /// </summary>
    public static class MeshUtilities
    {
        /// <summary>
        /// Converts the simulation API data into a vector3[]
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Vector3[] DataToVector(double[] data)
        {
            Vector3[] toReturn = new Vector3[data.Length / 3];

            for (int i = 0; i < data.Length; i += 3)
            {
                toReturn[i / 3] = new Vector3(
                    (float)data[i],
                    (float)data[i + 1],
                    (float)data[i + 2]
                    );
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a bullet mesh shape given a BXDA sub mesh and a list of vectors to index from
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static StridingMeshInterface BulletShapeFromSubMesh(BXDAMesh.BXDASubMesh subMesh, Vector3[] vertices)
        {
            IEnumerable<int> indices = new List<int>();
            subMesh.surfaces.ForEach((s) => indices = indices.Concat(s.indicies));
            Vector3 center = MeshCenter(vertices);
            return new TriangleIndexVertexArray(indices.ToArray(), (from Vector3 v in vertices select v - center).ToArray());
        }

        /// <summary>
        /// Returns a bullet mesh centered around just the vertices used in the subMesh
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static StridingMeshInterface CenteredBulletShapeFromMesh(BXDAMesh.BXDASubMesh subMesh, Vector3[] vertices)
        {
            IEnumerable<int> indices = new List<int>();
            subMesh.surfaces.ForEach((s) => indices = indices.Concat(s.indicies));

            IEnumerable<Vector3> usedVertices = from int i in indices select vertices[i];
            Vector3 center = MeshCenter(usedVertices);

            return new TriangleIndexVertexArray(indices.ToArray(), (from Vector3 v in usedVertices select v - center).ToArray());
        }

        /// <summary>
        /// Gets the center vertex of some vertices
        /// </summary>
        /// <param name="mesh">find the center of this</param>
        /// <returns></returns>
        public static Vector3 MeshCenter(IEnumerable<Vector3> vertices)
        {
            return vertices.Aggregate(Vector3.Add) / vertices.Count();
        }

        /// <summary>
        /// Gets the center of a mesh's vertices
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public static Vector3 MeshCenter(BXDAMesh mesh)
        {
            return mesh.colliders.ConvertAll(m => DataToVector(m.verts).Aggregate(Vector3.Add) / (m.verts.Length / 3)).Aggregate(Vector3.Add) / mesh.colliders.Count;
        }
    }
}
