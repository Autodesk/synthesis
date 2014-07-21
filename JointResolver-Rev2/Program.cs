using System;
using System.Collections.Generic;
using Inventor;
using System.IO;

static class Program
{
    public static Application INVENTOR_APPLICATION;
    private const int MAX_VERTICIES = 8192;
    public static unsafe void Main(String[] args)
    {
        INVENTOR_APPLICATION = (Application) System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
        AnalyzeRigidResults();
        //AssemblyDocument asmDoc = (AssemblyDocument) INVENTOR_APPLICATION.ActiveDocument;
        //SurfaceExporter exp = new SurfaceExporter();
        //foreach (ComponentOccurrence cc in asmDoc.ComponentDefinition.Occurrences){
        //    exp.ExportAll(cc);
        //}
        //List<BXDAMesh.BXDASubMesh> subs = ConvexHullCalculator.GetHull(exp.GetOutput());
        //BXDAMesh mesh = new BXDAMesh();
        //mesh.meshes.AddRange(subs);
        //mesh.WriteBXDA("C:/Temp/test.bxda");
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
            {
                BXDJSkeleton.SetupFileNames(baseNode);
                foreach (RigidNode_Base node in nodes)
                {
                    if (node is RigidNode && node.GetModel() != null && node.GetModelFileName() != null && node.GetModel() is CustomRigidGroup)
                    {
                        Console.WriteLine("Running deffered calculations for " + node.GetModelID());
                        ((RigidNode) node).DoDeferredCalculations();
                        Console.WriteLine("Exporting " + node.GetModelID());
                        CustomRigidGroup group = (CustomRigidGroup) node.GetModel();
                        surfs.Reset();
                        surfs.ExportAll(group);
                        BXDAMesh output = surfs.GetOutput();
                        Console.WriteLine("Computing colliders for " + node.GetModelID());
                        output.colliders.Clear();
                        output.colliders.AddRange(ConvexHullCalculator.GetHull(output, !group.convex));
                        output.WriteBXDA(pathBase + "\\" + node.GetModelFileName());
                    }
                }
            }
            Console.WriteLine("Writing skeleton");
            BXDJSkeleton.WriteSkeleton(pathBase + "\\skeleton.bxdj", baseNode);
        }
    }
}