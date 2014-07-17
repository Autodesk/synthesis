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
        ConvexAPI.iConvexDecomposition ic = new ConvexAPI.iConvexDecomposition();
        int vertCount = 0;
        int indexCount = 0;
        foreach (BXDAMesh.BXDASubMesh mesh in meshes)
        {
            vertCount += mesh.verts.Length;
            indexCount += mesh.indicies.Length;
        }
        float[] copy = new float[vertCount];
        uint[] index = new uint[indexCount];
        vertCount = 0;
        indexCount = 0;
        foreach (BXDAMesh.BXDASubMesh mesh in meshes)
        {
            for (int i = 0; i < mesh.verts.Length; i++)
            {
                copy[vertCount + i] = (float) mesh.verts[i];
            }
            for (int i = 0; i < mesh.indicies.Length; i++)
            {
                index[indexCount + i] = (uint) (mesh.indicies[i] - 1 + (vertCount/3));
            }
            indexCount += mesh.indicies.Length;
            vertCount += mesh.verts.Length;
        }

        ic.setMesh((uint) copy.Length / 3, copy, (uint) index.Length / 3, index);

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
            System.IO.StreamWriter writer = new System.IO.StreamWriter("C:/Temp/out/hulls-" + i + ".obj");
            ConvexAPI.ConvexHullResult result = ic.getConvexHullResult(i);
            uint trisCount = result.getTriangleCount();
            uint vertCount2 = result.getVertexCount();
            float[] verts = result.getVertices();
            uint[] inds = result.getIndicies();
            vcount_total += vertCount2;
            tcount_total += trisCount;
            for (uint i2 = 0; i2 < vertCount2; i2++)
            {
                writer.WriteLine("v " + verts[i2 * 3] + " " + verts[i2 * 3 + 1] + " " + verts[i2 * 3 + 2]);
            }
            for (uint i2 = 0; i2 < trisCount; i2++)
            {
                uint b = i2 * 3;
                writer.WriteLine("f " + (inds[b] + vcount_base) + " " + (inds[b + 1] + vcount_base) + " " + (inds[b + 2] + vcount_base));
            }
            writer.Close();
        }
        Console.WriteLine("Output contains " + vcount_total + " vertices and " + tcount_total + " triangles.");
        return null;
    }
}