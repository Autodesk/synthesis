// Should we export textures.  (Useless currently)
// #define USE_TEXTURES

using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// Exports Inventor objects into the BXDA format.  One instance per thread.
/// </summary>
public partial class SurfaceExporter
{
    public class TooManyVerticesException : ApplicationException
    {
        public TooManyVerticesException() : base("Too many vertices in a surface.") { }
    }

    private const uint MAX_VERTS_OR_FACETS = ushort.MaxValue;
    private class VertexCollection
    {
        public double[] coordinates = new double[MAX_VERTS_OR_FACETS * 3];
        public double[] norms = new double[MAX_VERTS_OR_FACETS * 3];
        public int count = 0;
    }

    private class FacetCollection
    {
        public int[] indices = new int[MAX_VERTS_OR_FACETS * 3];
        public int count = 0;
    }

    private class PartialSurface
    {
        public VertexCollection verts = new VertexCollection();
        public FacetCollection facets = new FacetCollection();
    }

    /// <summary>
    /// Default tolerance used when generating meshes (cm)
    /// </summary>
    private const double DEFAULT_TOLERANCE = 1;

    private class ExportJob
    {
        public delegate void JobFinishedReporter();

        public struct JobContext
        {
            public ManualResetEvent doneEvent;
            public JobFinishedReporter onFinish;
        }

        public Exception error = null;

        private SurfaceBody surf;
        private bool bestResolution;
        private bool separateFaces;

        private PartialSurface bufferSurface;

        private VertexCollection outputVerts;
        private List<BXDAMesh.BXDASurface> outputMeshSurfaces;

        private BXDAMesh outputMesh;

        public ExportJob(SurfaceBody surface, BXDAMesh outputMesh, bool bestResolution = false, bool separateFaces = false)
        {
            surf = surface;
            this.bestResolution = bestResolution;
            this.separateFaces = separateFaces;

            bufferSurface = new PartialSurface();
            outputVerts = new VertexCollection();
            outputMeshSurfaces = new List<BXDAMesh.BXDASurface>();

            this.outputMesh = outputMesh;
        }

        public void ThreadPoolCallback(object threadContext)
        {
            if (threadContext is JobContext context)
            {
                try
                {
                    CalculateSurfaceFacets();
                    DumpOutputMesh();
                }
                catch (Exception e)
                {
                    error = e;
                }
                finally
                {
                    context.doneEvent.Set();
                    context.onFinish?.Invoke();
                }
            }
        }

        private double GetTolerance()
        {
            int toleranceCount = 10;
            double[] tolerances = new double[10];
            surf.GetExistingFacetTolerances(out toleranceCount, out tolerances);

            double tolerance = DEFAULT_TOLERANCE;

            int bestIndex = -1;
            for (int i = 0; i < toleranceCount; i++)
            {
                if (bestIndex < 0 || tolerances[i] < tolerances[bestIndex])
                {
                    bestIndex = i;
                }
            }

            if (bestResolution || tolerances[bestIndex] > tolerance)
                tolerance = tolerances[bestIndex];

            return tolerance;
        }

        private bool CheckShouldSeparate(out Dictionary<string, AssetProperties> assets)
        {
            AssetProperties firstAsset = null;
            assets = new Dictionary<string, AssetProperties>();

            // Create an asset for each unique face appearance
            foreach (Face f in surf.Faces)
            {
                try
                {
                    if (!assets.ContainsKey(f.Appearance.DisplayName))
                    {
                        assets.Add(f.Appearance.DisplayName, new AssetProperties(f.Appearance)); // Used to quickly access asset properties later

                        if (firstAsset == null)
                            firstAsset = assets[f.Appearance.DisplayName];
                    }
                }
                catch
                {
                    // Failed to create asset for face
                }
            }

            // If more than one different assets exist, separate faces
            return assets.Count > 1;
        }

        private void CalculateSurfaceFacets()
        {
            double tolerance = GetTolerance();
            
            if (separateFaces && CheckShouldSeparate(out Dictionary<string, AssetProperties> assets))
            {
                // Add facets for each face of the surface
                foreach (Face face in surf.Faces)
                {
                    face.GetExistingFacets(tolerance, out bufferSurface.verts.count, out bufferSurface.facets.count, out bufferSurface.verts.coordinates, out bufferSurface.verts.norms, out bufferSurface.facets.indices);

                    if (bufferSurface.verts.count == 0)
                        face.CalculateFacets(tolerance, out bufferSurface.verts.count, out bufferSurface.facets.count, out bufferSurface.verts.coordinates, out bufferSurface.verts.norms, out bufferSurface.facets.indices);

                    AddBufferToOutput(assets[face.Appearance.DisplayName]);
                }
            }
            else
            {
                // Add facets once for the entire surface
                AssetProperties asset = new AssetProperties(surf.Faces[1].Appearance);

                surf.GetExistingFacets(tolerance, out bufferSurface.verts.count, out bufferSurface.facets.count, out bufferSurface.verts.coordinates, out bufferSurface.verts.norms, out bufferSurface.facets.indices);

                if (bufferSurface.verts.count == 0)
                    surf.CalculateFacets(tolerance, out bufferSurface.verts.count, out bufferSurface.facets.count, out bufferSurface.verts.coordinates, out bufferSurface.verts.norms, out bufferSurface.facets.indices);

                AddBufferToOutput(asset);
            }
        }

        private void AddBufferToOutput(AssetProperties asset)
        {
            if (bufferSurface.verts.count > MAX_VERTS_OR_FACETS)
                throw new TooManyVerticesException();

            if (outputVerts.count + bufferSurface.verts.count > MAX_VERTS_OR_FACETS)
                DumpOutputMesh();

            BXDAMesh.BXDASurface newMeshSurface = new BXDAMesh.BXDASurface();

            // Apply Asset Properties
            if (asset == null)
                return;

            newMeshSurface.hasColor = true;
            newMeshSurface.color = asset.color;
            newMeshSurface.transparency = (float)asset.transparency;
            newMeshSurface.translucency = (float)asset.translucency;
            newMeshSurface.specular = (float)asset.specular;

            // Copy buffer vertices into output
            Array.Copy(bufferSurface.verts.coordinates, 0, outputVerts.coordinates, outputVerts.count * 3, bufferSurface.verts.count * 3);
            Array.Copy(bufferSurface.verts.norms, 0, outputVerts.norms, outputVerts.count * 3, bufferSurface.verts.count * 3);

            // Copy buffer surface into output, incrementing indices relative to where verts where stitched into the vert array
            newMeshSurface.indicies = new int[bufferSurface.facets.count * 3];
            for (int i = 0; i < bufferSurface.facets.count * 3; i++)
                newMeshSurface.indicies[i] = bufferSurface.facets.indices[i] + outputVerts.count - 1; // Why does Inventor start from 1?!

            // Increment the number of verts in output
            outputVerts.count += bufferSurface.verts.count;

            // Add the new surface to the output
            outputMeshSurfaces.Add(newMeshSurface);
        }

        private void DumpOutputMesh()
        {
            if (outputVerts.count == 0 || outputMeshSurfaces.Count == 0)
                return;

            // Copy the output surface's vertices and normals into the new sub-object
            BXDAMesh.BXDASubMesh subObject = new BXDAMesh.BXDASubMesh();
            subObject.verts = new double[outputVerts.count * 3];
            subObject.norms = new double[outputVerts.count * 3];
            Array.Copy(outputVerts.coordinates, 0, subObject.verts, 0, outputVerts.count * 3);
            Array.Copy(outputVerts.norms, 0, subObject.norms, 0, outputVerts.count * 3);

            // Copy the output surfaces into the new sub-object
            subObject.surfaces = new List<BXDAMesh.BXDASurface>(outputMeshSurfaces);

            // Add the sub-object to the output mesh
            lock (outputMesh)
                outputMesh.meshes.Add(subObject);

            // Empty temporary storage
            outputVerts = new VertexCollection();
            outputMeshSurfaces.Clear();
        }
    }
}
