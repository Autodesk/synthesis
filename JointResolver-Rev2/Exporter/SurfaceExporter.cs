using Inventor;
using System.IO;
using System;
using System.Collections.Generic;

// Not thread safe.
class SurfaceExporter
{
    private const int MAX_VERTICIES = 8192 * 256;
    private const int TMP_VERTICIES = 8192;

    private bool adaptiveIgnoring = false;

    // Temporary output
    private double[] tmpVerts = new double[TMP_VERTICIES * 3];
    private double[] tmpNorms = new double[TMP_VERTICIES * 3];
    private int[] tmpIndicies = new int[TMP_VERTICIES * 3];
    public double[] tmpTextureCoords = new double[TMP_VERTICIES * 2];
    private int tmpVertCount = 0;
    private int tmpFacetCount = 0;

    // Final output
    private struct Mesh
    {
        public double[] verts;
        public double[] norms;
        public double[] textureCoords;
        public uint[] colors;
        public int[] indicies;
        public int vertCount;
        public int facetCount;
    }

    public List<Mesh> meshes = new List<Mesh>();
    public PhysicalProperties physics = new PhysicalProperties();

    // Tolerances
    private double[] tolerances = new double[10];
    private int tmpToleranceCount = 0;

    public void AddFacets(SurfaceBody surf, bool bestResolution = false, bool separateFaces = false)
    {
        surf.GetExistingFacetTolerances(out tmpToleranceCount, out tolerances);
        int bestIndex = -1;
        for (int i = 0; i < tmpToleranceCount; i++)
        {
            if (bestIndex < 0 || ((tolerances[i] < tolerances[bestIndex]) == bestResolution))
            {
                bestIndex = i;
            }
        }
        Console.WriteLine("Exporting " + surf.Parent.Name + "." + surf.Name + " with tolerance " + tolerances[bestIndex]);

        if (separateFaces)
        {
            int ia = 0;
            foreach (Face f in surf.Faces)
            {
                AddFacets(f, tolerances[bestIndex]);
                Console.WriteLine("\t" + (++ia) + "/" + surf.Faces.Count + "\t\tVerts: " + vertCount);
            }
        }
        else
        {
            tmpVertCount = 0;
            surf.GetExistingFacetsAndTextureMap(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out tmpVerts, out  tmpNorms, out  tmpIndicies, out tmpTextureCoords);
            if (tmpVertCount == 0)
            {
                Console.WriteLine("Calculate values");
                surf.CalculateFacetsAndTextureMap(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out  tmpVerts, out tmpNorms, out  tmpIndicies, out tmpTextureCoords);
            }
            AssetProperties assetProps;
            try
            {
                assetProps = new AssetProperties(surf.Appearance);
            }
            catch
            {
                assetProps = new AssetProperties(surf.Parent.Appearance);
            }
            Console.WriteLine("Add " + tmpVertCount + " verticies and " + tmpFacetCount + " triangles");
            AddFacetsInternal(assetProps);
        }
    }

    private void AddFacetsInternal(AssetProperties assetProps)
    {
        Mesh subObject = new Mesh();
        subObject.verts = new double[tmpVertCount * 3];
        subObject.norms = new double[tmpVertCount * 3];
        subObject.textureCoords = new double[tmpVertCount * 2];
        subObject.colors = new uint[tmpVertCount];
        subObject.indicies = new int[tmpFacetCount * 3];
        subObject.facetCount = tmpFacetCount;
        subObject.vertCount = tmpVertCount;
        Array.Copy(tmpVerts, 0, subObject.verts, 0, tmpVertCount * 3);
        Array.Copy(tmpNorms, 0, subObject.norms, 0, tmpVertCount * 3);
        Array.Copy(tmpTextureCoords, 0, subObject.textureCoords, 0, tmpVertCount * 2);
        Array.Copy(tmpIndicies, 0, subObject.indicies, 0, tmpFacetCount * 3);

        uint colorVal = 0xFFFFFFFF;
        if (assetProps.color != null)
        {
            colorVal = ((uint) assetProps.color.Red << 0) | ((uint) assetProps.color.Green << 8) | ((uint) assetProps.color.Blue << 16) | ((((uint) (assetProps.color.Opacity * 255)) & 0xFF) << 24);
        }
        for (int i = 0; i < tmpVertCount; i++)
        {
            subObject.colors[i] = colorVal;
        }
        meshes.Add(subObject);
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
        AssetProperties assetProps;
        try
        {
            assetProps = new AssetProperties(surf.Appearance);
        }
        catch
        {
            try
            {
                assetProps = new AssetProperties(surf.Parent.Appearance);
            }
            catch
            {
                assetProps = new AssetProperties(surf.Parent.Parent.Appearance);
            }
        }
        AddFacetsInternal(assetProps);
    }

    public void Reset()
    {
        vertCount = 0;
        facetCount = 0;
        physics = new PhysicalProperties();
    }

    public void ExportAll(ComponentOccurrence occ, bool bestResolution = false, bool separateFaces = false, bool ignorePhysics = false)
    {
        if (!ignorePhysics)
        {
            // Compute physics
            physics.centerOfMass.Multiply(physics.mass);
            float myMass = (float) occ.MassProperties.Mass;
            physics.mass += myMass;
            physics.centerOfMass.Add(Utilities.ToBXDVector(occ.MassProperties.CenterOfMass).Multiply(myMass));
            physics.centerOfMass.Multiply(1.0f / physics.mass);
        }

        if (!occ.Visible)
            return;

        foreach (SurfaceBody surf in occ.SurfaceBodies)
        {
            AddFacets(surf, bestResolution, separateFaces);
        }

        double totalVolume = 0;
        foreach (ComponentOccurrence occ2 in occ.SubOccurrences)
        {
            totalVolume += occ2.MassProperties.Volume;
        }
        totalVolume /= occ.SubOccurrences.Count * 5;

        foreach (ComponentOccurrence item in occ.SubOccurrences)
        {
            if (adaptiveIgnoring && item.MassProperties.Volume < totalVolume)
            {
                Console.WriteLine("Drop: " + item.Name);
            }
            else
            {
                ExportAll(item, bestResolution, separateFaces, true);
            }
        }
    }

    public void ExportAll(ComponentOccurrences occs, bool bestResolution = false, bool separateFaces = false)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ, bestResolution, separateFaces);
        }
    }

    public void ExportAll(List<ComponentOccurrence> occs, bool bestResolution = false, bool separateFaces = false)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ, bestResolution, separateFaces);
        }
    }

    public void ExportAll(CustomRigidGroup group)
    {
        double totalVolume = 0;
        foreach (ComponentOccurrence occ in group.occurrences)
        {
            totalVolume += occ.MassProperties.Volume;
        }
        totalVolume /= group.occurrences.Count * 5;

        foreach (ComponentOccurrence occ in group.occurrences)
        {
            if (adaptiveIgnoring && occ.MassProperties.Volume < totalVolume)
            {
                Console.WriteLine("Drop: " + occ.Name); // TODO Ignores physics
            }
            else
            {
                ExportAll(occ, group.highRes, group.colorFaces);
            }
        }
    }

    //[4 byte integer]  Mesh count
    //[Extensible]      Meshes

    //Each Mesh:
    //
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
        writer.Write(meshes.Count);
        foreach (Mesh mesh in meshes)
        {
            writer.Write(mesh.vertCount);
            for (int i = 0; i < mesh.vertCount; i++)
            {
                int vecI = i * 3;
                int texI = i * 2;
                int colI = i;
                writer.Write(mesh.verts[vecI]);
                writer.Write(mesh.verts[vecI + 1]);
                writer.Write(mesh.verts[vecI + 2]);
                writer.Write(mesh.norms[vecI]);
                writer.Write(mesh.norms[vecI + 1]);
                writer.Write(mesh.norms[vecI + 2]);
                writer.Write(mesh.colors[colI]);
                writer.Write(mesh.textureCoords[texI]);
                writer.Write(mesh.textureCoords[texI + 1]);
            }
            writer.Write(mesh.facetCount);
            for (int i = 0; i < mesh.facetCount; i++)
            {
                int fI = i * 3;
                writer.Write(mesh.indicies[fI] - 1);
                writer.Write(mesh.indicies[fI + 1] - 1);
                writer.Write(mesh.indicies[fI + 2] - 1);
            }
        }
        physics.WriteData(writer);
        writer.Close();
    }
}
