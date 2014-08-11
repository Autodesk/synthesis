// Should we export textures.  (Useless currently)
// #define USE_TEXTURES

using Inventor;
using System.IO;
using System;
using System.Collections.Generic;

/// <summary>
/// Exports Inventor objects into the BXDA format.  One instance per thread.
/// </summary>
public class SurfaceExporter
{
    private class PartialSurface
    {
        public double[] verts = new double[TMP_VERTICIES * 3];
        public double[] norms = new double[TMP_VERTICIES * 3];
        public int[] indicies = new int[TMP_VERTICIES * 3];
#if USE_TEXTURES
        public double[] textureCoords = new double[TMP_VERTICIES * 2];
#endif
        public int vertCount = 0;
        public int facetCount = 0;
    }

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
    private PartialSurface tmpSurface = new PartialSurface();

    // Pre-submesh output
    private PartialSurface postSurface = new PartialSurface();
    private List<BXDAMesh.BXDASurface> postSurfaces = new List<BXDAMesh.BXDASurface>();

    private BXDAMesh outputMesh = new BXDAMesh();

    // Tolerances
    private double[] tolerances = new double[10];
    private int tmpToleranceCount = 0;

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

        #region SHOULD_SEPARATE_FACES
        AssetProperties sharedValue = null;
        string sharedDisp = null;
        if (separateFaces)  // Only separate if they are actually different colors
        {
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
                    else if (!ast.DisplayName.Equals(sharedDisp))
                    {
                        separateFaces = true;
                        break;
                    }
                }
                catch
                {
                }
            }
        }
        #endregion

        surf.GetExistingFacets(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies);
        if (tmpSurface.vertCount == 0)
        {
            surf.CalculateFacets(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies);
        }

        if (separateFaces || tmpSurface.vertCount > TMP_VERTICIES)
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
            tmpSurface.vertCount = 0;
#if USE_TEXTURES
            surf.GetExistingFacetsAndTextureMap(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies, out tmpSurface.textureCoords);
            if (tmpSurface.vertCount == 0)
            {
                surf.CalculateFacetsAndTextureMap(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out  tmpSurface.verts, out tmpSurface.norms, out  tmpSurface.indicies, out tmpSurface.textureCoords);
            }
#endif
            AssetProperties assetProps = sharedValue;
            if (sharedValue == null)
            {
                assetProps = AssetProperties.Create(surf);
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

        if (postSurface.vertCount == 0 || postSurface.facetCount == 0)
            return;
        BXDAMesh.BXDASubMesh subObject = new BXDAMesh.BXDASubMesh();
        subObject.verts = new double[postSurface.vertCount * 3];
        subObject.norms = new double[postSurface.vertCount * 3];
        Array.Copy(postSurface.verts, 0, subObject.verts, 0, postSurface.vertCount * 3);
        Array.Copy(postSurface.norms, 0, subObject.norms, 0, postSurface.vertCount * 3);
        Console.WriteLine("Mesh segment " + outputMesh.meshes.Count + " has " + postSurface.vertCount + " verts and " + postSurface.facetCount + " facets");
        subObject.surfaces = new List<BXDAMesh.BXDASurface>(postSurfaces);
        outputMesh.meshes.Add(subObject);

        postSurface.vertCount = 0;
        postSurface.facetCount = 0;
        postSurfaces = new List<BXDAMesh.BXDASurface>();
    }


    /// <summary>
    /// Moves the mesh currently in the temporary mesh buffer into the mesh structure itself, 
    /// with material information from the asset properties.
    /// </summary>
    /// <param name="assetProps">Material information to use</param>
    private void AddFacetsInternal(AssetProperties assetProps)
    {
        if (tmpSurface.vertCount > TMP_VERTICIES)
        {
            // This is just bad.  It could be fixed by exporting it per-face instead of with a single block.
            System.Windows.Forms.MessageBox.Show("Warning: Mesh segment exceededed " + TMP_VERTICIES + " verticies.  Strange things may begin to happen.");
        }
        // If adding this would cause the sub mesh to overflow dump what currently exists.
        if (tmpSurface.vertCount + postSurface.vertCount >= TMP_VERTICIES)
        {
            DumpMeshBuffer();
        }

        Array.Copy(tmpSurface.verts, 0, postSurface.verts, postSurface.vertCount * 3, tmpSurface.vertCount * 3);
        Array.Copy(tmpSurface.norms, 0, postSurface.norms, postSurface.vertCount * 3, tmpSurface.vertCount * 3);
#if USE_TEXTURES
        Array.Copy(tmpSurface.textureCoords, 0, postSurface.textureCoords, postSurface.vertCount * 2, tmpSurface.vertCount * 2);
#endif

        BXDAMesh.BXDASurface nextSurface = new BXDAMesh.BXDASurface();

        nextSurface.color = 0xFFFFFFFF;

        if (assetProps.color != null)
        {
            nextSurface.hasColor = true;
            nextSurface.color = ((uint) assetProps.color.Red << 0) | ((uint) assetProps.color.Green << 8) | ((uint) assetProps.color.Blue << 16) | ((((uint) (assetProps.color.Opacity * 255)) & 0xFF) << 24);
        }
        nextSurface.transparency = (float) assetProps.transparency;
        nextSurface.translucency = (float) assetProps.translucency;

        nextSurface.indicies = new int[tmpSurface.facetCount * 3];

        // Raw copy the indicies for now, then fix the offset in a background thread.
        Array.Copy(tmpSurface.indicies, nextSurface.indicies, nextSurface.indicies.Length);

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
            int offset = postSurface.vertCount;
            int backingFacetCount = tmpSurface.facetCount;
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

        postSurface.facetCount += tmpSurface.facetCount;
        postSurface.vertCount += tmpSurface.vertCount;
    }

    /// <summary>
    /// Copies mesh information from the Inventor API to the temporary mesh buffer, then into the mesh structure.
    /// </summary>
    /// <param name="surf">The source mesh</param>
    /// <param name="tolerance">The chord tolerance for the mesh</param>
    private void AddFacets(Face surf, double tolerance)
    {
        tmpSurface.vertCount = 0;
#if USE_TEXTURES
            surf.GetExistingFacetsAndTextureMap(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies, out tmpSurface.textureCoords);
            if (tmpSurface.vertCount == 0)
            {
                surf.CalculateFacetsAndTextureMap(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out  tmpSurface.verts, out tmpSurface.norms, out  tmpSurface.indicies, out tmpSurface.textureCoords);
            }
#else
        surf.GetExistingFacets(tolerance, out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies);
        if (tmpSurface.vertCount == 0)
        {
            surf.CalculateFacets(tolerance, out tmpSurface.vertCount, out tmpSurface.facetCount, out  tmpSurface.verts, out tmpSurface.norms, out  tmpSurface.indicies);
        }
#endif
        AssetProperties assetProps = AssetProperties.Create(surf);
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
