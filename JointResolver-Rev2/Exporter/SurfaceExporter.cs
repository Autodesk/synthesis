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
    private int tmpVertCount = 0;
    private int tmpFacetCount = 0;

    public double[] verts = new double[MAX_VERTICIES * 3];
    public double[] norms = new double[MAX_VERTICIES * 3];
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
        surf.GetExistingFacets(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out tmpVerts, out  tmpNorms, out  tmpIndicies);
        if (tmpVertCount == 0)
        {
            Console.WriteLine("Calculate values");
            surf.CalculateFacets(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out  tmpVerts, out tmpNorms, out  tmpIndicies);
        }
        Array.Copy(tmpVerts, 0, verts, vertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpNorms, 0, norms, facetCount * 3, tmpVertCount * 3);

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
}
