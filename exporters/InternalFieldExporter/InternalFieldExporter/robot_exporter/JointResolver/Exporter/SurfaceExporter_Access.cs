using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SurfaceExporter
{
    /// <summary>
    /// Clears the properties of an asset
    /// </summary>
    public SurfaceExporter()
    {
        assets.Clear();
    }
    
    /// <summary>
    /// Exports the field as a series of submeshes
    /// </summary>
    /// <param name="component"></param>
    /// <returns>A list of submeshes</returns>
    public List<BXDAMesh.BXDASubMesh> Export(ComponentOccurrence component)
    {
        MeshController outputMesh = new MeshController(Guid.NewGuid());
        List<SurfaceBody> plans = new List<SurfaceBody>();
        GenerateExportList(component, plans);

        for (int i = 0; i < plans.Count; i++)
        {
            CalculateSurfaceFacets(plans[i], outputMesh);
        }

        outputMesh.DumpOutput();
        return outputMesh.Mesh.meshes;
    }

    /// <summary>
    /// Exports all the components in this group to the in-RAM mesh.
    /// </summary>
    /// <param name="group">Group to export from</param>
    /// <param name="reporter">Progress reporter</param>
    /// <returns>A list of meshes<returns>
    public BXDAMesh ExportAll(CustomRigidGroup group, Guid guid, BXDIO.ProgressReporter reporter = null)
    {
        // Create output mesh
        MeshController outputMesh = new MeshController(guid);

        //Collect surfaces to export
        List<SurfaceBody> plannedSurfaces = CreateExportList(group);

        // Export faces, multithreaded
        if (plannedSurfaces.Count > 0)
        {
            // Reset progress bar
            reporter?.Invoke(0, plannedSurfaces.Count);

            // Start jobs
            int totalJobsFinished = 0;
            object finishLock = new object(); // Used to prevent multiple threads from updating progress bar at the same time.

            Parallel.ForEach(plannedSurfaces, (SurfaceBody surface) =>
            {
                CalculateSurfaceFacets(surface, outputMesh, SynthesisGUI.PluginSettings.GeneralUseFancyColors);

                lock (finishLock)
                {
                    totalJobsFinished++;
                    reporter?.Invoke(totalJobsFinished, plannedSurfaces.Count);
                }
            });

            outputMesh.DumpOutput();
        }

        return outputMesh.Mesh;

    }

}