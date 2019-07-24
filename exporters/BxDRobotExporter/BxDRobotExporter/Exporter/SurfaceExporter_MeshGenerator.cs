// Should we export textures.  (Useless currently)
// #define USE_TEXTURES

using System;
using System.Collections.Generic;
using Inventor;

namespace BxDRobotExporter.Exporter
{
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
            public double[] Coordinates = new double[MAX_VERTS_OR_FACETS * 3];
            public double[] Norms = new double[MAX_VERTS_OR_FACETS * 3];
            public int Count = 0;
        }

        private class FacetCollection
        {
            public int[] Indices = new int[MAX_VERTS_OR_FACETS * 3];
            public int Count = 0;
        }

        private class PartialSurface
        {
            public VertexCollection Verts = new VertexCollection();
            public FacetCollection Facets = new FacetCollection();
        }

        /// <summary>
        /// Default tolerance used when generating meshes (cm)
        /// </summary>
        private const double DEFAULT_TOLERANCE = 1;

        /// <summary>
        /// Used to store asset properties to prevent unnecessary calls to Inventor API.
        /// </summary>
        private static Dictionary<string, AssetProperties> assets = new Dictionary<string, AssetProperties>();

        /// <summary>
        /// Gets an AssetProperties based on an Inventor Asset.
        /// </summary>
        /// <param name="appearance">Inventor Asset to find AssetProperties based on.</param>
        /// <returns>AssetProperties based on the Inventor Asset. May be pre-existing or newly created.</returns>
        private AssetProperties GetAssetProperties(Asset appearance)
        {
            string assetName = appearance.DisplayName;

            lock (assets)
            {
                if (assets == null)
                    assets = new Dictionary<string, AssetProperties>();

                if (!assets.ContainsKey(assetName))
                    assets.Add(assetName, new AssetProperties(appearance));

                return assets[assetName];
            }
        }

        /// <summary>
        /// Checks if a surface uses multiple assets.
        /// </summary>
        /// <param name="surf">Surface to analyze</param>
        /// <returns>True if multiple assets exist on the surface.</returns>
        private bool MultipleAssets(Faces surfaceFaces)
        {
            string firstAssetName = null;

            foreach (Face face in surfaceFaces)
            {
                if (firstAssetName == null)
                    firstAssetName = face.Appearance.DisplayName;
                else if (face.Appearance.DisplayName != firstAssetName)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates the facets of a surface, storing them in a <see cref="MeshController"/>.
        /// </summary>
        private void CalculateSurfaceFacets(SurfaceBody surf, MeshController outputMesh, bool separateFaces = false)
        {
            double tolerance = DEFAULT_TOLERANCE;

            PartialSurface bufferSurface = new PartialSurface();

            // Store a list of faces separate from the Inventor API
            Faces faces = surf.Faces;

            // Don't separate if only one color
            if (separateFaces)
                separateFaces = MultipleAssets(faces);

            // Do separate if too many faces
            if (!separateFaces)
            {
                surf.CalculateFacets(tolerance, out bufferSurface.Verts.Count, out bufferSurface.Facets.Count, out bufferSurface.Verts.Coordinates, out bufferSurface.Verts.Norms, out bufferSurface.Facets.Indices);

                if (bufferSurface.Verts.Count > MAX_VERTS_OR_FACETS || bufferSurface.Facets.Count > MAX_VERTS_OR_FACETS)
                    separateFaces = true;
            }

            if (separateFaces)
            {
                // Add facets for each face of the surface
                foreach (Face face in faces)
                {
                    if (face.Appearance != null) { 
                        face.CalculateFacets(tolerance, out bufferSurface.Verts.Count, out bufferSurface.Facets.Count, out bufferSurface.Verts.Coordinates, out bufferSurface.Verts.Norms, out bufferSurface.Facets.Indices);
                        outputMesh.AddSurface(ref bufferSurface, GetAssetProperties(face.Appearance));
                    }
                }
            }
            else
            {
                // Add facets once for the entire surface
                AssetProperties asset;
                try
                {
                    asset = GetAssetProperties(faces[1].Appearance);
                }
                catch (Exception)
                {
                    asset = new AssetProperties();
                }

                outputMesh.AddSurface(ref bufferSurface, asset);
            }
        }

        /// <summary>
        /// Used for storing shared mesh data.
        /// </summary>
        private class MeshController
        {
            private VertexCollection outputVerts = new VertexCollection();
            private List<BXDAMesh.BXDASurface> outputMeshSurfaces = new List<BXDAMesh.BXDASurface>();

            private BXDAMesh outputMesh;
            public BXDAMesh Mesh { get => outputMesh; }

            public MeshController(Guid guid)
            {
                outputMesh = new BXDAMesh(guid);
            }

            /// <summary>
            /// Adds a surface to the output mesh. Not saved as a sub-surface until <see cref="DumpOutput"/> is called.
            /// </summary>
            /// <param name="bufferSurface">Surface to add to mesh.</param>
            /// <param name="asset">Asset for surface.</param>
            public void AddSurface(ref PartialSurface bufferSurface, AssetProperties asset)
            {
                // Create new surface
                BXDAMesh.BXDASurface newMeshSurface = new BXDAMesh.BXDASurface();

                // Apply Asset Properties
                if (asset == null)
                    return;

                newMeshSurface.hasColor = true;
                newMeshSurface.color = asset.Color;
                newMeshSurface.transparency = (float)asset.Transparency;
                newMeshSurface.translucency = (float)asset.Translucency;
                newMeshSurface.specular = (float)asset.Specular;

                // Prevent too many vertices
                if (bufferSurface.Verts.Count > MAX_VERTS_OR_FACETS)
                    throw new TooManyVerticesException();

                int indexOffset;
                lock (outputVerts)
                {
                    // Prevent too many vertices
                    if (outputVerts.Count + bufferSurface.Verts.Count > MAX_VERTS_OR_FACETS)
                        DumpOutputInternal();
                
                    // Copy buffer vertices into output
                    Array.Copy(bufferSurface.Verts.Coordinates, 0, outputVerts.Coordinates, outputVerts.Count * 3, bufferSurface.Verts.Count * 3);
                    Array.Copy(bufferSurface.Verts.Norms, 0, outputVerts.Norms, outputVerts.Count * 3, bufferSurface.Verts.Count * 3);

                    // Store length of output verts for later
                    indexOffset = outputVerts.Count - 1;
                    outputVerts.Count += bufferSurface.Verts.Count;
                }

                // Copy buffer surface into output, incrementing indices relative to where verts where stitched into the vert array
                newMeshSurface.indicies = new int[bufferSurface.Facets.Count * 3];
                for (int i = 0; i < bufferSurface.Facets.Count * 3; i++)
                    newMeshSurface.indicies[i] = bufferSurface.Facets.Indices[i] + indexOffset; // Why does Inventor start from 1?!

                // Add the new surface to the output
                lock (outputMeshSurfaces)
                    outputMeshSurfaces.Add(newMeshSurface);

                // Empty buffer
                bufferSurface.Verts.Count = 0;
                bufferSurface.Facets.Count = 0;
            }
        
            /// <summary>
            /// Adds the <see cref="outputVerts"/> and <see cref="outputMeshSurfaces"/> to the <see cref="outputMesh"/>. Thread safe.
            /// </summary>
            public void DumpOutput()
            {
                lock (outputVerts)
                lock (outputMeshSurfaces)
                    DumpOutputInternal();
            }

            /// <summary>
            /// Adds the <see cref="outputVerts"/> and <see cref="outputMeshSurfaces"/> to the <see cref="outputMesh"/>.
            /// </summary>
            private void DumpOutputInternal()
            {             
                if (outputVerts.Count == 0 || outputMeshSurfaces.Count == 0)
                    return;

                // Copy the output surface's vertices and normals into the new sub-object
                BXDAMesh.BXDASubMesh subObject = new BXDAMesh.BXDASubMesh();
                subObject.verts = new double[outputVerts.Count * 3];
                subObject.norms = new double[outputVerts.Count * 3];
                Array.Copy(outputVerts.Coordinates, 0, subObject.verts, 0, outputVerts.Count * 3);
                Array.Copy(outputVerts.Norms, 0, subObject.norms, 0, outputVerts.Count * 3);

                // Copy the output surfaces into the new sub-object
                subObject.surfaces = new List<BXDAMesh.BXDASurface>(outputMeshSurfaces);

                // Add the sub-object to the output mesh
                lock (outputMesh)
                    outputMesh.meshes.Add(subObject);

                // Empty temporary storage
                outputVerts.Count = 0;
                outputMeshSurfaces.Clear();
            }
        }
    }
}
