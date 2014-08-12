using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

public class VBOMesh
{
    private int[] bufferObjects = null;
    private BXDAMesh.BXDASubMesh subMesh;

    public VBOMesh(BXDAMesh.BXDASubMesh subMesh)
    {
        this.subMesh = subMesh;
    }

    public void loadToGPU()
    {
        unloadFromGPU();

        bufferObjects = new int[2 + subMesh.surfaces.Count];
        Gl.glGenBuffersARB(bufferObjects.Length, bufferObjects);

        Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, bufferObjects[0]);
        Gl.glBufferDataARB(Gl.GL_ARRAY_BUFFER, subMesh.verts.Length * sizeof(double), subMesh.verts,
                Gl.GL_STATIC_DRAW);

        Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, bufferObjects[1]);
        Gl.glBufferDataARB(Gl.GL_ARRAY_BUFFER, subMesh.norms.Length * sizeof(double), subMesh.norms,
                Gl.GL_STATIC_DRAW);

        for (int i = 0; i < subMesh.surfaces.Count; i++)
        {
            Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, bufferObjects[2 + i]);
            Gl.glBufferDataARB(Gl.GL_ARRAY_BUFFER, subMesh.surfaces[i].indicies.Length * sizeof(int), subMesh.surfaces[i].indicies,
                    Gl.GL_STATIC_DRAW);
        }
        Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, 0);
    }

    public void unloadFromGPU()
    {
        if (bufferObjects != null && bufferObjects.Length > 0)
        {
            Console.WriteLine("Deleting");
            Gl.glDeleteBuffersARB(bufferObjects.Length, bufferObjects);
            bufferObjects = null;
        }
    }


    public void draw()
    {
        if (bufferObjects == null)
        {
            loadToGPU();
        }
        Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
        Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);

        Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, bufferObjects[0]);
        Gl.glVertexPointer(3, Gl.GL_DOUBLE, 0, IntPtr.Zero);

        Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, bufferObjects[1]);
        Gl.glNormalPointer(Gl.GL_DOUBLE, 0, IntPtr.Zero);

        for (int i = 0; i < bufferObjects.Length - 2; i++)
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
            Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_DIFFUSE, color);
            Gl.glMaterialfv(Gl.GL_FRONT_AND_BACK, Gl.GL_SPECULAR, new float[] { subMesh.surfaces[i].specular, subMesh.surfaces[i].specular, subMesh.surfaces[i].specular, subMesh.surfaces[i].specular });
            Gl.glBindBufferARB(Gl.GL_ELEMENT_ARRAY_BUFFER, bufferObjects[i + 2]);
            Gl.glDrawElements(Gl.GL_TRIANGLES, subMesh.surfaces[i].indicies.Length,
                    Gl.GL_UNSIGNED_INT, IntPtr.Zero);
        }

        Gl.glBindBufferARB(Gl.GL_ARRAY_BUFFER, 0);
        Gl.glBindBufferARB(Gl.GL_ELEMENT_ARRAY_BUFFER, 0);
        Gl.glDisableClientState(Gl.GL_VERTEX_ARRAY);
        Gl.glDisableClientState(Gl.GL_NORMAL_ARRAY);
    }
}
