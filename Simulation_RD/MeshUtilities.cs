using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using OpenTK;

namespace Simulation_RD
{
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
            TriangleMesh tMesh = new TriangleMesh();
            foreach (BXDAMesh.BXDASurface surf in subMesh.surfaces)
            {
                TriangleIndexVertexArray vertexArray = new TriangleIndexVertexArray(surf.indicies, vertices);
                MultimaterialTriangleMeshShape m = new MultimaterialTriangleMeshShape(vertexArray, true);
                
            }
            return tMesh;
        }
    }
}
