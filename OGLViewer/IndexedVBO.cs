using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OGLViewer
{
    public class IndexedVBO
    {

        private double[] dataBuffer;
        private int dataBufferID;

        private int[][] indexBuffers; //Draw each surface individually
        private int[] indexBufferIDs;

        public IndexedVBO(BXDAMesh.BXDASubMesh mesh)
        {
            dataBuffer = new double[mesh.verts.Length * 3 + mesh.norms.Length * 3];

            for (int v = 0; v < mesh.verts.Length / 3; v += 3)
            {
                dataBuffer[v] = mesh.verts[v];
                dataBuffer[v + 1] = mesh.verts[v + 1];
                dataBuffer[v + 2] = mesh.verts[v + 2];
                dataBuffer[v + 3] = mesh.norms[v];
                dataBuffer[v + 4] = mesh.norms[v + 1];
                dataBuffer[v + 5] = mesh.norms[v + 2];
            }

            dataBufferID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, dataBufferID);

            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(dataBuffer.Length * 8), dataBuffer, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, 3 * 8, 0); //Vertices
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Double, false, 3 * 8, 3 * 8); //Normals

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            indexBuffers = new int[mesh.surfaces.Count][];
            indexBufferIDs = new int[mesh.surfaces.Count];
            GL.GenBuffers(indexBuffers.Length, indexBufferIDs);
            for (int s = 0; s < mesh.surfaces.Count; s++)
            {
                BXDAMesh.BXDASurface surf = mesh.surfaces[s];

                indexBuffers[s] = new int[surf.indicies.Length];
                for (int i = 0; i < surf.indicies.Length; i++)
                {
                    indexBuffers[s][i] = surf.indicies[i];
                }

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferIDs[s]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(indexBuffers[s].Length * 4), indexBuffers[s], BufferUsageHint.StaticDraw);
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Draw()
        {
            GL.EnableVertexAttribArray(0); //Vertices
            GL.EnableVertexAttribArray(1); //Normals

            for (int s = 0; s < indexBufferIDs.Length; s++)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferIDs[s]);
                GL.DrawElements(BeginMode.Triangles, indexBuffers[s].Length, DrawElementsType.UnsignedByte, 0);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            }

            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(0);
        }

        public void Destroy()
        {
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(dataBufferID);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DeleteBuffers(indexBufferIDs.Length, indexBufferIDs);
        }

    }
}
