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
#if LITEMODE
        LiteExporterForm.Instance.SetProgressText("Including parts...");
#else
        SynthesisGUI.Instance.ExporterSetSubText("Including parts");
#endif

        List<ExportPlan> plans = GenerateExportList(group);
        Console.WriteLine();
        reporter?.Invoke(0, plans.Count);
        for (int i = 0; i < plans.Count; i++)
        {
            AddFacets(plans[i].surf, plans[i].bestResolution, plans[i].separateFaces);
            reporter?.Invoke((i + 1), plans.Count);
        }
    }

    /// <summary>
    /// Exports all the components in this enumerable to the in-RAM mesh.
    /// </summary>
    /// <param name="enumm">Enumerable to export from</param>
    /// <param name="reporter">Progress reporter</param>
    public void ExportAll(IEnumerator<ComponentOccurrence> enumm, BXDIO.ProgressReporter reporter = null)
    {
        List<ExportPlan> plans = new List<ExportPlan>();
        while (enumm.MoveNext()){
            plans.AddRange(GenerateExportList(enumm.Current));
        }
        Console.WriteLine();
        if (reporter != null)
        {
            reporter(0, plans.Count);
        }
        for (int i = 0; i < plans.Count; i++)
        {
            AddFacets(plans[i].surf, plans[i].bestResolution, plans[i].separateFaces);
            if (reporter != null)
            {
                reporter((i + 1), plans.Count);
            }
        }
    }
}