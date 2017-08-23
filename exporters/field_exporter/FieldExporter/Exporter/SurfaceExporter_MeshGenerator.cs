// Should we export textures.  (Useless currently)
// #define USE_TEXTURES

using Inventor;
using System;
using System.Collections.Generic;

/// <summary>
/// Exports Inventor objects into the BXDA format.  One instance per thread.
/// </summary>
public partial class SurfaceExporter
{
    private struct ExportPlan
    {
        public SurfaceBody surf;
        public bool bestResolution;
        public bool separateFaces;

        public ExportPlan(SurfaceBody b, bool bestResolution, bool separateFaces)
        {
            surf = b;
            this.bestResolution = bestResolution;
            this.separateFaces = separateFaces;
        }
    }

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
    private void AddFacets(SurfaceBody surf, bool bestResolution = false, bool separateFaces = false)
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

#if USE_TEXTURES
            surf.GetExistingFacetsAndTextureMap(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies, out tmpSurface.textureCoords);
            if (tmpSurface.vertCount == 0)
            {
                surf.CalculateFacetsAndTextureMap(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out  tmpSurface.verts, out tmpSurface.norms, out  tmpSurface.indicies, out tmpSurface.textureCoords);
            }
#else
        surf.GetExistingFacets(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies);
        if (tmpSurface.vertCount == 0)
        {
            surf.CalculateFacets(tolerances[bestIndex], out tmpSurface.vertCount, out tmpSurface.facetCount, out tmpSurface.verts, out  tmpSurface.norms, out  tmpSurface.indicies);
        }
#endif
        if (separateFaces || tmpSurface.vertCount > TMP_VERTICIES)
        {
            int i = 0;
            foreach (Face f in surf.Faces)
            {
                i++;
                AddFacets(f, tolerances[bestIndex]);
            }
        }
        else
        {
            //Console.WriteLine("Exporting single block for " + surf.Parent.Name + "\t(" + surf.Name + ")");
            AssetProperties assetProps = sharedValue;
            if (sharedValue == null)
            {
                assetProps = AssetProperties.Create(surf);
            }

            if (assetProps != null)
            {
                AddFacetsInternal(assetProps);
            }
        }
    }

    private void DumpMeshBuffer()
    {
        // Make sure all index copy threads have completed.
        if (waitingThreads.Count > 0)
        {
         //   Console.WriteLine("Got ahead of ourselves....");
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
        nextSurface.specular = (float) assetProps.specular;

        nextSurface.indicies = new int[tmpSurface.facetCount * 3];

        // Raw copy the indicies for now, then fix the offset in a background thread.
        Array.Copy(tmpSurface.indicies, nextSurface.indicies, nextSurface.indicies.Length);

        #region Fix Index Buffer Offset
        // Make sure we haven't exceeded the maximum number of background tasks.
        if (waitingThreads.Count > MAX_BACKGROUND_THREADS)
        {
           // Console.WriteLine("Got ahead of ourselves....");
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
}
