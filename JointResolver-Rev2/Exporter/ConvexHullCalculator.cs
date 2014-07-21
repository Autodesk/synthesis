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

    private class SimplificationVertex
    {
        public uint finalIndex;
        public float[] pos = new float[3];
        public List<SimplificationFace> faces = new List<SimplificationFace>();

        public SimplificationVertex(float x, float y, float z)
        {
            pos[0] = x;
            pos[1] = y;
            pos[2] = z;
        }
    }
    private class SimplificationFace : IComparable<SimplificationFace>
    {
        public SimplificationVertex[] verts = new SimplificationVertex[3];
        public float area;
        public float partnerNorms;
        public float[] norm;

        public bool containsVert(SimplificationVertex v)
        {
            foreach (SimplificationVertex j in verts)
            {
                if (j == v)
                {
                    return true;
                }
            }
            return false;
        }

        public void updateInfo()
        {
            // Calc area
            float[] p1 = new float[] { verts[0].pos[0] - verts[1].pos[0], verts[0].pos[0] - verts[1].pos[1], verts[0].pos[2] - verts[1].pos[2] };
            float[] p2 = new float[] { verts[2].pos[0] - verts[1].pos[0], verts[2].pos[0] - verts[1].pos[1], verts[2].pos[2] - verts[1].pos[2] };
            float[] cross = new float[]{(p1[1]*p2[2])-(p1[2]*p2[1]),
                (p1[2]*p2[0])-(p1[0]*p2[2]),
                (p1[0]*p2[1])-(p1[1]*p2[0])};
            float normMag = (float) Math.Sqrt((cross[0] * cross[0]) + (cross[1] * cross[1]) + (cross[2] * cross[2]));
            area = normMag;
            norm = cross;
            norm[0] /= normMag;
            norm[1] /= normMag;
            norm[2] /= normMag;
        }

        public void updateNeighbors()
        {
            partnerNorms = 0.05f;    // Every time you divide by zero you kill a kitten
            List<SimplificationFace> yay = new List<SimplificationFace>();
            for (int v = 0; v < 3; v++)
            {
                foreach (SimplificationFace face in verts[v].faces)
                {
                    for (int v2 = 0; v2 < 3 && face != this; v2++)
                    {
                        if (v2 != v && face.verts[v2].faces.Contains(this))
                        {
                            yay.Add(face);
                        }
                    }
                }
            }
            foreach (SimplificationFace ff in yay)
            {
                // Dot them!
                partnerNorms += Math.Abs((ff.norm[0] * norm[0]) + (ff.norm[1] * norm[1]) + (ff.norm[2] * norm[2]));
            }
        }

        int IComparable<SimplificationFace>.CompareTo(SimplificationFace other)
        {
            // The best ones have coplanar neighbors.
            return (area/partnerNorms).CompareTo(area / other.partnerNorms);
        }
    }
    private static void Simplify(ref float[] verts, ref uint vertCount, ref uint[] inds, ref  uint trisCount)
    {
        List<SimplificationVertex> simplVerts = new List<SimplificationVertex>();
        for (int i = 0; i < vertCount; i++)
        {
            simplVerts.Add(new SimplificationVertex(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]));
        }
        List<SimplificationFace> simplFace = new List<SimplificationFace>();
        for (int i = 0; i < trisCount * 3; i += 3)
        {
            SimplificationFace face = new SimplificationFace();
            face.verts[0] = simplVerts[(int) inds[i]];
            face.verts[1] = simplVerts[(int) inds[i + 1]];
            face.verts[2] = simplVerts[(int) inds[i + 2]];
            foreach (SimplificationVertex v in face.verts)
            {
                v.faces.Add(face);
            }
            simplFace.Add(face);
        }
        foreach (SimplificationFace face in simplFace)
        {
            face.updateInfo();
        }
        foreach (SimplificationFace face in simplFace)
        {
            face.updateNeighbors();
        }

        simplFace.Sort();
        Console.WriteLine("Starts at " + simplFace.Count);
        // Time for shenanigans!  We are going to naively pick the smallest triangle and then pretend it isn't there.
        for (int i = 0; i < 2000 && simplFace.Count > 50; i++)
        {
            Console.WriteLine("Simply from " + simplFace.Count);
            SimplificationFace remove = simplFace[0];
            float[] center = new float[3];
            foreach (SimplificationVertex vert in remove.verts)
            {
                center[0] += vert.pos[0] / 3.0f;
                center[1] += vert.pos[1] / 3.0f;
                center[2] += vert.pos[2] / 3.0f;
            }
            SimplificationVertex newVertex = new SimplificationVertex(center[0], center[1], center[2]);
            for (int k = simplFace.Count - 1; k >= 0; k--)
            {
                int matched = 0;
                SimplificationFace face = simplFace[k];
                for (int j = 0; j < face.verts.Length; j++)
                {
                    if (face.verts[j] == remove.verts[0] ||
                        face.verts[j] == remove.verts[1] ||
                        face.verts[j] == remove.verts[2])
                    {
                        face.verts[j] = newVertex;
                        if (matched == 0)
                        {
                            newVertex.faces.Add(face);
                        }
                        matched++;
                    }
                }
                if (matched >= 2)
                {
                    // Degenerate
                    simplFace.RemoveAt(k);
                    foreach (SimplificationVertex v in face.verts)
                    {
                        v.faces.Remove(face);
                    }
                }
                else if (matched == 1)
                {
                    face.updateInfo();
                }
            }
            simplVerts.Add(newVertex);
            foreach (SimplificationFace face in simplFace)
            {
                face.updateNeighbors();
            }
            simplFace.Sort();
        }

        simplVerts.RemoveAll((vert) => vert.faces.Count <= 0);

        // Rebuild arrays
        System.IO.StreamWriter writer = new System.IO.StreamWriter(new System.IO.FileStream("C:/Temp/test.obj", System.IO.FileMode.Create));

        vertCount = (uint) simplVerts.Count;
        verts = new float[vertCount * 3];
        for (int i = 0; i < simplVerts.Count; i++)
        {
            int off = i * 3;
            simplVerts[i].finalIndex = (uint) (i + 1);
            Console.WriteLine(simplVerts[i].finalIndex);
            Array.Copy(simplVerts[i].pos, 0, verts, off, 3);
            writer.WriteLine("v " + simplVerts[i].pos[0] + " " + simplVerts[i].pos[1] + "  " + simplVerts[i].pos[2]);
        }
        trisCount = (uint) simplFace.Count;
        inds = new uint[trisCount * 3];
        for (int i = 0; i < simplFace.Count; i++ )
        {
            int off = i * 3;
            inds[off] = simplFace[i].verts[0].finalIndex;
            inds[off+1] = simplFace[i].verts[1].finalIndex;
            inds[off+2] = simplFace[i].verts[2].finalIndex;
            writer.WriteLine("f " + inds[off] + "  " + inds[off+1] + "  " + inds[off+2]);
        }

        writer.Close();
    }

    private static BXDAMesh.BXDASubMesh ExportMeshInternal(float[] verts, uint vertCount, uint[] inds, uint trisCount)
    {
        Simplify(ref verts, ref vertCount, ref inds, ref trisCount);
        BXDAMesh.BXDASubMesh sub = new BXDAMesh.BXDASubMesh();
        sub.colors = null;
        sub.textureCoords = null;
        sub.norms = null;

        sub.verts = new double[verts.Length];

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