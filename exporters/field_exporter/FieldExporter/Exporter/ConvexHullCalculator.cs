// Yay or nay to fancy things.
// If this is defined then the program will show the simplification progress on each hull.
// #define FANCY_SIMPLIFY_GRAPHICS

using System;
using System.Collections.Generic;
using ConvexLibraryWrapper;

/// <summary>
/// Computes and simplifies convex hulls for BXDA meshes.
/// </summary>
public class ConvexHullCalculator
{
    private const float EPSILON = 0.5f; // Magic unicorns here.  < 0.5cm = who cares

    /// <summary>
    /// Maximum number of triangles a convex hull is allowed to have.
    /// </summary>
    private const int CONVEX_HULL_FACET_LIMIT = 64;

    /// <summary>
    /// Represents a vertex with a list of attached faces and an index buffer item.
    /// </summary>
    private class SimplificationVertex
    {
        public int finalIndex = -99;
        public float[] pos = new float[3];
        public List<SimplificationFace> faces = new List<SimplificationFace>();

        public SimplificationVertex(float x, float y, float z)
        {
            pos[0] = x;
            pos[1] = y;
            pos[2] = z;
        }
    }

    /// <summary>
    /// Represents a face to be simplified, containing edge lengths.
    /// </summary>
    private class SimplificationFace : IComparable<SimplificationFace>
    {
        public SimplificationVertex[] verts = new SimplificationVertex[3];
        public float[] edgeLengths = new float[3];
        public int minEdge = 0;

        /// <summary>
        /// Updates the cached edge lengths
        /// </summary>
        public void updateInfo()
        {
            float best = 999999999;
            for (int i = 0; i < 3; i++)
            {
                SimplificationVertex a = verts[i];
                SimplificationVertex b = verts[(i + 1) % verts.Length];
                float dx = a.pos[0] - b.pos[0];
                float dy = a.pos[1] - b.pos[1];
                float dz = a.pos[2] - b.pos[2];
                edgeLengths[i] = (dx * dx) + (dy * dy) + (dz * dz);
                if (edgeLengths[i] < best)
                {
                    best = edgeLengths[i];
                    minEdge = i;
                }
            }
        }

        /// <summary>
        /// Compare two simplification faces for the one with the smallest edge.
        /// </summary>
        /// <param name="other">The other face</param>
        int IComparable<SimplificationFace>.CompareTo(SimplificationFace other)
        {
            return edgeLengths[minEdge].CompareTo(other.edgeLengths[other.minEdge]);
        }
    }

    /// <summary>
    /// Simplifies the mesh provided, then places it back into the parameters.
    /// </summary>
    /// <remarks>
    /// Repeatedly remove the shortest edge in the mesh, reducing the triangle count by two each step.  This is repeated
    /// until the triangle count is low enough or more than 2000 tries have been made.
    /// </remarks>
    /// <param name="verts">The original and final verticies. (3 elements per vertex)</param>
    /// <param name="vertCount">The original and final vertex counts.</param>
    /// <param name="inds">The original and final index buffer.  (3 element per triangle, zero based)</param>
    /// <param name="trisCount">The original and final triangle counts.</param>
    private static void Simplify(ref float[] verts, ref uint vertCount, ref uint[] inds, ref uint trisCount)
    {
        // Setup edge relations, compute min edge lengths...
        List<SimplificationVertex> simplVerts = new List<SimplificationVertex>();
        for (int i = 0; i < vertCount; i++)
        {
            simplVerts.Add(new SimplificationVertex(verts[i * 3], verts[i * 3 + 1], verts[i * 3 + 2]));
        }
        List<SimplificationFace> simplFace = new List<SimplificationFace>();
        for (int i = 0; i < trisCount * 3; i += 3)
        {
            SimplificationFace face = new SimplificationFace();
            face.verts[0] = simplVerts[(int)inds[i]];
            face.verts[1] = simplVerts[(int)inds[i + 1]];
            face.verts[2] = simplVerts[(int)inds[i + 2]];
            foreach (SimplificationVertex v in face.verts)
            {
                v.faces.Add(face);  // Make sure all verticies know their neighbors
            }
            simplFace.Add(face);
        }
        foreach (SimplificationFace face in simplFace)
        {
            face.updateInfo();
        }

        simplFace.Sort();

        #region FANCY_SIMPLIFY_GRAPHICS
#if FANCY_SIMPLIFY_GRAPHICS
        double[] major, minor;
        GraphicsCoordinateSet majorCoordSet, minorCoordSet;

            major = new double[9 * simplFace.Count];
            for (int a = 0; a < simplFace.Count; a++)
            {
                int off = a * 9;
                simplFace[a].tmpHead = off;
                Array.Copy(simplFace[a].verts[0].pos, 0, major, off, 3);
                Array.Copy(simplFace[a].verts[1].pos, 0, major, off + 3, 3);
                Array.Copy(simplFace[a].verts[2].pos, 0, major, off + 6, 3);
            }
            minor = new double[9];
            AssemblyDocument part = ((AssemblyDocument) Exporter.INVENTOR_APPLICATION.ActiveDocument);
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
            Exporter.INVENTOR_APPLICATION.ActiveView.Update();
#endif
        #endregion

        // Time for shenanigans!  We are going to naively pick the shortest edge and then remove it.  Destroys two faces per loop typically.
        // Give it 2000 tries at most.
        for (int i = 0; i < 2000 && simplFace.Count > CONVEX_HULL_FACET_LIMIT; i++)
        {
            SimplificationFace bFace = simplFace[0];
            // This is the edge to remove
            SimplificationVertex[] remove = new SimplificationVertex[] {
                bFace.verts[bFace.minEdge], bFace.verts[(bFace.minEdge + 1) % bFace.verts.Length] };


            #region FANCY_SIMPLIFY_GRAPHICS
#if FANCY_SIMPLIFY_GRAPHICS
                // Highlight
                Array.Copy(bFace.verts[0].pos, 0, minor, 0, 3);
                Array.Copy(bFace.verts[1].pos, 0, minor, 3, 3);
                Array.Copy(bFace.verts[2].pos, 0, minor, 6, 3);
                minorCoordSet.PutCoordinates(minor);
                Exporter.INVENTOR_APPLICATION.ActiveView.Update();
#endif
            #endregion

            // Find the center point of the edge.  One edge -> one vertex
            float[] center = new float[3];
            foreach (SimplificationVertex vert in remove)
            {
                center[0] += vert.pos[0] / 2.0f;
                center[1] += vert.pos[1] / 2.0f;
                center[2] += vert.pos[2] / 2.0f;
                vert.faces.Clear(); // Really, never use vertex again.
            }
            SimplificationVertex newVertex = new SimplificationVertex(center[0], center[1], center[2]);

            // Ineffeciently check each face to see if it shares a vertex with the edge
            for (int k = simplFace.Count - 1; k >= 0; k--)
            {
                int matched = 0;
                SimplificationFace face = simplFace[k];
                for (int j = 0; j < face.verts.Length; j++)
                {
                    if (face.verts[j] == remove[0] ||
                        face.verts[j] == remove[1])
                    {
                        face.verts[j] = newVertex;

                        #region FANCY_SIMPLIFY_GRAPHICS
#if FANCY_SIMPLIFY_GRAPHICS
                            Array.Copy(face.verts[j].pos, 0, major, face.tmpHead + (3 * j), 3);
                            int index = (face.tmpHead / 3) + j + 1;
                            majorCoordSet.Remove(index);
                            majorCoordSet.Add(index, Exporter.INVENTOR_APPLICATION.TransientGeometry.CreatePoint(face.verts[j].pos[0], face.verts[j].pos[1], face.verts[j].pos[2]));
#endif
                        #endregion

                        if (matched == 0)
                        {
                            newVertex.faces.Add(face);
                        }
                        matched++;
                    }
                }
                // If we share at least two verts with the edge we are dead, since the triangle ends up with two
                // of the same vertex
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
                    // We changed, so update edge lengths
                    face.updateInfo();
                }
            }
            simplVerts.Add(newVertex);
            // Resort by edge length
            simplFace.Sort();
        }

        simplVerts.RemoveAll((vert) => vert.faces.Count <= 0);

        // Rebuild arrays
        vertCount = (uint)simplVerts.Count;
        verts = new float[vertCount * 3];
        for (int i = 0; i < simplVerts.Count; i++)
        {
            int off = i * 3;
            simplVerts[i].finalIndex = (i);  // Our indices are zero based <3
            Array.Copy(simplVerts[i].pos, 0, verts, off, 3);
        }
        trisCount = (uint)simplFace.Count;
        inds = new uint[trisCount * 3];
        for (int i = 0; i < simplFace.Count; i++)
        {
            int off = i * 3;
            inds[off] = (uint)simplFace[i].verts[0].finalIndex;
            inds[off + 1] = (uint)simplFace[i].verts[1].finalIndex;
            inds[off + 2] = (uint)simplFace[i].verts[2].finalIndex;
        }
    }

    /// <summary>
    /// Wraps the given raw mesh as a BXDA Submesh with a single surface.
    /// </summary>
    /// <param name="verts">The verticies. (3 elements per vertex)</param>
    /// <param name="vertCount">The vertex count.</param>
    /// <param name="inds">The index buffer.  (3 element per triangle, zero based)</param>
    /// <param name="trisCount">The triangle count</param>
    /// <returns>The resulting mesh</returns>
    private static BXDAMesh.BXDASubMesh ExportSubMesh(float[] verts, uint vertCount, uint[] inds, uint trisCount)
    {
        Simplify(ref verts, ref vertCount, ref inds, ref trisCount);
        BXDAMesh.BXDASubMesh sub = new BXDAMesh.BXDASubMesh();
        sub.norms = null;

        sub.verts = new double[verts.Length];

        for (uint i2 = 0; i2 < vertCount * 3; i2++)
        {
            sub.verts[i2] = verts[i2];
        }

        BXDAMesh.BXDASurface collisionSurface = new BXDAMesh.BXDASurface();

        collisionSurface.indicies = new int[inds.Length];
        for (uint i2 = 0; i2 < trisCount * 3; i2++)
        {
            collisionSurface.indicies[i2] = (int)inds[i2];
        }

        sub.surfaces = new List<BXDAMesh.BXDASurface>();
        sub.surfaces.Add(collisionSurface);

        return sub;
    }

    /// <summary>
    /// Used for creating a hull from the given BXDAMesh.BXDASubmesh.
    /// </summary>
    /// <param name="subMesh"></param>
    /// <param name="decompose"></param>
    /// <returns></returns>
    public static BXDAMesh.BXDASubMesh GetHull(BXDAMesh.BXDASubMesh subMesh)
    {
        try
        {
            List<int> indices = new List<int>();

            foreach (BXDAMesh.BXDASurface surface in subMesh.surfaces)
            {
                indices.AddRange(surface.indicies);
            }

            IVHACD decomposer = new IVHACD();

            ConvexLibraryWrapper.Parameters parameters = new ConvexLibraryWrapper.Parameters();

            parameters.m_depth = 1;
            parameters.m_concavity = 1;

            if (!decomposer.Compute(Array.ConvertAll<double, float>(subMesh.verts, (d) => (float)d),
                3, (uint)subMesh.verts.Length / 3, indices.ToArray(), 3, (uint)indices.Count / 3, parameters))
                return null;

            ConvexLibraryWrapper.ConvexHull result = decomposer.GetConvexHull(0);

            BXDAMesh.BXDASubMesh resultMesh = ExportSubMesh(Array.ConvertAll<double, float>(result.m_points, (d) => (float)d), result.m_nPoints,
                Array.ConvertAll<int, uint>(result.m_triangles, (i) => (uint)i), result.m_nTriangles);

            decomposer.Cancel();
            decomposer.Clean();
            decomposer.Release();

            return resultMesh;
        }
        catch (Exception e)
        {
            Type t = e.GetType();
            throw;
        }
    }
}