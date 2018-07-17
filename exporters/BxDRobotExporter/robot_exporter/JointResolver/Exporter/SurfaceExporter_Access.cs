using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class SurfaceExporter
{
    public SurfaceExporter()
    {
        assets.Clear();
    }

    /// <summary>
    /// Exports all the components in this group to the in-RAM mesh.
    /// </summary>
    /// <param name="group">Group to export from</param>
    /// <param name="reporter">Progress reporter</param>
    public BXDAMesh ExportAll(CustomRigidGroup group, Guid guid, BXDIO.ProgressReporter reporter = null)
    {
        // Create output mesh
        MeshController outputMesh = new MeshController(guid);

        // Collect surfaces to export
        List<SurfaceBody> plannedSurfaces = GenerateExportList(group, outputMesh.Mesh.physics);

        // Export faces, multithreaded
        reporter?.Invoke(0, plannedSurfaces.Count);

        // Start jobs
        int totalJobsFinished = 0;
        object finishLock = new object(); // Used to prevent multiple threads from updating progress bar at the same time.

        Parallel.ForEach(plannedSurfaces, (SurfaceBody surface) =>
        {
        //SurfaceBody surface = plan.surface;
            CalculateSurfaceFacets(surface, outputMesh, SynthesisGUI.PluginSettings.GeneralUseFancyColors);// && !plan.simplify);

            lock (finishLock)
            {
                totalJobsFinished++;
                reporter?.Invoke(totalJobsFinished, plannedSurfaces.Count);
            }
        });

        outputMesh.DumpOutput();
        return outputMesh.Mesh;
    }
}