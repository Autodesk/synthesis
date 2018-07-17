using Inventor;
using System.IO;
using System;
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
    /// <param name="occ">Component occurence to analize.</param>
    /// <param name="mesh">Mesh to store physics data in.</param>
    /// <param name="ignorePhysics">True to ignore physics in component.</param>
    /// <returns>All the sufaces to export</returns>
    private void GenerateExportList(ComponentOccurrence occ, List<SurfaceBody> plannedExports, PhysicalProperties physics, double minVolume = 0, bool ignorePhysics = false)
    {
        // Invisible objects don't need to be exported
        if (!occ.Visible)
            return;

        if (!ignorePhysics)
        {
            // Compute physics
            try
            {
                physics.Add((float) occ.MassProperties.Mass, Utilities.ToBXDVector(occ.MassProperties.CenterOfMass));
            }
            catch
            {
                Console.Write("Failed to get physics data for " + occ.Name);
            }
        }

        // Prepare exporting surfaces
        foreach (SurfaceBody surf in occ.SurfaceBodies)
            plannedExports.Add(surf);

        // Add sub-occurences
        foreach (ComponentOccurrence item in occ.SubOccurrences)
        {
            if (!adaptiveIgnoring || Utilities.BoxVolume(item.RangeBox) >= minVolume)
            {
                GenerateExportList(item, plannedExports, physics, minVolume, true);
            }
        }
    }

    /// <summary>
    /// Adds the mesh for all the components and their subcomponenets in the custom rigid group.  <see cref="ExportAll(ComponentOccurrence,bool,bool,bool)"/>
    /// </summary>
    /// <param name="group">The group to export from</param>
    /// <param name="mesh">Mesh to store physics data in.</param>
    /// <returns>All the sufaces to export</returns>
    private List<SurfaceBody> GenerateExportList(CustomRigidGroup group, PhysicalProperties physics)
    {
        List<SurfaceBody> plannedExports = new List<SurfaceBody>();

        // Calculate minimum volume to export a component
        double avgVolume = 0;
        foreach (ComponentOccurrence occ in group.occurrences)
        {
            avgVolume += Utilities.BoxVolume(occ.RangeBox);
        }
        avgVolume /= group.occurrences.Count;
        double minVolume = avgVolume * adaptiveDegredation;

        // Analyze all component occurrences
        foreach (ComponentOccurrence occ in group.occurrences)
        {
            if (!adaptiveIgnoring || Utilities.BoxVolume(occ.RangeBox) >= minVolume)
            {
                GenerateExportList(occ, plannedExports, physics, minVolume);
            }
        }
        
        return plannedExports
    }
}