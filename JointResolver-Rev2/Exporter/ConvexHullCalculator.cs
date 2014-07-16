using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MIConvexHull;
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
        double[] major = new double[9 * faces.Count];
        for (int a = 0; a < faces.Count; a++)
        {
            int off = a * 9;
            faces[a].tmpHead = off;
            Array.Copy(faces[a].Vertices[0].Position, 0, major, off, 3);
            Array.Copy(faces[a].Vertices[1].Position, 0, major, off + 3, 3);
            Array.Copy(faces[a].Vertices[2].Position, 0, major, off + 6, 3);
        }
        double[] minor = new double[9];


        AssemblyDocument part = ((AssemblyDocument) Program.INVENTOR_APPLICATION.ActiveDocument);
        GraphicsDataSets dataSets;
        try
        {
            dataSets = part.GraphicsDataSetsCollection.AddNonTransacting("convexHULL");
        }
        catch
        {
            dataSets = part.GraphicsDataSetsCollection["convexHULL"];
            dataSets.Delete();
            dataSets = part.GraphicsDataSetsCollection.AddNonTransacting("convexHULL");
        }
        ClientGraphics graphics;
        try
        {
            graphics = part.ComponentDefinition.ClientGraphicsCollection.AddNonTransacting("convexHULL");
        }
        catch
        {
            graphics = part.ComponentDefinition.ClientGraphicsCollection["convexHULL"];
            graphics.Delete();
            graphics = part.ComponentDefinition.ClientGraphicsCollection.AddNonTransacting("convexHULL");
        }
        GraphicsCoordinateSet majorCoordSet;
        {
            GraphicsNode node = graphics.AddNode(graphics.Count + 1);
            majorCoordSet =
            dataSets.CreateCoordinateSet(dataSets.Count + 1);
            majorCoordSet.PutCoordinates(major);
            LineGraphics primitive = node.AddLineGraphics();
            primitive.BurnThrough = false;
            //Create Coordinate Index Set 
            GraphicsIndexSet indexSetCoords =
            dataSets.CreateIndexSet(dataSets.Count + 1);
            for (int i = 0; i < major.Length; i += 3)
            {
                indexSetCoords.Add(indexSetCoords.Count + 1, i + 1); //from point 1 
                indexSetCoords.Add(indexSetCoords.Count + 1, i + 2); //connect to point 2 
                indexSetCoords.Add(indexSetCoords.Count + 1, i + 2); //from point 2 
                indexSetCoords.Add(indexSetCoords.Count + 1, i + 3); //connect to point 3 
                indexSetCoords.Add(indexSetCoords.Count + 1, i + 3); //from point 3 
                indexSetCoords.Add(indexSetCoords.Count + 1, i + 1); //connect to point 1
            }
            primitive.CoordinateSet = majorCoordSet;
            primitive.CoordinateIndexSet = indexSetCoords;
        }
        GraphicsCoordinateSet minorCoordSet;
        {
            GraphicsNode node = graphics.AddNode(graphics.Count + 1);
            minorCoordSet =
            dataSets.CreateCoordinateSet(dataSets.Count + 1);
            minorCoordSet.PutCoordinates(minor);
            TriangleGraphics primitive = node.AddTriangleGraphics();
            primitive.DepthPriority = 19999;
            //Create Coordinate Index Set 
            GraphicsColorSet colorSet = dataSets.CreateColorSet(dataSets.Count + 1);
            for (int i = 0; i < minor.Length; i += 3)
            {
                colorSet.Add(colorSet.Count + 1, 255, 0, 0);
            }
            primitive.CoordinateSet = minorCoordSet;
            primitive.ColorSet = colorSet;
            primitive.ColorBinding = ColorBindingEnum.kPerItemColors;
            //primitive.CoordinateIndexSet = indexSetCoords;
        }
        Program.INVENTOR_APPLICATION.ActiveView.Update();

        for (int i = 0; i < 2000 && faces.Count > faceLimit; i++)
        {
            Console.WriteLine("Removing another face; current at: " + faces.Count);
            DefaultConvexFace<DefaultVertex> remove = faces[0];
            // Highlight
            Array.Copy(remove.Vertices[0].Position, 0, minor, 0, 3);
            Array.Copy(remove.Vertices[1].Position, 0, minor, 3, 3);
            Array.Copy(remove.Vertices[2].Position, 0, minor, 6, 3);
            minorCoordSet.PutCoordinates(minor);
            Program.INVENTOR_APPLICATION.ActiveView.Update();

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
                        Array.Copy(face.Vertices[j].Position, 0, major, face.tmpHead + (3 * j), 3);
                        int index = (face.tmpHead / 3) + j + 1;
                        majorCoordSet.Remove(index);
                        majorCoordSet.Add(index, Program.INVENTOR_APPLICATION.TransientGeometry.CreatePoint(face.Vertices[j].Position[0], face.Vertices[j].Position[1], face.Vertices[j].Position[2]));
                        matched++;
                    }
                }
                if (matched >= 2)
                {
                    // Degenerate
                    faces.RemoveAt(k);
                }
            }
            // Rebuild pairing
            foreach (DefaultConvexFace<DefaultVertex> face in faces)
            {
                int myMatches = 0;
                face.Adjacency = new DefaultConvexFace<DefaultVertex>[3];
                foreach (DefaultConvexFace<DefaultVertex> face2 in faces)
                {
                    int matched = 0;
                    for (int j = 0; j < face.Vertices.Length; j++)
                    {
                        if (face.Vertices[j].Position == face2.Vertices[0].Position ||
                            face.Vertices[j].Position == face2.Vertices[1].Position ||
                            face.Vertices[j].Position == face2.Vertices[2].Position)
                        {
                            matched++;
                        }
                    }
                    if (matched >= 2)
                    {
                        face.Adjacency[myMatches++] = face2;
                    }
                    if (myMatches >= 3)
                    {
                        break;
                    }
                }
                face.getAreaSQ(true);
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