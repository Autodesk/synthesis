using System;
using System.Collections.Generic;
using Inventor;

public class Exporter
{

    private const int MAX_VERTICIES = 8192;
    
    public class EmptyAssemblyException : ApplicationException
    {
        public EmptyAssemblyException() : base("No parts in assembly.") { }
    }

    public class InvalidJointException : ApplicationException
    {
        AssemblyJoint joint;

        public InvalidJointException(string message, AssemblyJoint joint) : base(message)
        {
            this.joint = joint;
        }
    }

    public class NoGroundException : ApplicationException
    {
        public NoGroundException() : base("Assembly has no ground.") { }
    }

    public static RigidNode_Base ExportSkeleton(List<ComponentOccurrence> occurrences)
    {
        if (occurrences.Count == 0) throw new Exception("No components selected!");

//        SynthesisGUI.Instance.ExporterSetOverallText("Centering joints");

//        SynthesisGUI.Instance.ExporterReset();
//        SynthesisGUI.Instance.ExporterSetSubText("Centering 0 / 0");
//        SynthesisGUI.Instance.ExporterSetProgress(0);
//        SynthesisGUI.Instance.ExporterSetMeshes(2);

        int numOccurrences = occurrences.Count;

//        SynthesisGUI.Instance.ExporterStepOverall();
//        SynthesisGUI.Instance.ExporterSetOverallText("Getting rigid info");

        Console.WriteLine("Get rigid info...");
        //Group components into rigid bodies.
        NameValueMap options = InventorManager.Instance.TransientObjects.CreateNameValueMap();
        options.Add("DoubleBearing", false);
        RigidBodyResults rigidResults = InventorManager.Instance.AssemblyDocument.ComponentDefinition.RigidBodyAnalysis(options);

        Console.WriteLine("Got rigid info...");
        CustomRigidResults customRigid = new CustomRigidResults(rigidResults);

        Console.WriteLine("Build model...");
        RigidBodyCleaner.CleanGroundedBodies(customRigid);
        //After this point, all grounded groups have been merged into one CustomRigidGroup, and their joints have been updated.

        RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(customRigid);
        Console.WriteLine("Built");

        Console.WriteLine(baseNode.ToString());

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        foreach (RigidNode_Base node in nodes)
        {
            node.ModelFileName = ((RigidNode)node).group.ToString();
            node.ModelFullID = node.GetModelID();
        }

        return baseNode;
    }
    
    public static List<BXDAMesh> ExportMeshes(RigidNode_Base baseNode, bool useOCL = false)
    {
        SurfaceExporter surfs = new SurfaceExporter();
        BXDJSkeleton.SetupFileNames(baseNode);

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        List<BXDAMesh> meshes = new List<BXDAMesh>();
        foreach (RigidNode_Base node in nodes)
        {
//            SynthesisGUI.Instance.ExporterSetOverallText("Exporting " + node.ModelFileName);

            if (node is RigidNode && node.GetModel() != null && node.ModelFileName != null && node.GetModel() is CustomRigidGroup)
            {
                Console.WriteLine("Exporting " + node.ModelFileName);

                try
                {
//                    SynthesisGUI.Instance.ExporterReset();
                    CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
                    Console.WriteLine("Exporting meshes...");
                    BXDAMesh output = surfs.ExportAll(group, node.GUID, (long progress, long total) =>
                    {
                        double totalProgress = (((double)progress / (double)total) * 100.0);
//                        SynthesisGUI.Instance.ExporterSetSubText(String.Format("Export {1} / {2}", Math.Round(totalProgress, 2), progress, total));
//                        SynthesisGUI.Instance.ExporterSetProgress(totalProgress);
                    });
                    Console.WriteLine();
                    Console.WriteLine("Output: " + output.meshes.Count + " meshes");
                    Console.WriteLine("Computing colliders...");
                    output.colliders.Clear();
                    output.colliders.AddRange(ConvexHullCalculator.GetHull(output));

                    meshes.Add(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw new Exception("Error exporting mesh: " + node.GetModelID());
                }
            }

//            SynthesisGUI.Instance.ExporterStepOverall();
        }

        return meshes;
    }
}
