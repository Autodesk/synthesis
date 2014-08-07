using Inventor;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// Exports Inventor objects into the BXDA format.  One instance per thread.
/// </summary>
public class SurfaceExporter
{
    private const int TMP_VERTICIES = ushort.MaxValue;
    private const int MAX_BACKGROUND_THREADS = 32;

    private List<System.Threading.ManualResetEvent> waitingThreads = new List<System.Threading.ManualResetEvent>(MAX_BACKGROUND_THREADS);

    /// <summary>
    /// Should the exporter attempt to automatically ignore small parts.
    /// </summary>
    private bool adaptiveIgnoring = true;
    /// <summary>
    /// The minimum ratio between a sub component's bounding box volume and the average bounding box volume for an object
    /// to be considered small.  The higher the number the less that is dropped, while if the value is one about half the objects
    /// would be dropped.
    /// </summary>
    private double adaptiveDegredation = 5;

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
    private uint[] postColors = new uint[TMP_VERTICIES];
    private double[] postTextureCoords = new double[TMP_VERTICIES * 2];
    private int postVertCount = 0;
    private int postFacetCount = 0;
    private List<BXDAMesh.BXDASurface> postSurfaces = new List<BXDAMesh.BXDASurface>();

    private BXDAMesh outputMesh = new BXDAMesh();

    // Tolerances
    private double[] tolerances = new double[10];
    private int tmpToleranceCount = 0;


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
    public void AddFacets(SurfaceBody surf, bool bestResolution = false, bool separateFaces = false)
    {
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
        // Make sure all index copy threads have completed.
        if (waitingThreads.Count > 0)
        {
            Console.WriteLine("Got ahead of ourselves....");
            System.Threading.WaitHandle.WaitAll(waitingThreads.ToArray());
            waitingThreads.Clear();
        }

        if (postVertCount == 0 || postFacetCount == 0)
            return;
        BXDAMesh.BXDASubMesh subObject = new BXDAMesh.BXDASubMesh();
        subObject.verts = new double[postVertCount * 3];
        subObject.norms = new double[postVertCount * 3];
        Array.Copy(postVerts, 0, subObject.verts, 0, postVertCount * 3);
        Array.Copy(postNorms, 0, subObject.norms, 0, postVertCount * 3);
        Console.WriteLine("Mesh segment " + outputMesh.meshes.Count + " has " + postVertCount + " verts and " + postFacetCount + " facets");
        subObject.surfaces = new List<BXDAMesh.BXDASurface>(postSurfaces);
        outputMesh.meshes.Add(subObject);

        postVertCount = 0;
        postFacetCount = 0;
        postSurfaces = new List<BXDAMesh.BXDASurface>();
    }


    /// <summary>
    /// Moves the mesh currently in the temporary mesh buffer into the mesh structure itself, 
    /// with material information from the asset properties.
    /// </summary>
    /// <param name="assetProps">Material information to use</param>
    private void AddFacetsInternal(AssetProperties assetProps)
    {
        if (tmpVertCount > TMP_VERTICIES)
        {
            // This is just bad.  It could be fixed by exporting it per-face instead of with a single block.
            System.Windows.Forms.MessageBox.Show("Warning: Mesh segment exceededed " + TMP_VERTICIES + " verticies.  Strange things may begin to happen.");
        }
        // If adding this would cause the sub mesh to overflow dump what currently exists.
        if (tmpVertCount + postVertCount >= TMP_VERTICIES)
        {
            DumpMeshBuffer();
        }

        Array.Copy(tmpVerts, 0, postVerts, postVertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpNorms, 0, postNorms, postVertCount * 3, tmpVertCount * 3);
        Array.Copy(tmpTextureCoords, 0, postTextureCoords, postVertCount * 2, tmpVertCount * 2);

        BXDAMesh.BXDASurface nextSurface = new BXDAMesh.BXDASurface();

        nextSurface.color = 0xFFFFFFFF;

        if (assetProps.color != null)
        {
            nextSurface.hasColor = true;
            nextSurface.color = ((uint) assetProps.color.Red << 0) | ((uint) assetProps.color.Green << 8) | ((uint) assetProps.color.Blue << 16) | ((((uint) (assetProps.color.Opacity * 255)) & 0xFF) << 24);
        }
        nextSurface.transparency = (float) assetProps.transparency;
        nextSurface.translucency = (float) assetProps.translucency;

        nextSurface.indicies = new int[tmpFacetCount * 3];

        // Raw copy the indicies for now, then fix the offset in a background thread.
        Array.Copy(tmpIndicies, nextSurface.indicies, nextSurface.indicies.Length);

        #region Fix Index Buffer Offset
        // Make sure we haven't exceeded the maximum number of background tasks.
        if (waitingThreads.Count > MAX_BACKGROUND_THREADS)
        {
            Console.WriteLine("Got ahead of ourselves....");
            System.Threading.WaitHandle.WaitAll(waitingThreads.ToArray());
            waitingThreads.Clear();
        }
        {
            System.Threading.ManualResetEvent lockThing = new System.Threading.ManualResetEvent(false);
            waitingThreads.Add(lockThing);
            int offset = postVertCount;
            int backingFacetCount = tmpFacetCount;
            System.Threading.ThreadPool.QueueUserWorkItem(delegate(object obj)
            {
                for (int i = 0; i < backingFacetCount * 3; i++)
                {
                    nextSurface.indicies[i] = nextSurface.indicies[i] + offset - 1;
                    // Inventor has one-based indicies.  Zero-based is the way to go for everything except Inventor.
                }
                lockThing.Set();
            }, waitingThreads.Count);
        }
        #endregion

        postSurfaces.Add(nextSurface);

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
    public void ExportAll(ComponentOccurrence occ, bool bestResolution = false, bool separateFaces = false, bool ignorePhysics = false)
    {
        if (!ignorePhysics)
        {
            // Compute physics
            try
            {
                outputMesh.physics.Add((float) occ.MassProperties.Mass, Utilities.ToBXDVector(occ.MassProperties.CenterOfMass));
            }
            catch
            {
            }
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
        totalVolume /= occ.SubOccurrences.Count * adaptiveDegredation;

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

    /// <summary>
    /// Adds the mesh for the given components, and all their subcomponents to the mesh storage structure.
    /// </summary>
    /// <param name="occs">The components to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Export each face as a separate mesh</param>
    public void ExportAll(IEnumerable<ComponentOccurrence> occs, bool bestResolution = false, bool separateFaces = false)
    {
        foreach (ComponentOccurrence occ in occs)
        {
            ExportAll(occ, bestResolution, separateFaces);
        }
    }

    /// <summary>
    /// Adds the mesh for all the components and their subcomponenets in the custom rigid group.  <see cref="ExportAll(ComponentOccurrence,bool,bool,bool)"/>
    /// </summary>
    /// <remarks>
    /// This uses the best resolution and separate faces options stored inside the provided custom rigid group.
    /// </remarks>
    /// <param name="group">The group to export from</param>
    public void ExportAll(CustomRigidGroup group)
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
                ExportAll(occ, group.highRes, group.colorFaces);
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
