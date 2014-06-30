using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Inventor;
using System.IO;

static class Program
{
    public static Application invApplication;
    private const int MAX_VERTICIES = 8192;
    public static void Main(String[] args)
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
        AssemblyDocument asmDoc = (AssemblyDocument) invApplication.ActiveDocument;
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

        // Build the file structure
        //Dim pathBase As String = "C:\Users\t_millw\Downloads\skele\"
        //Dim surfs As New SurfaceExporter
        //Dim writer As StreamWriter = New StreamWriter(pathBase + "meta.txt")
        //For i As Integer = 0 To nodes.Count - 1
        //    writer.Write(Str(i) & ":")
        //    Dim path As String = nodes.Item(i).group.occurrences.Item(0).Name.Replace(":", "_") & ".stl"
        //    writer.Write(path & ":")
        //    surfs.Reset()
        //    surfs.ExportAll(nodes.Item(i).group.occurrences)
        //    surfs.WriteSTL(pathBase & path)
        //    If IsNothing(nodes.Item(i).parent) Then
        //        writer.Write("-1,")
        //    Else
        //        writer.Write(Str(nodes.IndexOf(nodes.Item(i).parent)) & ":")
        //        Dim joint As CustomRigidJoint = nodes.Item(i).parentConnection
        //        If RotationalJoint.isRotationalJoint(joint) Then
        //            writer.Write(RotationalJoint.getInfo(joint, nodes.Item(i).parent.group))
        //        End If
        //    End If
        //    writer.WriteLine()
        //Next i
        //writer.Close()
        ControlGroups controlGUI = new ControlGroups();
        controlGUI.setNodeList(nodes);
        controlGUI.ShowDialog();
        controlGUI.Cleanup();
        Console.WriteLine("Form exit with code " + Enum.GetName(typeof(FormState), controlGUI.formState));
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