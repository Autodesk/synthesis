using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;
using System.IO;
using System.Runtime.InteropServices;

static class Program
{

    public static Application invApplication;
    private const int MAX_VERTICIES = 8192;
    public static unsafe void Main(String[] args)
    {
        invApplication = (Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
        AnalyzeRigidResults();
    }

    public static Matrix WorldTransformation(ComponentOccurrence comp)
    {
        Matrix trans = invApplication.TransientGeometry.CreateMatrix();
        trans.SetToIdentity();
        if (!((comp.ParentOccurrence == null)))
        {
            trans.TransformBy(WorldTransformation(comp.ParentOccurrence));
        }
        trans.TransformBy(comp.Transformation);
        return trans;
    }

    public static void AnalyzeRigidResults()
    {
        AssemblyDocument asmDoc = (AssemblyDocument)invApplication.ActiveDocument;
        Console.WriteLine("Get rigid info...");
        RigidBodyResults rigidResults = asmDoc.ComponentDefinition.RigidBodyAnalysis(invApplication.TransientObjects.CreateNameValueMap());
        Console.WriteLine("Got rigid info...");
        CustomRigidResults customRigid = new CustomRigidResults(rigidResults);

        Console.WriteLine("Built model...");
        RigidBodyCleaner.CleanGroundedBodies(customRigid);

        RigidNode baseNode = RigidBodyCleaner.buildAndCleanDijkstra(customRigid);
        Console.WriteLine("Built");

        Console.WriteLine(baseNode.ToString());
        List<RigidNode> nodes = new List<RigidNode>();
        baseNode.listAllNodes(nodes);

        ControlGroups controlGUI = new ControlGroups();
        controlGUI.setNodeList(nodes);
        controlGUI.setGroupList(customRigid.groups);
        controlGUI.ShowDialog();
        controlGUI.Cleanup();
        Console.WriteLine("Form exit with code " + Enum.GetName(typeof(FormState), controlGUI.formState));
        if (controlGUI.formState == FormState.SUBMIT)
        {
            string pathBase = "C:/Users/t_millw/Downloads/skele/";
            SurfaceExporter surfs = new SurfaceExporter();
            Dictionary<CustomRigidGroup, string> bxdaOutputPath;
            SkeletalIO.write(pathBase + "skeleton.bxdj", nodes, out bxdaOutputPath);
            foreach (KeyValuePair<CustomRigidGroup, string> output in bxdaOutputPath)
            {
                Console.WriteLine("Output " + output.Key.ToString() + " to " + output.Value);
                surfs.Reset();
                surfs.ExportAll(output.Key);
                surfs.WriteBXDA(output.Value);
                if (surfs.vertCount > 65000)
                {
                    System.Windows.Forms.MessageBox.Show("Warning: " + output.Key.ToString() + " exceededed 65000 verticies.  Strange things may begin to happen.");
                }
            }
        }
    }

    public static string printVector(object pO)
    {
        if (pO is Vector)
        {
            Vector p = (Vector)pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        else if (pO is UnitVector)
        {
            UnitVector p = (UnitVector)pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        else if (pO is Point)
        {
            Point p = (Point)pO;
            return (p.X + "," + p.Y + "," + p.Z);
        }
        return "";
    }
}