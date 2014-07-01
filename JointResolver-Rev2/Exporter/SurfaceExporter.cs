using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// Not thread safe.
class SurfaceExporter
{
    private const int MAX_VERTICIES = 8192 * 256;
    private const int TMP_VERTICIES = 8192;

    private double[] tmpVerts = new double[TMP_VERTICIES * 3];
    private double[] tmpNorms = new double[TMP_VERTICIES * 3];
    private int[] tmpIndicies = new int[TMP_VERTICIES * 3];
    public double[] tmpTextureCoords = new double[TMP_VERTICIES * 2];
    private int tmpVertCount = 0;
    private int tmpFacetCount = 0;

    public double[] verts = new double[MAX_VERTICIES * 3];
    public double[] norms = new double[MAX_VERTICIES * 3];
    public double[] textureCoords = new double[MAX_VERTICIES * 2];
    public uint[] colors = new uint[MAX_VERTICIES];
    public int[] indicies = new int[MAX_VERTICIES * 3];
    public int vertCount = 0;
    public int facetCount = 0;

    // Tolerances
    private double[] tolerances = new double[10];
    private int tmpToleranceCount = 0;

    public void AddFacets(SurfaceBody surf, bool bestResolution = false)
    {
        surf.GetExistingFacetTolerances(out tmpToleranceCount, out tolerances);
        int bestIndex = -1;
        for (int i = 0; i < tmpToleranceCount; i++)
        {
            if (bestIndex < 0 || ((tolerances[i] < tolerances[bestIndex]) == bestResolution)) { bestIndex = i; }
        }
        Console.WriteLine("Exporting " + surf.Parent.Name + "." + surf.Name + " with tolerance " + tolerances[bestIndex]);

        int ia = 0;
        foreach (Face f in surf.Faces)
        {
            AddFacets(f, tolerances[bestIndex]);
            Console.WriteLine("\t" + (++ia) + "/" + surf.Faces.Count + "\t\tVerts: " + vertCount);
        }
    }

    private static void Memset<T>(T[] array, T elem)
    {
        int length = array.Length;
        if (length == 0) return;
        array[0] = elem;
        int count;
        for (count = 1; count <= length / 2; count *= 2)
            Array.Copy(array, 0, array, count, count);
        Array.Copy(array, 0, array, count, length - count);
    }

    private void AddFacets(Face surf, double tolerance)
    {
        tmpVertCount = 0;
        surf.GetExistingFacetsAndTextureMap(tolerance, out tmpVertCount, out tmpFacetCount, out tmpVerts, out  tmpNorms, out  tmpIndicies, out tmpTextureCoords);
        if (tmpVertCount == 0)
        {
            Console.WriteLine("Calculate values");
            surf.CalculateFacetsAndTextureMap(tolerance, out tmpVertCount, out tmpFacetCount, out  tmpVerts, out tmpNorms, out  tmpIndicies, out tmpTextureCoords);
        }
        Array.Copy(tmpVerts, 0, verts, vertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpNorms, 0, norms, vertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpTextureCoords, 0, textureCoords, vertCount * 2, tmpVertCount * 2);

        AssetProperties assetProps;
        try
        {
            assetProps = new AssetProperties(surf.Appearance);
        }
        catch
        {
            assetProps = new AssetProperties(surf.Parent.Appearance);
        }
        uint colorVal = 0xFFFFFFFF;
        if (assetProps.color != null)
        {
            colorVal = ((uint)assetProps.color.Red << 24) | ((uint)assetProps.color.Green << 16) | ((uint)assetProps.color.Blue << 8) | (((uint)(assetProps.color.Opacity * 255)) & 0xFF);
        }
        for (int i = vertCount; i < vertCount + tmpVertCount; i++)
        {
            colors[i] = colorVal;
        }

        // Now we must manually copy the indicies
        int indxOffset = facetCount * 3;
        for (int i = 0; i < tmpFacetCount * 3; i++)
        {
            indicies[i + indxOffset] = tmpIndicies[i] + vertCount;
        }
        facetCount += tmpFacetCount;
        vertCount += tmpVertCount;
    }

    public void Reset()
    {
        vertCount = 0;
        facetCount = 0;
    }

    public void ExportAll(ComponentOccurrence occ)
    {
        if (!occ.Visible) return;
        foreach (SurfaceBody surf in occ.SurfaceBodies)
        {
            AddFacets(surf, false);
        }
        foreach (ComponentOccurrence item in occ.SubOccurrences)
        {
            ExportAll(item);
        }
    }

    public void ExportAll(ComponentOccurrences occs)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ);
        }
    }

    public void ExportAll(List<ComponentOccurrence> occs)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ);
        }
    }

    public void WriteSTL(String path)
    {
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("solid " + "bleh");
        for (int i = 0; i < facetCount; i++)
        {
            int offset = i * 3;
            for (int j = 0; j < 3; j++)
            {
                int oj = indicies[offset + j] * 3 - 3;
                if (j == 0)
                {
                    writer.WriteLine("facet normal " + norms[oj] + " " + norms[oj + 1] + " " + norms[oj + 2]);
                    writer.WriteLine("outer loop");
                }
                writer.WriteLine("vertex " + verts[oj] + " " + verts[oj + 1] + " " + verts[oj + 2]);
            }
            writer.WriteLine("endloop");
            writer.WriteLine("endfacet");
        }
        writer.WriteLine("endsolid");
        writer.Close();
    }

    //[4 byte integer]	Vertex Count
    //For each vertex…
    //[8 byte double]	Vertex position x
    //[8 byte double]	Vertex position y
    //[8 byte double]	Vertex position z
    //[8 byte double]	Vertex normal x
    //[8 byte double]	Vertex normal y
    //[8 byte double]	Vertex normal z
    //[1 byte]		Vertex color red
    //[1 byte]		Vertex color green
    //[1 byte]		Vertex color blue
    //[1 byte]		Vertex color alpha
    //[8 byte double]	Vertex texture u
    //[8 byte double]	Vertex texture v
    //
    //[4 byte integer]	Facet count
    //For each facet…
    //[4 byte integer]	Vertex 1
    //[4 byte integer]	Vertex 2
    //[4 byte integer]	Vertex 3


    public void WriteBXDA(String path)
    {
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
        writer.Write(new char[] { 'Y', 'U', 'N', 'O', '1', '8' });
        writer.Write(vertCount);
        for (int i = 0; i < vertCount; i++)
        {
            int vecI = i * 3;
            int texI = i * 2;
            int colI = i;
            writer.Write(verts[vecI]);
            writer.Write(verts[vecI + 1]);
            writer.Write(verts[vecI + 2]);
            writer.Write(norms[vecI]);
            writer.Write(norms[vecI + 1]);
            writer.Write(norms[vecI + 2]);
            writer.Write(colors[colI]);
            writer.Write(textureCoords[texI]);
            writer.Write(textureCoords[texI + 1]);
        }
        writer.Write(facetCount);
        for (int i = 0; i < facetCount; i++)
        {
            int fI = i * 3;
            writer.Write(indicies[fI] - 1);
            writer.Write(indicies[fI + 1] - 1);
            writer.Write(indicies[fI + 2] - 1);
        }
        writer.Close();
    }
}
