using Inventor;
using System.Collections.Generic;

public partial class SurfaceExporter
{
    /// <summary>
    /// Clears the mesh structure and physical properties, 
    /// preparing this exporter for another set of objects.
    /// </summary>
    public void Reset()
    {
        outputMesh = new BXDAMesh();
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
    /// Exports from a ComponentOccurrence.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="bestResolution"></param>
    /// <param name="separateFaces"></param>
    /// <param name="ignorePhysics"></param>
    public void Export(ComponentOccurrence component, bool bestResolution = false,
        bool separateFaces = false, bool ignorePhysics = false)
    {
        List<ExportPlan> plans = new List<ExportPlan>();
        plans.AddRange(GenerateExportList(component, bestResolution, separateFaces, ignorePhysics));

        for (int i = 0; i < plans.Count; i++)
        {
            AddFacets(plans[i].surf, plans[i].bestResolution, plans[i].separateFaces);
        }
    }

    /// <summary>
    /// Exports from a ComponentPartDefinition.
    /// </summary>
    /// <param name="pcd"></param>
    /// <param name="bestResolution"></param>
    /// <param name="separateFaces"></param>
    /// <param name="ignorePhysics"></param>
    public void Export(PartComponentDefinition pcd, bool bestResolution = false,
        bool separateFaces = false, bool ignorePhysics = false)
    {
        List<ExportPlan> plans = new List<ExportPlan>();
        plans.AddRange(GenerateExportList(pcd, bestResolution, separateFaces, ignorePhysics));

        for (int i = 0; i < plans.Count; i++)
        {
            AddFacets(plans[i].surf, plans[i].bestResolution, plans[i].separateFaces);
        }
    }
}