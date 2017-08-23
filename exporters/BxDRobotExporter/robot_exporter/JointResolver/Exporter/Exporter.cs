using System;
using System.Collections.Generic;
using Inventor;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

public class Exporter
{

    private const int MAX_VERTICIES = 8192;

    public static void CenterAllJoints(ComponentOccurrence component)
    {
        Console.Write("Centering: " + component.Name);
        foreach (AssemblyJoint joint in component.Joints)
        {
            //Takes the average of the linear or rotational limits and sets the joints position to it.
            if (joint.Definition.JointType == AssemblyJointTypeEnum.kCylindricalJointType || joint.Definition.JointType == AssemblyJointTypeEnum.kRotationalJointType)
            {
                if (joint.Definition.HasAngularPositionLimits)
                {
                    joint.Definition.AngularPosition = (joint.Definition.AngularPositionStartLimit.Value + joint.Definition.AngularPositionEndLimit.Value) / 2.0;
                }
            }

            if (joint.Definition.JointType == AssemblyJointTypeEnum.kCylindricalJointType || joint.Definition.JointType == AssemblyJointTypeEnum.kSlideJointType)
            {
                if (joint.Definition.HasLinearPositionStartLimit && joint.Definition.HasLinearPositionEndLimit)
                {
                    joint.Definition.LinearPosition = (joint.Definition.LinearPositionStartLimit.Value + joint.Definition.LinearPositionEndLimit.Value) / 2.0;
                }
                else
                {
                    //No robot would have a piece that would just keep going.
                    throw new Exception("Joints with linear motion require limits.");
                }
            }
        }

        try
        {
            //Contiues down to subassemblies.
            foreach (ComponentOccurrence subComponent in component.SubOccurrences)
            {
                 CenterAllJoints(subComponent);
            }
        }
        catch
        {
        }
    }

    public static RigidNode_Base ExportSkeleton(List<ComponentOccurrence> occurrences)
    {
        if (occurrences.Count == 0) throw new Exception("No components selected!");

        SynthesisGUI.Instance.ExporterSetOverallText("Centering joints");

        SynthesisGUI.Instance.ExporterReset();
        SynthesisGUI.Instance.ExporterSetSubText("Centering 0 / 0");
        SynthesisGUI.Instance.ExporterSetProgress(0);
        SynthesisGUI.Instance.ExporterSetMeshes(2);

        int numOccurrences = occurrences.Count;
        int current = 0;

        //Centers all the joints for each component.  Done to match the assembly's joint position with the subassembly's position.
        foreach (ComponentOccurrence component in occurrences)
        {
            CenterAllJoints(component);
            double totalProgress = (((double) (current + 1) / (double) numOccurrences) * 100.0);
            SynthesisGUI.Instance.ExporterSetSubText(String.Format("Centering {1} / {2}", Math.Round(totalProgress, 2), (current + 1), numOccurrences));
            SynthesisGUI.Instance.ExporterSetProgress(totalProgress);
            current++;
        }
        Console.WriteLine();

        SynthesisGUI.Instance.ExporterStepOverall();
        SynthesisGUI.Instance.ExporterSetOverallText("Getting rigid info");

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

        SynthesisGUI.Instance.ExporterStepOverall();

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
        BXDJSkeleton.SetupFileNames(baseNode, true);

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        SynthesisGUI.Instance.ExporterSetMeshes(nodes.Count);

        List<BXDAMesh> meshes = new List<BXDAMesh>();
        foreach (RigidNode_Base node in nodes)
        {
            SynthesisGUI.Instance.ExporterSetOverallText("Exporting " + node.ModelFileName);

            if (node is RigidNode && node.GetModel() != null && node.ModelFileName != null && node.GetModel() is CustomRigidGroup)
            {
                Console.WriteLine("Exporting " + node.ModelFileName);

                try
                {
                    SynthesisGUI.Instance.ExporterReset();
                    CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
                    surfs.Reset(node.GUID);
                    Console.WriteLine("Exporting meshes...");
                    surfs.ExportAll(group, (long progress, long total) =>
                    {
                        double totalProgress = (((double)progress / (double)total) * 100.0);
                        SynthesisGUI.Instance.ExporterSetSubText(String.Format("Export {1} / {2}", Math.Round(totalProgress, 2), progress, total));
                        SynthesisGUI.Instance.ExporterSetProgress(totalProgress);
                    });
                    Console.WriteLine();
                    BXDAMesh output = surfs.GetOutput();
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

            SynthesisGUI.Instance.ExporterStepOverall();
        }

        return meshes;
    }
}
