using System;
using System.Collections.Generic;
using Inventor;
using System.IO;
using MIConvexHull;

static class Program
{

    public static Application INVENTOR_APPLICATION;
    private const int MAX_VERTICIES = 8192;
    public static unsafe void Main(String[] args)
    {
        INVENTOR_APPLICATION = (Application) System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
        //AnalyzeRigidResults();
        AssemblyDocument doc = (AssemblyDocument) INVENTOR_APPLICATION.ActiveDocument;
        SurfaceExporter exp = new SurfaceExporter();
        foreach (ComponentOccurrence b in doc.ComponentDefinition.Occurrences)
        {
            exp.ExportAll(b);
        }
        BXDAMesh mesh = exp.GetOutput();

        BXDAMesh.BXDASubMesh hull = ConvexHullCalculator.GetHull(mesh);
    }

    public static Matrix GetWorldTransformation(ComponentOccurrence comp)
    {
        Matrix trans = INVENTOR_APPLICATION.TransientGeometry.CreateMatrix();
        trans.SetToIdentity();
        if (!((comp.ParentOccurrence == null)))
        {
            trans.TransformBy(GetWorldTransformation(comp.ParentOccurrence));
        }
        trans.TransformBy(comp.Transformation);
        return trans;
    }

    public static void AnalyzeRigidResults()
    {
        string homePath = (System.Environment.OSVersion.Platform == PlatformID.Unix || System.Environment.OSVersion.Platform == PlatformID.MacOSX) ? System.Environment.GetEnvironmentVariable("HOME") : System.Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");
        string pathBase = homePath + "\\Downloads\\Skeleton";
        Directory.CreateDirectory(pathBase);

        AssemblyDocument asmDoc = (AssemblyDocument) INVENTOR_APPLICATION.ActiveDocument;
        Console.WriteLine("Get rigid info...");

        NameValueMap options = INVENTOR_APPLICATION.TransientObjects.CreateNameValueMap();
        //options.Add("SuperfluousDOF", true);
        options.Add("DoubleBearing", false);
        RigidBodyResults rigidResults = asmDoc.ComponentDefinition.RigidBodyAnalysis(options);

        Console.WriteLine("Got rigid info...");
        CustomRigidResults customRigid = new CustomRigidResults(rigidResults);

        Console.WriteLine("Built model...");
        RigidBodyCleaner.CleanGroundedBodies(customRigid);

        RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(customRigid);
        Console.WriteLine("Built");

        Console.WriteLine(baseNode.ToString());

        // Merge with existing values
        if (System.IO.File.Exists(pathBase + "\\skeleton.bxdj"))
        {
            try
            {
                RigidNode_Base loadedBase = BXDJSkeleton.ReadSkeleton(pathBase + "\\skeleton.bxdj");
                JointDriver.CloneDriversFromTo(loadedBase, baseNode);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error loading existing skeleton: " + e.ToString());
            }
        }

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        ControlGroups controlGUI = new ControlGroups();
        controlGUI.SetNodeList(nodes);
        controlGUI.SetGroupList(customRigid.groups);
        controlGUI.ShowDialog();
        controlGUI.Cleanup();
        Console.WriteLine("Form exit with code " + Enum.GetName(typeof(FormState), controlGUI.formState));
        if (controlGUI.formState == FormState.SUBMIT)
        {
            SurfaceExporter surfs = new SurfaceExporter();
            Dictionary<RigidNode_Base, string> bxdaOutputPath;
            BXDJSkeleton.WriteSkeleton(pathBase + "\\skeleton.bxdj", baseNode, out bxdaOutputPath);
            foreach (KeyValuePair<RigidNode_Base, string> output in bxdaOutputPath)
            {
                if (output.Key != null && output.Key.GetModel() != null && output.Key.GetModel() is CustomRigidGroup)
                {
                    CustomRigidGroup group = (CustomRigidGroup) output.Key.GetModel();
                    Console.WriteLine("Output " + group.ToString() + " to " + output.Value);
                    surfs.Reset();
                    surfs.ExportAll(group);
                    surfs.GetOutput().WriteBXDA(output.Value);
                }
            }
        }
    }
}