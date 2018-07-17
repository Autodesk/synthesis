using Inventor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

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
    private void GenerateExportList(ComponentOccurrence occ, ConcurrentBag<SurfaceBody> plannedExports, PhysicalProperties physics, bool ignorePhysics = false)
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
        {
            plannedExports.Add(surf);
        }

        // Add sub-occurences
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
                GenerateExportList(item, plannedExports, physics, true);
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
        ConcurrentBag<SurfaceBody> plannedExports = new ConcurrentBag<SurfaceBody>();

        double totalVolume = 0;
        foreach (ComponentOccurrence occ in group.occurrences)
        {
            totalVolume += Utilities.BoxVolume(occ.RangeBox);
        }
        totalVolume /= group.occurrences.Count * adaptiveDegredation;

        Parallel.ForEach(group.occurrences, (ComponentOccurrence occ) =>
        {
            if (!adaptiveIgnoring || Utilities.BoxVolume(occ.RangeBox) >= totalVolume)
            {
                GenerateExportList(occ, plannedExports, physics);
            }
        });

        List<SurfaceBody> plannedExportList = new List<SurfaceBody>(plannedExports);
        return plannedExportList;
    }
}