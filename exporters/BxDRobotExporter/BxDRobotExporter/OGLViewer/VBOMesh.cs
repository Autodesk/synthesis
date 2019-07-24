using System;
using OpenTK.Graphics.OpenGL;

namespace BxDRobotExporter.OGLViewer
{
    public class VboMesh
    {
        private int[] bufferObjects = null;
        public BXDAMesh.BXDASubMesh SubMesh;

        public VboMesh(BXDAMesh.BXDASubMesh subMesh)
        {
            this.SubMesh = subMesh;
        }

        public void LoadToGpu()
        {
            UnloadFromGpu();

            bufferObjects = new int[2 + SubMesh.surfaces.Count];
            GL.GenBuffers(bufferObjects.Length, bufferObjects);

            GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[0]);
            GL.BufferData<double>(BufferTarget.ArrayBuffer, new IntPtr(SubMesh.verts.Length * sizeof(double)), SubMesh.verts, BufferUsageHint.StaticDraw);

            if (SubMesh.norms != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[1]);
                GL.BufferData<double>(BufferTarget.ArrayBuffer, new IntPtr(SubMesh.norms.Length * sizeof(double)), SubMesh.norms,
                        BufferUsageHint.StaticDraw);
            }

            for (int i = 0; i < SubMesh.surfaces.Count; i++)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, bufferObjects[2 + i]);
                GL.BufferData<int>(BufferTarget.ArrayBuffer, new IntPtr(SubMesh.surfaces[i].indicies.Length * sizeof(int)),
                    SubMesh.surfaces[i].indicies, BufferUsageHint.StaticDraw);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void UnloadFromGpu()
        {
            if (bufferObjects != null && bufferObjects.Length > 0)
            {
                GL.DeleteBuffers(bufferObjects.Length, bufferObjects);
                bufferObjects = null;
            }
        }


        public void Draw(bool effects = true)
        {
            if (bufferObjects == null)
            {
                LoadToGpu();
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
                    uint val = SubMesh.surfaces[i].hasColor ? SubMesh.surfaces[i].color : 0xFFFFFFFF;
                    float[] color = { (val & 0xFF) / 255f, ((val >> 8) & 0xFF) / 255f, ((val >> 16) & 0xFF) / 255f, ((val >> 24) & 0xFF) / 255f };
                    if (SubMesh.surfaces[i].transparency != 0)
                    {
                        color[3] = SubMesh.surfaces[i].transparency;
                    }
                    else if (SubMesh.surfaces[i].translucency != 0)
                    {
                        color[3] = SubMesh.surfaces[i].translucency;
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
                    GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, SubMesh.surfaces[i].specular);
                }
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, bufferObjects[i + 2]);
                GL.DrawElements(PrimitiveType.Triangles, SubMesh.surfaces[i].indicies.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }

        internal void Destroy()
        {
            GL.DeleteBuffers(bufferObjects.Length, bufferObjects);
            bufferObjects = new int[0];
        }
    }
}
