using Inventor;
using System.IO;
using System;
using System.Collections.Generic;

// Not thread safe.
public class SurfaceExporter
{
    private const int TMP_VERTICIES = ushort.MaxValue;

    private bool adaptiveIgnoring = true;
    private double adaptiveDegredation = 5; // Higher = less dropping

    // Temporary output
    private double[] tmpVerts = new double[TMP_VERTICIES * 3];
    private double[] tmpNorms = new double[TMP_VERTICIES * 3];
    private int[] tmpIndicies = new int[TMP_VERTICIES * 3];
    private double[] tmpTextureCoords = new double[TMP_VERTICIES * 2];
    private int tmpVertCount = 0;
    private int tmpFacetCount = 0;


    // Pre-submesh output
    private double[] postVerts = new double[TMP_VERTICIES * 3];
    private double[] postNorms = new double[TMP_VERTICIES * 3];
    private int[] postIndicies = new int[TMP_VERTICIES * 3];
    private uint[] postColors = new uint[TMP_VERTICIES];
    private double[] postTextureCoords = new double[TMP_VERTICIES * 2];
    private int postVertCount = 0;
    private int postFacetCount = 0;


    private Box totalSize;
    private BXDAMesh outputMesh = new BXDAMesh();

    // Tolerances
    private double[] tolerances = new double[10];
    private int tmpToleranceCount = 0;

    private List<BXDAMesh.BXDASurface> tmpSurfaces = new List<BXDAMesh.BXDASurface>();
    private BXDAMesh.BXDASurface nextSurface;

    private static int compareColors(Color a, Color b)
    {
        return Math.Abs(a.Red - b.Red) + Math.Abs(a.Blue - b.Blue) + Math.Abs(a.Green - b.Green) + (int) Math.Abs(a.Opacity - b.Opacity);
    }

    /// <summary>
    /// Copies mesh information for the given surface body into the mesh storage structure.
    /// </summary>
    /// <param name="surf">The surface body to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Separate the surface body into one mesh per face</param>
    /// <param name="onlyExtents">Only export parts that contribute to overall shape, not appearance</param>
    public void AddFacets(SurfaceBody surf, bool bestResolution = false, bool separateFaces = false, bool onlyExtents = false)
    {
        if (onlyExtents)
        {
            Box me = surf.RangeBox;
            if (totalSize != null)
            {
                if (totalSize.Contains(me.MinPoint) && totalSize.Contains(me.MaxPoint))
                {
                    // Skip
                    return;
                }
                totalSize.Extend(me.MinPoint);
                totalSize.Extend(me.MaxPoint);
            }
        }
        BXDAMesh.BXDASurface nextSurface = new BXDAMesh.BXDASurface();

        surf.GetExistingFacetTolerances(out tmpToleranceCount, out tolerances);
        int bestIndex = -1;
        for (int i = 0; i < tmpToleranceCount; i++)
        {
            if (bestIndex < 0 || ((tolerances[i] < tolerances[bestIndex]) == bestResolution))
            {
                bestIndex = i;
            }
        }
        //  Console.WriteLine("(" + postVertCount + "v/" + postFacetCount + "f)\tExporting " + surf.Parent.Name + "." + surf.Name + " with tolerance " + tolerances[bestIndex]);

        AssetProperties sharedValue = null;
        string sharedDisp = null;
        if (separateFaces)  // Only separate if they are actually different colors
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            separateFaces = false;
            foreach (Face f in surf.Faces)
            {
                try
                {
                    Asset ast = f.Appearance;
                    if (sharedValue == null)
                    {
                        sharedValue = new AssetProperties(ast);
                        sharedDisp = ast.DisplayName;
                    }
                    else
                    {
                        // HACKKKKKK
                        if (!ast.DisplayName.Equals(sharedDisp))
                        {
                            separateFaces = true;
                            break;
                            /*AssetProperties props = new AssetProperties(ast);
                            if (compareColors(sharedValue.color, props.color) > 10 || sharedValue.translucency != props.translucency || sharedValue.transparency != props.transparency)
                            {
                                separateFaces = true;
                                break;
                            }*/
                        }
                    }
                }
                catch
                {
                }
            }
        }

        if (separateFaces)
        {
            Console.WriteLine("Exporting " + surf.Faces.Count + " faces for " + surf.Parent.Name + "\t(" + surf.Name + ")");
            int i = 0;
            foreach (Face f in surf.Faces)
            {
                Console.Write(i + "/" + surf.Faces.Count);
                Console.CursorLeft = 0;
                i++;
                AddFacets(f, tolerances[bestIndex]);
            }
            Console.WriteLine(i + "/" + surf.Faces.Count);
        }
        else
        {
            Console.WriteLine("Exporting single block for " + surf.Parent.Name + "\t(" + surf.Name + ")");
            tmpVertCount = 0;
            surf.GetExistingFacetsAndTextureMap(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out tmpVerts, out  tmpNorms, out  tmpIndicies, out tmpTextureCoords);
            if (tmpVertCount == 0)
            {
                Console.WriteLine("Calculate values");
                surf.CalculateFacetsAndTextureMap(tolerances[bestIndex], out tmpVertCount, out tmpFacetCount, out  tmpVerts, out tmpNorms, out  tmpIndicies, out tmpTextureCoords);
            }
            AssetProperties assetProps = sharedValue;
            if (sharedValue == null)
            {
                try
                {
                    assetProps = new AssetProperties(surf.Appearance);
                }
                catch
                {
                    assetProps = new AssetProperties(surf.Parent.Appearance);
                }
            }
            AddFacetsInternal(assetProps);
        }
    }

    private void DumpMeshBuffer()
    {
        if (postVertCount == 0 || postFacetCount == 0)
            return;
        BXDAMesh.BXDASubMesh subObject = new BXDAMesh.BXDASubMesh();
        subObject.verts = new double[postVertCount * 3];
        subObject.norms = new double[postVertCount * 3];
        Array.Copy(postVerts, 0, subObject.verts, 0, postVertCount * 3);
        Array.Copy(postNorms, 0, subObject.norms, 0, postVertCount * 3);
        Console.WriteLine("Mesh segment " + outputMesh.meshes.Count + " has " + postVertCount + " verts and " + postFacetCount + " facets");
        subObject.surfaces = new List<BXDAMesh.BXDASurface>(tmpSurfaces);
        outputMesh.meshes.Add(subObject);

        postVertCount = 0;
        postFacetCount = 0;
        tmpSurfaces = new List<BXDAMesh.BXDASurface>();

        // Wait for shenanigans
        if (waitingThreads.Count > 0)
        {
            Console.WriteLine("Got ahead of ourselves....");
            System.Threading.WaitHandle.WaitAll(waitingThreads.ToArray());
            waitingThreads.Clear();
        }

        /*foreach (BXDAMesh.BXDASurface surface in subObject.surfaces)
        {
            int facetCount = surface.indicies.Length / 3;

            for (int i = 0; i < facetCount; i++)
            {
                int fI = i * 3;
                // Integrity check
                for (int j = 0; j < 3; j++)
                {
                    if (surface.indicies[fI + j] < 0 || surface.indicies[fI + j] >= subObject.verts.Length)
                    {
                        Console.WriteLine("Tris #" + i + " failed.  Index is " + surface.indicies[fI + j]);
                        Console.ReadLine();
                    }
                }
            }
        } Integrity mainly for debug */
    }

    List<System.Threading.ManualResetEvent> waitingThreads = new List<System.Threading.ManualResetEvent>();

    /// <summary>
    /// Moves the mesh currently in the temporary mesh buffer into the mesh structure itself, 
    /// with material information from the asset properties.
    /// </summary>
    /// <param name="assetProps">Material information to use</param>
    private void AddFacetsInternal(AssetProperties assetProps)
    {
        if (tmpVertCount > TMP_VERTICIES)
        {
            // This won't actually happen since TMP_VERTEX_COUNT is max short.
            System.Windows.Forms.MessageBox.Show("Warning: Mesh segment exceededed " + TMP_VERTICIES + " verticies.  Strange things may begin to happen.");
        }
        if (tmpVertCount + postVertCount >= TMP_VERTICIES)
        {
            DumpMeshBuffer();
        }

        Array.Copy(tmpVerts, 0, postVerts, postVertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpNorms, 0, postNorms, postVertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpTextureCoords, 0, postTextureCoords, postVertCount * 2, tmpVertCount * 2);

        nextSurface = new BXDAMesh.BXDASurface();

        nextSurface.color = 0xFFFFFFFF;

        if (assetProps.color != null)
        {
            nextSurface.hasColor = true;
            nextSurface.color = ((uint) assetProps.color.Red << 0) | ((uint) assetProps.color.Green << 8) | ((uint) assetProps.color.Blue << 16) | ((((uint) (assetProps.color.Opacity * 255)) & 0xFF) << 24);
        }
        nextSurface.transparency = (float) assetProps.transparency;
        nextSurface.translucency = (float) assetProps.translucency;

        nextSurface.indicies = new int[tmpFacetCount * 3];

        // Now we must manually copy the indicies
        Array.Copy(tmpIndicies, nextSurface.indicies, nextSurface.indicies.Length);
        System.Threading.ManualResetEvent lockThing = new System.Threading.ManualResetEvent(false);
        if (waitingThreads.Count > 32)
        {
            Console.WriteLine("Got ahead of ourselves....");
            System.Threading.WaitHandle.WaitAll(waitingThreads.ToArray());
            waitingThreads.Clear();
        }
        waitingThreads.Add(lockThing);
        BXDAMesh.BXDASurface surfThing = nextSurface;
        int offset = postVertCount;

        System.Threading.ThreadPool.QueueUserWorkItem(delegate(object obj)
        {
            for (int i = 0; i < tmpFacetCount * 3; i++)
            {
                surfThing.indicies[i] = surfThing.indicies[i] + offset - 1;
                // Inventor has one-based indicies.  Zero-based is the way to go for everything except Inventor.
            }
            lockThing.Set();
        }, waitingThreads.Count);

        tmpSurfaces.Add(nextSurface);

        postFacetCount += tmpFacetCount;
        postVertCount += tmpVertCount;
    }

    /// <summary>
    /// Copies mesh information from the Inventor API to the temporary mesh buffer, then into the mesh structure.
    /// </summary>
    /// <param name="surf">The source mesh</param>
    /// <param name="tolerance">The chord tolerance for the mesh</param>
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

    /// <summary>
    /// Clears the mesh structure and physical properties, 
    /// preparing this exporter for another set of objects.
    /// </summary>
    public void Reset()
    {
        outputMesh = new BXDAMesh();
    }

    /// <summary>
    /// Adds the mesh for the given component, and all its subcomponents to the mesh storage structure.
    /// </summary>
    /// <param name="occ">The component to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Export each face as a separate mesh</param>
    /// <param name="ignorePhysics">Don't add the physical properties of this component to the exporter</param>
    /// <param name="onlyExtents">Only export parts that contribute to overall shape, not appearance</param>
    public void ExportAll(ComponentOccurrence occ, bool bestResolution = false, bool separateFaces = false, bool ignorePhysics = false, bool onlyExtents = false)
    {
        if (!ignorePhysics)
        {
            // Compute physics
            try
            {
                outputMesh.physics.centerOfMass.Multiply(outputMesh.physics.mass);
                float myMass = (float) occ.MassProperties.Mass;
                outputMesh.physics.centerOfMass.Add(Utilities.ToBXDVector(occ.MassProperties.CenterOfMass).Multiply(myMass));
                outputMesh.physics.mass += myMass;
                outputMesh.physics.centerOfMass.Multiply(1.0f / outputMesh.physics.mass);
            }
            catch
            {
            }
        }

        if (!occ.Visible)
            return;

        foreach (SurfaceBody surf in occ.SurfaceBodies)
        {
            AddFacets(surf, bestResolution, separateFaces, onlyExtents);
        }

        double totalVolume = 0;
        foreach (ComponentOccurrence occ2 in occ.SubOccurrences)
        {
            totalVolume += occ2.MassProperties.Volume;
        }
        totalVolume /= occ.SubOccurrences.Count * adaptiveDegredation;

        foreach (ComponentOccurrence item in occ.SubOccurrences)
        {
            if (adaptiveIgnoring && item.MassProperties.Volume < totalVolume)
            {
                Console.WriteLine("Drop: " + item.Name);
            }
            else
            {
                ExportAll(item, bestResolution, separateFaces, true, onlyExtents);
            }
        }
    }

    /// <summary>
    /// Adds the mesh for the given components, and all their subcomponents to the mesh storage structure.
    /// </summary>
    /// <param name="occs">The components to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Export each face as a separate mesh</param>
    /// <param name="onlyExtents">Only export parts that contribute to overall shape, not appearance</param>
    public void ExportAll(ComponentOccurrences occs, bool bestResolution = false, bool separateFaces = false, bool onlyExtents = false)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ, bestResolution, separateFaces, onlyExtents);
        }
    }

    /// <summary>
    /// Adds the mesh for the given components, and all their subcomponents to the mesh storage structure.
    /// </summary>
    /// <param name="occs">The components to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Export each face as a separate mesh</param>
    /// <param name="onlyExtents">Only export parts that contribute to overall shape, not appearance</param>
    public void ExportAll(List<ComponentOccurrence> occs, bool bestResolution = false, bool separateFaces = false, bool onlyExtents = false)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ, bestResolution, separateFaces, onlyExtents);
        }
    }

    /// <summary>
    /// Adds the mesh for all the components and their subcomponenets in the custom rigid group.  <see cref="ExportAll(ComponentOccurrence,bool,bool,bool)"/>
    /// </summary>
    /// <remarks>
    /// This uses the best resolution and separate faces options stored inside the provided custom rigid group.
    /// </remarks>
    /// <param name="group">The group to export from</param>
    /// <param name="onlyExtents">Only export parts that contribute to overall shape, not appearance</param>
    public void ExportAll(CustomRigidGroup group, bool onlyExtents = false)
    {
        double totalVolume = 0;
        foreach (ComponentOccurrence occ in group.occurrences)
        {
            totalVolume += occ.MassProperties.Volume;
        }
        totalVolume /= group.occurrences.Count * adaptiveDegredation;

        foreach (ComponentOccurrence occ in group.occurrences)
        {
            if (adaptiveIgnoring && occ.MassProperties.Volume < totalVolume)
            {
                Console.WriteLine("Drop: " + occ.Name); // TODO Ignores physics
            }
            else
            {
                ExportAll(occ, group.highRes && !onlyExtents, group.colorFaces && !onlyExtents, onlyExtents);
            }
        }
    }

    /// <summary>
    /// Gets the currently generated mesh object.
    /// </summary>
    /// <returns>a BXDA Mesh</returns>
    public BXDAMesh GetOutput()
    {
        DumpMeshBuffer();
        return outputMesh;
    }
}
