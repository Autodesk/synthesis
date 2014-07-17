using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MIConvexHull;
using Inventor;
using System.Collections;

public class ConvexHullCalculator
{
    public static BXDAMesh.BXDASubMesh GetHull(BXDAMesh mesh, int faceLimit = 255)
    {
        return GetHull(mesh.meshes, faceLimit);
    }

    public static BXDAMesh.BXDASubMesh GetHull(BXDAMesh.BXDASubMesh mesh, int faceLimit = 255)
    {
        return GetHull(new BXDAMesh.BXDASubMesh[] { mesh }, faceLimit);
    }

    public static BXDAMesh.BXDASubMesh GetHull(IEnumerable<BXDAMesh.BXDASubMesh> meshes, int faceLimit = 255)
    {
        IEnumerator<BXDAMesh.BXDASubMesh> en = meshes.GetEnumerator();
        en.MoveNext();
        BXDAMesh.BXDASubMesh subMesh = en.Current;

        ConvexAPI.iConvexDecomposition ic = new ConvexAPI.iConvexDecomposition();
        float[] copy = new float[subMesh.verts.Length];
        for (int i = 0; i < copy.Length; i++)
        {
            copy[i] = (float) subMesh.verts[i];
        }
        uint[] index = new uint[subMesh.indicies.Length];
        for (int i = 0; i < index.Length; i++)
        {
            index[i] = (uint) subMesh.indicies[i]-1;
        }
        Console.WriteLine("Start: " + (subMesh.verts.Length / 3) + ", " + (subMesh.indicies.Length / 3));
        ic.setMesh((uint) subMesh.verts.Length / 3, copy, (uint) subMesh.indicies.Length / 3, index);

        ic.computeConvexDecomposition();

        while (!ic.isComputeComplete())
        {
            Console.WriteLine("Computing the convex decomposition in a background thread.");
            System.Threading.Thread.Sleep(1000);
        }

        uint hullCount = ic.getHullCount();
        Console.WriteLine("Convex Decomposition produced " + hullCount + " hulls.");

        Console.WriteLine("Saving the convex hulls into a single Wavefront OBJ file 'hulls.obj'");
        uint vcount_base = 1;
        uint vcount_total = 0;
        uint tcount_total = 0;
        for (uint i = 0; i < hullCount; i++)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:/Temp/hulls-" + i + ".obj");
            ConvexAPI.ConvexHullResult result = ic.getConvexHullResult(i);
            float[] verts = result.getVertices();
            uint[] inds = result.getIndicies();
            vcount_total += result.getVertexCount();
            tcount_total += result.getTriangleCount();
            for (uint i2 = 0; i2 < result.getVertexCount(); i2++)
            {
                writer.WriteLine("v " + verts[i2 * 3] + " " + verts[i2 * 3 + 1] + " " + verts[i2 * 3 + 2]);
            }
            for (uint i2 = 0; i2 < result.getTriangleCount(); i2++)
            {
                uint b = i2 * 3;
                writer.WriteLine("f " + (inds[b] + vcount_base) + " " + (inds[b + 1] + vcount_base) + " " + (inds[b + 2] + vcount_base));
            }
            writer.Close();
            //vcount_base += result.getVertexCount();
        }
        Console.WriteLine("Output contains " + vcount_total + " vertices and " + tcount_total + " triangles.");
        return null;
    }
}