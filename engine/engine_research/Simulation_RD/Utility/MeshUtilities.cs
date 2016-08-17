using System.Collections.Generic;
using System.Linq;
using System;
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
        public static StridingMeshInterface BulletShapeFromSubMesh(BXDAMesh.BXDASubMesh subMesh)
        {
            return new TriangleIndexVertexArray(subMesh.GetAllIndices().ToArray(), subMesh.GetVertexData());
        }

        /// <summary>
        /// Returns a bullet mesh shape offset relative to its starting position
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="vertices"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static StridingMeshInterface BulletShapeFromSubMesh(BXDAMesh.BXDASubMesh subMesh, Vector3 offset)
        {
            Vector3[] vertices = subMesh.GetVertexData();
            return new TriangleIndexVertexArray(subMesh.GetAllIndices().ToArray(), (from Vector3 v in vertices select v + offset).ToArray());
        }

        /// <summary>
        /// Returns a bullet mesh centered around the origin
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static StridingMeshInterface CenteredBulletShapeFromSubMesh(BXDAMesh.BXDASubMesh subMesh)
        {
            IEnumerable<int> indices = subMesh.GetAllIndices();
            Vector3[] vertices = subMesh.GetVertexData();
            
            Vector3 center = MeshCenter(from int i in indices select vertices[i]);

            return BulletShapeFromSubMesh(subMesh, -center);
        }

        /// <summary>
        /// Gets the center of some vertices
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
            IEnumerable<Vector3> verts = mesh.AllColliderVertices();
            return verts.Aggregate(Vector3.Add) / verts.Count();
        }

        /// <summary>
        /// Gets the center of a sub mesh that uses the given vertices
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static Vector3 MeshCenter(BXDAMesh.BXDASubMesh subMesh)
        {
            IEnumerable<int> indices = subMesh.GetAllIndices();
            Vector3[] vertices = subMesh.GetVertexData();
            return (from int i in indices select vertices[i]).Aggregate(Vector3.Add) / indices.Count();
        }

        /// <summary>
        /// Gets the center of a sub mesh relative to the entire BXDA mesh
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static Vector3 MeshCenterRelative(BXDAMesh.BXDASubMesh subMesh, BXDAMesh mesh)
        {
            return MeshCenter(subMesh) - MeshCenter(mesh.AllColliderVertices());
        }
    }
}
