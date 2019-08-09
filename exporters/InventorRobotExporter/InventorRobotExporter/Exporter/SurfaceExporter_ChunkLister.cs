using System;
using System.Collections.Generic;
using InventorRobotExporter.RigidAnalyzer;
using InventorRobotExporter.Utilities;
using Inventor;

namespace InventorRobotExporter.Exporter
{
    public partial class SurfaceExporter
    {
        /// <summary>
        /// The minimum percent a sub component's bounding box volume of the largest bounding box volume for an object
        /// to be considered small. The lower the number the less that is dropped.
        /// </summary>
        private const double MIN_VOLUME_PERCENT = 0.0002;

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
                    physics.Add((float) occ.MassProperties.Mass, MathUtils.ToBXDVector(occ.MassProperties.CenterOfMass));
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
            foreach (ComponentOccurrence subOcc in occ.SubOccurrences)
            {
                if (MathUtils.BoxVolume(subOcc.RangeBox) >= minVolume)
                {
                    GenerateExportList(subOcc, plannedExports, physics, minVolume, true);
                }
            }
        }

        /// <summary>
        /// Adds the mesh for all the components and their subcomponenets in the custom rigid group.  <see cref="ExportAll"/>
        /// </summary>
        /// <param name="group">The group to export from</param>
        /// <param name="mesh">Mesh to store physics data in.</param>
        /// <returns>All the sufaces to export</returns>
        private List<SurfaceBody> GenerateExportList(CustomRigidGroup group, PhysicalProperties physics)
        {
            List<SurfaceBody> plannedExports = new List<SurfaceBody>();

            // Calculate minimum volume to export a component
            double maxVolume = 0;
            foreach (ComponentOccurrence occ in group.occurrences)
            {
                double curVolume = MathUtils.BoxVolume(occ.RangeBox);
                if (curVolume > maxVolume)
                    maxVolume = curVolume;
            }
            double minVolume = maxVolume * MIN_VOLUME_PERCENT;

            // Analyze all component occurrences
            foreach (ComponentOccurrence occ in group.occurrences)
            {
                if (MathUtils.BoxVolume(occ.RangeBox) >= minVolume)
                {
                    GenerateExportList(occ, plannedExports, physics, minVolume);
                }
            }

            return plannedExports;
        }
    }
}