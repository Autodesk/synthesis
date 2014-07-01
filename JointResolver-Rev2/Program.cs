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
        Console.WriteLine("Cleaned grounded; " + (customRigid.groups.Count) + " groups and " + (customRigid.joints.Count) + " joints remain.");
        RigidBodyCleaner.CleanConstraintOnly(customRigid);
        Console.WriteLine("Clean constraints 1; " + (customRigid.groups.Count) + " groups and " + (customRigid.joints.Count) + " joints remain.");
        RigidBodyCleaner.CleanConstraintOnly(customRigid);
        // Repeat gives a better join
        Console.WriteLine("Clean constraints 2; " + (customRigid.groups.Count) + " groups and " + (customRigid.joints.Count) + " joints remain.");

        foreach (CustomRigidGroup group in customRigid.groups)
        {
            Console.WriteLine("RigidGroup " + group.ToString());
        }

        Console.WriteLine("");
        Console.WriteLine((customRigid.joints.Count) + " joints remain");

        RigidNode baseNode = RigidNode.generateNodeTree(customRigid);

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
            StreamWriter writer = new StreamWriter(pathBase + "meta.txt");
            for (int i = 0; i < nodes.Count; i++)
            {
                writer.Write(i + ":");
                string path = nodes[i].group.occurrences[0].Name.Replace(":", "_") + ".bxda";
                writer.Write(path + ":");
                surfs.Reset();
                surfs.ExportAll(nodes[i].group.occurrences);
                surfs.WriteBXDA(pathBase + path);
                if (nodes[i].parent == null)
                {
                    writer.Write("-1,");
                }
                else
                {
                    writer.Write(nodes.IndexOf(nodes[i].parent) + ":");
                    CustomRigidJoint joint = nodes[i].parentConnection;
                    if (RotationalJoint.isRotationalJoint(joint))
                    {
                        writer.Write(new RotationalJoint(nodes[i].parent.group, joint));
                    }
                }
                writer.WriteLine();
            }
            writer.Close();
        }
    }

    public static string printVector(object pO)
    {
        if (pO is Vector)
        {
            Vector p = (Vector)pO;
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