using Inventor;
using System.Collections.Generic;

public partial class SurfaceExporter
{
    /// <summary>
    /// Should the exporter attempt to automatically ignore small parts.
    /// </summary>
    private bool adaptiveIgnoring = true;
    /// <summary>
    /// The minimum ratio between a sub component's bounding box volume and the average bounding box volume for an object
    /// to be considered small.  The higher the number the less that is dropped, while if the value is one about half the objects
    /// would be dropped.
    /// </summary>
    private double adaptiveDegredation = 7;

    /// <summary>
    /// Adds the mesh for the given component, and all its subcomponents to the mesh storage structure.
    /// </summary>
    /// <param name="occ">The component to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Export each face as a separate mesh</param>
    /// <param name="ignorePhysics">Don't add the physical properties of this component to the exporter</param>
    /// <returns>All the sufaces to export</returns>
    private List<ExportPlan> GenerateExportList(ComponentOccurrence occ, bool bestResolution = false, bool separateFaces = false, bool ignorePhysics = false)
    {
        List<ExportPlan> plannedExports = new List<ExportPlan>();

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
            return plannedExports;

        foreach (SurfaceBody surf in occ.SurfaceBodies)
        {
            string line = "Including: " + surf.Parent.Name;
            plannedExports.Add(new ExportPlan(surf, bestResolution, separateFaces));
        }

        double totalVolume = 0;
        foreach (ComponentOccurrence occ2 in occ.SubOccurrences)
        {
            totalVolume += Utilities.BoxVolume(occ2.RangeBox);
        }
        totalVolume /= occ.SubOccurrences.Count * adaptiveDegredation;

        foreach (ComponentOccurrence item in occ.SubOccurrences)
        {
            if (!adaptiveIgnoring || Utilities.BoxVolume(item.RangeBox) >= totalVolume)
            {
                plannedExports.AddRange(GenerateExportList(item, bestResolution, separateFaces, true));
            }
        }
        return plannedExports;
    }

    /// <summary>
    /// Adds the mesh for the given component, and all its subcomponents to the mesh storage structure.
    /// </summary>
    /// <param name="pcd">The component to export</param>
    /// <param name="bestResolution">Use the best possible resolution</param>
    /// <param name="separateFaces">Export each face as a separate mesh</param>
    /// <param name="ignorePhysics">Don't add the physical properties of this component to the exporter</param>
    /// <returns>All the sufaces to export</returns>
    private List<ExportPlan> GenerateExportList(PartComponentDefinition pcd, bool bestResolution = false, bool separateFaces = false, bool ignorePhysics = false)
    {
        List<ExportPlan> plannedExports = new List<ExportPlan>();

        if (!ignorePhysics)
        {
            // Compute physics
            try
            {
                outputMesh.physics.Add((float)pcd.MassProperties.Mass, Utilities.ToBXDVector(pcd.MassProperties.CenterOfMass));
            }
            catch
            {
            }
        }

        foreach (SurfaceBody surf in pcd.SurfaceBodies)
        {
            plannedExports.Add(new ExportPlan(surf, bestResolution, separateFaces));
        }
        
        return plannedExports;
    }

}