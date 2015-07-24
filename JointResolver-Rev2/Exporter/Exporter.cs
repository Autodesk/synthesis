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

    public static Inventor.Application INVENTOR_APPLICATION;

    public static void LoadInventorInstance()
    {
        if (INVENTOR_APPLICATION != null) return;

        try
        {
            INVENTOR_APPLICATION = (Inventor.Application)Marshal.GetActiveObject("Inventor.Application");
        }
        catch (COMException e)
        {
            Console.WriteLine(e);
            throw new Exception("Could not get a running instance of Inventor");
        }
    }

    public static void ReleaseInventorInstance()
    {
        if (INVENTOR_APPLICATION == null) return;

        try
        {
            Marshal.FinalReleaseComObject(INVENTOR_APPLICATION);
            INVENTOR_APPLICATION = null;
        }
        catch (COMException e)
        {
            Console.WriteLine(e);
            throw new Exception("Could not release Inventor instance");
        }
    }

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

        //Contiues down to subassemblies.
        foreach (ComponentOccurrence subComponent in component.SubOccurrences)
        {
            CenterAllJoints(subComponent);
        }
    }

    public static RigidNode_Base ExportSkeleton(List<ComponentOccurrence> occurrences)
    {
        if (occurrences == null) throw new Exception("No components selected!");

        AssemblyDocument asmDoc = (AssemblyDocument)INVENTOR_APPLICATION.ActiveDocument;

        //Centers all the joints for each component.  Done to match the assembly's joint position with the subassembly's position.
        foreach (ComponentOccurrence component in occurrences)
        {
            CenterAllJoints(component);
        }
        Console.WriteLine();

        Console.WriteLine("Get rigid info...");
        //Group components into rigid bodies.
        NameValueMap options = INVENTOR_APPLICATION.TransientObjects.CreateNameValueMap();
        options.Add("DoubleBearing", false);
        RigidBodyResults rigidResults = asmDoc.ComponentDefinition.RigidBodyAnalysis(options);

        Console.WriteLine("Got rigid info...");
        CustomRigidResults customRigid = new CustomRigidResults(rigidResults);

        Console.WriteLine("Built model...");
        RigidBodyCleaner.CleanGroundedBodies(customRigid);
        //After this point, all grounded groups have been merged into one CustomRigidGroup, and their joints have been updated.

        RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(customRigid);
        Console.WriteLine("Built");

        Console.WriteLine(baseNode.ToString());

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        foreach (RigidNode_Base node in nodes)
        {
            node.modelFileName = ((RigidNode)node).group.ToString();
            node.modelFullID = node.GetModelID();
        }

        return baseNode;
    }

    public static List<BXDAMesh> ExportMeshes(RigidNode_Base baseNode, bool highRes = false, bool colorFaces = true)
    {
        SurfaceExporter surfs = new SurfaceExporter();
        BXDJSkeleton.SetupFileNames(baseNode, true);

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        List<BXDAMesh> meshes = new List<BXDAMesh>();
        foreach (RigidNode_Base node in nodes)
        {
            if (node is RigidNode && node.GetModel() != null && node.modelFileName != null && node.GetModel() is CustomRigidGroup)
            {
                Console.WriteLine("Exporting " + node.modelFileName);
                ((RigidNode)node).DoDeferredCalculations();

                try
                {
                    SynthesisGUI.Instance.ExporterReset();
                    CustomRigidGroup group = (CustomRigidGroup)node.GetModel();
                    group.highRes = highRes;
                    group.colorFaces = colorFaces;
                    surfs.Reset();
                    Console.WriteLine("Exporting meshes...");
                    surfs.ExportAll(group, (long progress, long total) =>
                    {
                        double totalProgress = (((double)progress / (double)total) * 100.0);
                        SynthesisGUI.Instance.ExporterSetSubText(String.Format("{0}% \t {1} / {2}", Math.Round(totalProgress, 2), progress, total));
                        SynthesisGUI.Instance.ExporterSetProgress(totalProgress);
                    });
                    Console.WriteLine();
                    BXDAMesh output = surfs.GetOutput();
                    Console.WriteLine("Output: " + output.meshes.Count + " meshes");
                    Console.WriteLine("Computing colliders...");
                    output.colliders.Clear();
                    output.colliders.AddRange(ConvexHullCalculator.GetHull(output, !group.convex));

                    meshes.Add(output);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    throw new Exception("Error exporting mesh: " + node.GetModelID());
                }
            }
        }

        return meshes;
    }

}
