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
        private bool separateFaces;

        private PartialSurface bufferSurface;

        private VertexCollection outputVerts;
        private List<BXDAMesh.BXDASurface> outputMeshSurfaces;

        private BXDAMesh outputMesh;

        /// <summary>
        /// Create a job to export a surface to a BXDAMesh
        /// </summary>
        /// <param name="surface">Surface to export.</param>
        /// <param name="outputMesh">Mesh to export to.</param>
        /// <param name="separateFaces">True to separate faces if the surface has multiple colors.</param>
        public ExportJob(SurfaceBody surface, BXDAMesh outputMesh, bool separateFaces = false)
        {
            surf = surface;
            this.separateFaces = separateFaces;

            bufferSurface = new PartialSurface();
            outputVerts = new VertexCollection();
            outputMeshSurfaces = new List<BXDAMesh.BXDASurface>();

            this.outputMesh = outputMesh;
        }

        /// <summary>
        /// Resets the assets dictionary for all jobs.
        /// </summary>
        public static void ResetAssets()
        {
            assets = new Dictionary<string, AssetProperties>();
        }

        /// <summary>
        /// Callback for thread pooling a job.
        /// </summary>
        /// <param name="threadContext">Should be of type <see cref="JobContext"/>. Stores information specific to the pooling of the job.</param>
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

        /// <summary>
        /// Used to store asset properties to prevent unnecessary calls to Inventor API.
        /// </summary>
        private static Dictionary<string, AssetProperties> assets = new Dictionary<string, AssetProperties>();

        /// <summary>
        /// Gets an AssetProperties based on an Inventor Asset.
        /// </summary>
        /// <param name="appearance">Inventor Asset to find AssetProperties based on.</param>
        /// <returns>AssetProperties based on the Inventor Asset. May be pre-existing or newly created.</returns>
        private AssetProperties GetAsset(Asset appearance)
        {
            string assetName = appearance.DisplayName;

            lock (assets)
            {
                if (assets == null)
                    assets = new Dictionary<string, AssetProperties>();

                if (!assets.ContainsKey(assetName))
                    assets.Add(assetName, new AssetProperties(appearance));
            }

            return assets[assetName];
        }

        /// <summary>
        /// Creates a list of faces from a surface.
        /// </summary>
        /// <param name="surf">Surface to analyze</param>
        /// <param name="faces">List of faces on surface</param>
        /// <returns>True if multiple assets exist on the surface.</returns>
        private bool AnalyzeFaces(SurfaceBody surf, out List<Face> faces)
        {
            List<string> uniqueAssets = new List<string>();

            faces = new List<Face>();
            foreach (Face face in surf.Faces)
            {
                faces.Add(face);

                if (!uniqueAssets.Contains(face.Appearance.DisplayName))
                    uniqueAssets.Add(face.Appearance.DisplayName);
            }

            return uniqueAssets.Count > 1;
        }

        /// <summary>
        /// Calculates the facets of a surface, storing them in <see cref="outputVerts"/> and <see cref="outputMeshSurfaces"/>.
        /// </summary>
        private void CalculateSurfaceFacets()
        {
            double tolerance = DEFAULT_TOLERANCE;

            // Stores a list of faces separate from the Inventor API
            List<Face> faces;

            if (separateFaces && AnalyzeFaces(surf, out faces))
            {
                // Add facets for each face of the surface
                foreach (Face face in faces)
                {
                    face.CalculateFacets(tolerance, out bufferSurface.verts.count, out bufferSurface.facets.count, out bufferSurface.verts.coordinates, out bufferSurface.verts.norms, out bufferSurface.facets.indices);
                    AddBufferToOutput(GetAsset(face.Appearance));
                }
            }
            else
            {
                // Add facets once for the entire surface
                surf.CalculateFacets(tolerance, out bufferSurface.verts.count, out bufferSurface.facets.count, out bufferSurface.verts.coordinates, out bufferSurface.verts.norms, out bufferSurface.facets.indices);
                AddBufferToOutput(GetAsset(surf.Faces[1].Appearance));
            }
        }

        /// <summary>
        /// Adds the vertices and facets in the <see cref="bufferSurface"/> to the <see cref="outputVerts"/> and <see cref="outputMeshSurfaces"/>.
        /// </summary>
        /// <param name="asset">Asset to associate with the buffer surface.</param>
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

        /// <summary>
        /// Adds the <see cref="outputVerts"/> and <see cref="outputMeshSurfaces"/> to the <see cref="outputMesh"/>.
        /// </summary>
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
