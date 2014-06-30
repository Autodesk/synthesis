using Inventor;
using System.IO;
using System;
using System.Collections.Generic;

// Not thread safe.
class SurfaceExporter
{
    private const int MAX_VERTICIES = 8192 * 128;

    private double[] tmpVerts = new double[MAX_VERTICIES * 3];
    private double[] tmpNorms = new double[MAX_VERTICIES * 3];
    private int[] tmpIndicies = new int[MAX_VERTICIES * 3];
    public double[] tmpTextureCoords = new double[MAX_VERTICIES * 2];
    private int tmpVertCount = 0;
    private int tmpFacetCount = 0;

    public double[] verts = new double[MAX_VERTICIES * 3];
    public double[] norms = new double[MAX_VERTICIES * 3];
    public double[] textureCoords = new double[MAX_VERTICIES * 2];
    public byte[] colors = new byte[MAX_VERTICIES * 4];
    public int[] indicies = new int[MAX_VERTICIES * 3];
    public int vertCount = 0;
    public int facetCount = 0;

    // Tolerances
    private double[] tolerances = new double[10];
    private int tmpToleranceCount = 0;

    // Facets are relative to world space!
    public void AddFacets(SurfaceBody surf, bool bestResolution = false)
    {
        surf.GetExistingFacetTolerances(out tmpToleranceCount, out tolerances);
        int bestIndex = -1;
        for (int i = 0; i < tmpToleranceCount; i++)
        {
            if (bestIndex < 0 || ((tolerances[i] < tolerances[bestIndex]) == bestResolution)) { bestIndex = i; }
        }
        Console.WriteLine("Exporting " + surf.Parent.Name + "." + surf.Name + " with tolerance " + tolerances[bestIndex]);
        surf.GetExistingFacetsAndTextureMap(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out tmpVerts, out  tmpNorms, out  tmpIndicies, out textureCoords);
        if (tmpVertCount == 0)
        {
            Console.WriteLine("Calculate values");
            surf.CalculateFacetsAndTextureMap(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out  tmpVerts, out tmpNorms, out  tmpIndicies, out textureCoords);
        }
        Array.Copy(tmpVerts, 0, verts, vertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpNorms, 0, norms, vertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpTextureCoords, 0, tmpTextureCoords, vertCount * 2, tmpVertCount * 2);

        AssetProperties assetProps;
        try
        {
            assetProps = new AssetProperties(surf.Appearance);
        }
        catch
        {
            assetProps = new AssetProperties(surf.Parent.Appearance);
        }
        for (int i = 0; i < tmpVertCount; i++)
        {
            int bH = (vertCount + i) * 4;
            if (assetProps.color != null)
            {
                colors[bH] = assetProps.color.Red;
                colors[bH+1] = assetProps.color.Green;
                colors[bH+2] = assetProps.color.Blue;
                colors[bH+3] = (byte)(assetProps.color.Opacity*255.0);
            }
            else
            {
                colors[bH] = colors[bH + 1] = colors[bH + 2] = colors[bH + 3] = 255;
            }
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

    public void WriteSTL(String path) {
        StreamWriter writer = new StreamWriter(path);
        writer.WriteLine("solid " + "bleh");
        for (int i = 0; i<facetCount;i++){
            int offset = i * 3;
            for (int j = 0; j<3; j++){
                int oj = indicies[offset+j] * 3 - 3;
                if (j==0) {
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
        BigEndianBinaryWriter writer = new BigEndianBinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
        writer.Write(vertCount);
        for (int i = 0; i < vertCount; i++)
        {
            int vecI = i * 3;
            int texI = i * 2;
            int colI = i *4;
            writer.Write(verts[vecI]);
            writer.Write(verts[vecI+1]);
            writer.Write(verts[vecI+2]);
            writer.Write(norms[vecI]);
            writer.Write(norms[vecI + 1]);
            writer.Write(norms[vecI + 2]);
            writer.Write(colors, colI, 4);
            writer.Write(textureCoords[texI]);
            writer.Write(textureCoords[texI+1]);
        }
        writer.Write(facetCount);
        for (int i = 0; i < facetCount; i++)
        {
            int fI = i * 3;
            writer.Write(indicies[fI]-1);
            writer.Write(indicies[fI+1]-1);
            writer.Write(indicies[fI+2]-1);
        }
        writer.Close();
    }
}
