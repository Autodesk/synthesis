using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace OGLViewer
{
    public class VBOMesh
    {
        private int[] bufferObjects = null;
        public BXDAMesh.BXDASubMesh subMesh;

        public VBOMesh(BXDAMesh.BXDASubMesh subMesh)
        {
            this.subMesh = subMesh;
        }

        public void loadToGPU()
        {
            unloadFromGPU();

            bufferObjects = new int[2 + subMesh.surfaces.Count];
            GL.GenBuffers(bufferObjects.Length, bufferObjects);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[0]);
            GL.BufferData<double>(BufferTarget.ArrayBuffer, new IntPtr(subMesh.verts.Length * sizeof(double)), subMesh.verts, BufferUsageHint.StaticDraw);

            if (subMesh.norms != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[1]);
                GL.BufferData<double>(BufferTarget.ArrayBuffer, new IntPtr(subMesh.norms.Length * sizeof(double)), subMesh.norms,
                        BufferUsageHint.StaticDraw);
            }

            for (int i = 0; i < subMesh.surfaces.Count; i++)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[2 + i]);
                GL.BufferData<int>(BufferTarget.ArrayBuffer, new IntPtr(subMesh.surfaces[i].indicies.Length * sizeof(int)),
                    subMesh.surfaces[i].indicies, BufferUsageHint.StaticDraw);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void unloadFromGPU()
        {
            if (bufferObjects != null && bufferObjects.Length > 0)
            {
                GL.DeleteBuffers(bufferObjects.Length, bufferObjects);
                bufferObjects = null;
            }
        }


        public void draw(bool effects = true)
        {
            if (bufferObjects == null)
            {
                loadToGPU();
            }
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[0]);
            GL.VertexPointer(3, VertexPointerType.Double, 0, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[1]);
            GL.NormalPointer(NormalPointerType.Double, 0, IntPtr.Zero);

            for (int i = 0; i < bufferObjects.Length - 2; i++)
            {
                if (effects)
                {
                    uint val = subMesh.surfaces[i].hasColor ? subMesh.surfaces[i].color : 0xFFFFFFFF;
                    float[] color = { (val & 0xFF) / 255f, ((val >> 8) & 0xFF) / 255f, ((val >> 16) & 0xFF) / 255f, ((val >> 24) & 0xFF) / 255f };
                    if (subMesh.surfaces[i].transparency != 0)
                    {
                        color[3] = subMesh.surfaces[i].transparency;
                    }
                    else if (subMesh.surfaces[i].translucency != 0)
                    {
                        color[3] = subMesh.surfaces[i].translucency;
                    }
                    if (color[3] == 0)   // No perfectly transparent things plz.
                    {
                        color[3] = 1;
                    }
                    if (color[3] != 1 && effects)
                    {
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                    }
                    else
                    {
                        GL.Disable(EnableCap.Blend);
                    }
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, color);
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new float[] { 1, 1, 1, 1 });
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, subMesh.surfaces[i].specular);
                }
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferObjects[i + 2]);
                GL.DrawElements(PrimitiveType.Triangles, subMesh.surfaces[i].indicies.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }

        internal void destroy()
        {
            GL.DeleteBuffers(bufferObjects.Length, bufferObjects);
            bufferObjects = new int[0];
        }
    }
}
