using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIConvexHull;
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
        List<double[]> pts = new List<double[]>();
        foreach (BXDAMesh.BXDASubMesh mesh in meshes)
        {
            pts.AddRange(ArrayUtilities.WrapArray<double[]>(delegate(double x, double y, double z)
            {
                return new double[] { x, y, z };
            }, mesh.verts));
        }
        ConvexHull<DefaultVertex, DefaultConvexFace<DefaultVertex>> hull = MIConvexHull.ConvexHull.Create(pts);
        List<DefaultConvexFace<DefaultVertex>> faces = new List<DefaultConvexFace<DefaultVertex>>();
        foreach (DefaultConvexFace<DefaultVertex> face in hull.Faces)
        {
            if (face.Vertices.Length != 3)
            {
                Console.WriteLine("This wasn't a triangle!");
            }
            faces.Add(face);
        }
        faces.Sort();

        Console.WriteLine("Original: " + faces.Count);
        for (int i = 0; i < 2000 && faces.Count > faceLimit; i++)
        {
            DefaultConvexFace<DefaultVertex> remove = faces[0];
            // Square remove's verticies into the center.
            double[] center = new double[3];
            foreach (DefaultVertex v in remove.Vertices)
            {
                center[0] += v.Position[0] / 3.0;
                center[1] += v.Position[1] / 3.0;
                center[2] += v.Position[2] / 3.0;
            }
            DefaultVertex newVert = new DefaultVertex();
            newVert.Position = center;
            for (int k = faces.Count - 1; k >= 0; k--)
            {
                int matched = 0;
                DefaultConvexFace<DefaultVertex> face = faces[k];
                for (int j = 0; j < face.Vertices.Length; j++)
                {
                    if (face.Vertices[j].Position == remove.Vertices[0].Position ||
                        face.Vertices[j].Position == remove.Vertices[1].Position ||
                        face.Vertices[j].Position == remove.Vertices[2].Position)
                    {
                        face.Vertices[j] = newVert;
                        matched++;
                    }
                }
                if (matched >= 2)
                {
                    // Degenerate
                    faces.RemoveAt(k);
                }
            }
        }

        List<DefaultVertex> set = new List<DefaultVertex>();

        int[] indicies = new int[3 * faces.Count];
        int indexHead = 0;
        foreach (DefaultConvexFace<DefaultVertex> face in faces)
        {
            foreach (DefaultVertex vert in face.Vertices)
            {
                if (vert.indexNumber < 0)
                {
                    vert.indexNumber = set.Count;
                    set.Add(vert);
                    if (vert.normal == null)
                    {
                        vert.normal = new double[] { face.Normal[0], face.Normal[1], face.Normal[2] };
                    }
                    else
                    {
                        vert.normal[0] += face.Normal[0];
                        vert.normal[1] += face.Normal[1];
                        vert.normal[2] += face.Normal[2];
                    }
                    vert.faceCount++;
                }
                indicies[indexHead++] = vert.indexNumber;
            }
        }
        BXDAMesh.BXDASubMesh subMesh = new BXDAMesh.BXDASubMesh();
        subMesh.verts = new double[3 * set.Count];
        subMesh.norms = new double[3 * set.Count];
        subMesh.colors = null;
        subMesh.textureCoords = null;

        for (int vertHead = 0; vertHead < set.Count; vertHead++)
        {
            int vH = vertHead * 3;
            Array.Copy(set[vertHead].Position, 0, subMesh.verts, vH, 3);
            subMesh.norms[vH] = set[vertHead].normal[0] / (double) set[vertHead].faceCount;
            subMesh.norms[vH + 1] = set[vertHead].normal[1] / (double) set[vertHead].faceCount;
            subMesh.norms[vH + 2] = set[vertHead].normal[2] / (double) set[vertHead].faceCount;
        }

        subMesh.indicies = indicies;
        return subMesh;
    }
}