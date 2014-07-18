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
    public static List<BXDAMesh.BXDASubMesh> GetHull(BXDAMesh mesh, bool decompose = false)
    {
        return GetHull(mesh.meshes, decompose);
    }

    public static List<BXDAMesh.BXDASubMesh> GetHull(BXDAMesh.BXDASubMesh mesh, bool decompose = false)
    {
        return GetHull(new BXDAMesh.BXDASubMesh[] { mesh }, decompose);
    }

    private static BXDAMesh.BXDASubMesh ExportMeshInternal(float[] verts, uint vertCount, uint[] inds, uint trisCount)
    {
        BXDAMesh.BXDASubMesh sub = new BXDAMesh.BXDASubMesh();
        sub.colors = null;
        sub.textureCoords = null;
        sub.norms = null;

        sub.verts = new double[verts.Length];

        int[] facetCounts = new int[trisCount];

        for (uint i2 = 0; i2 < vertCount * 3; i2++)
        {
            sub.verts[i2] = verts[i2];
        }
        sub.indicies = new int[inds.Length];
        for (uint i2 = 0; i2 < trisCount; i2++)
        {
            uint off = i2 * 3;
            sub.indicies[off + 0] = (int) inds[off + 0] + 1;
            sub.indicies[off + 1] = (int) inds[off + 1] + 1;
            sub.indicies[off + 2] = (int) inds[off + 2] + 1;
        }

        return sub;
    }

    public static List<BXDAMesh.BXDASubMesh> GetHull(IEnumerable<BXDAMesh.BXDASubMesh> meshes, bool decompose = false)
    {
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
                copy[(vertCount * 3) + i] = (float) mesh.verts[i];
            }
            for (int i = 0; i < mesh.indicies.Length; i++)
            {
                index[indexCount + i] = (uint) (mesh.indicies[i] - 1 + vertCount);
            }
            indexCount += mesh.indicies.Length;
            vertCount += mesh.verts.Length / 3;
        }

        if (decompose)
        {
            ConvexAPI.iConvexDecomposition ic = new ConvexAPI.iConvexDecomposition();
            ic.setMesh((uint) copy.Length / 3, copy, (uint) index.Length / 3, index);

            ic.computeConvexDecomposition();

            while (!ic.isComputeComplete())
            {
                Console.WriteLine("Computing the convex decomposition in a background thread.");
                System.Threading.Thread.Sleep(1000);
            }

            uint hullCount = ic.getHullCount();
            Console.WriteLine("Convex Decomposition produced " + hullCount + " hulls.");

            List<BXDAMesh.BXDASubMesh> subs = new List<BXDAMesh.BXDASubMesh>();
            for (uint i = 0; i < hullCount; i++)
            {
                ConvexAPI.ConvexHullResult result = ic.getConvexHullResult(i);
                subs.Add(ExportMeshInternal(result.getVertices(), result.getVertexCount(), result.getIndicies(), result.getTriangleCount()));
            }
            return subs;
        }
        else
        {
            ConvexAPI.StandaloneConvexHull sch = new ConvexAPI.StandaloneConvexHull();
            sch.computeFor((uint) vertCount, copy);
            return new List<BXDAMesh.BXDASubMesh>(new BXDAMesh.BXDASubMesh[] { ExportMeshInternal(sch.getVertices(), sch.getVertexCount(), sch.getIndicies(), sch.getTriangleCount()) });
        }
    }
}