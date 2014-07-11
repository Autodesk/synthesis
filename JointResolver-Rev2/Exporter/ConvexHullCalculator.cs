using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIConvexHull;
public class ConvexHullCalculator
{
    public static List<DefaultConvexFace<DefaultVertex>> GetHullFaceList(BXDAMesh mesh, int faceLimit = 255)
    {
        List<double[]> pts = new List<double[]>();
        for (int a = 0; a < mesh.meshes.Count; a++)
        {
            pts.AddRange(ArrayUtilities.WrapArray<double[]>(delegate(double x, double y, double z)
            {
                return new double[] { x, y, z };
            }, mesh.meshes[a].verts));
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
        for (int i = 0; i < 2000 && faces.Count > 255; i++)
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
        return faces;
    }
}