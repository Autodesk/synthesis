using Inventor;
using System.IO;
using System;
using System.Collections.Generic;

public partial class SurfaceExporter
{
    /// <summary>
    /// Clears the mesh structure and physical properties, 
    /// preparing this exporter for another set of objects.
    /// </summary>
    public void Reset(Guid guid)
    {
        outputMesh = new BXDAMesh(guid);
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

    /// <summary>
    /// Exports all the components in this group to the in-RAM mesh.
    /// </summary>
    /// <param name="group">Group to export from</param>
    /// <param name="reporter">Progress reporter</param>
    public void ExportAll(CustomRigidGroup group, BXDIO.ProgressReporter reporter = null)
    {
        // Collect faces to export
        List<SurfaceBody> plannedSurfaces = GenerateExportList(group);

        // Export faces
        reporter?.Invoke(0, plannedSurfaces.Count);
        for (int i = 0; i < plannedSurfaces.Count; i++)
        {
            AddFacets(plannedSurfaces[i], false, SynthesisGUI.PluginSettings.GeneralUseFancyColors);
            reporter?.Invoke((i + 1), plannedSurfaces.Count);
        }
    }
}