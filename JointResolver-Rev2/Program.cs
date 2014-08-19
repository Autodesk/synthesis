using System;
using System.Collections.Generic;
using Inventor;
using System.IO;
using System.Threading;
using System.Windows.Forms;

static class Program
{
    public static Inventor.Application INVENTOR_APPLICATION;
    private const int MAX_VERTICIES = 8192;
    public static void Main(String[] args)
    {
        INVENTOR_APPLICATION = (Inventor.Application) System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
        //_2014FieldBounding.WriteModel();
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

    public static void CenterAllJoints(ComponentOccurrence component)
    {
        foreach (AssemblyJoint joint in component.Joints)
        {
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
                    throw new Exception("Joints with linear motion require limits.");
                }
            }
        }

        foreach(ComponentOccurrence subComponent in component.SubOccurrences)
        {
            CenterAllJoints(subComponent);
        }
    }

    public static void AnalyzeRigidResults()
    {
        AssemblyDocument asmDoc = (AssemblyDocument) INVENTOR_APPLICATION.ActiveDocument;  


        Console.WriteLine("Get rigid info...");

        foreach (ComponentOccurrence component in asmDoc.ComponentDefinition.Occurrences)
        {
            CenterAllJoints(component);            
        }
        Console.WriteLine("All joints centered");

        NameValueMap options = INVENTOR_APPLICATION.TransientObjects.CreateNameValueMap();
        //options.Add("SuperfluousDOF", true);
        options.Add("DoubleBearing", false);
        RigidBodyResults rigidResults = asmDoc.ComponentDefinition.RigidBodyAnalysis(options);

        Console.WriteLine("Got rigid info...");
        CustomRigidResults customRigid = new CustomRigidResults(rigidResults);
        //After this point, all grounded groups have been merged into one CustomRigidGroup, and their joints have been updated.

        Console.WriteLine("Built model...");
        RigidBodyCleaner.CleanGroundedBodies(customRigid);

        RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(customRigid);
        Console.WriteLine("Built");

        Console.WriteLine(baseNode.ToString());

        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        baseNode.ListAllNodes(nodes);

        ControlGroups controlGUI = new ControlGroups();
        foreach (RigidNode_Base node in nodes)
        {
            node.modelFileName = ((RigidNode) node).group.ToString();
        }
        controlGUI.SetSkeleton(baseNode);
        controlGUI.SetGroupList(customRigid.groups);
        controlGUI.ShowDialog();
        controlGUI.Cleanup();
        Console.WriteLine("Form exit with code " + Enum.GetName(typeof(FormState), controlGUI.formState));
        if (controlGUI.formState == FormState.SUBMIT)
        {
            string pathBase = controlGUI.ExportPath;

            SurfaceExporter surfs = new SurfaceExporter();
            {
                BXDJSkeleton.SetupFileNames(baseNode, true);
                foreach (RigidNode_Base node in nodes)
                {
                    if (node is RigidNode && node.GetModel() != null && node.modelFileName != null && node.GetModel() is CustomRigidGroup)
                    {
                        Console.WriteLine("Running deffered calculations for " + node.GetModelID());
                        ((RigidNode) node).DoDeferredCalculations();
                        try
                        {
                            Console.WriteLine("Exporting " + node.modelFileName);
                            CustomRigidGroup group = (CustomRigidGroup) node.GetModel();
                            surfs.Reset();
                            surfs.ExportAll(group, (long progress, long total) =>
                            {
                                Console.Write(Math.Round((progress / (float) total) * 100.0f, 2) + "%\t" + progress + " / " + total);
                                Console.CursorLeft = 0;
                            });
                            Console.WriteLine();
                            BXDAMesh output = surfs.GetOutput();
                            Console.WriteLine("Output mesh: " + output.meshes.Count + " meshes");
                            /*Console.WriteLine("Exporting for Colliders\n");
                            surfs.Reset();
                            surfs.ExportAll(group, true);*/
                            Console.WriteLine("Computing colliders for " + node.GetModelID());
                            output.colliders.Clear();
                            output.colliders.AddRange(ConvexHullCalculator.GetHull(output, !group.convex));
                            output.WriteToFile(pathBase + "\\" + node.modelFileName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                            System.Windows.Forms.MessageBox.Show("Error exporting mesh: " + node.GetModelID());
                        }
                    }
                }

                Console.WriteLine("Writing skeleton");
                BXDJSkeleton.WriteSkeleton(pathBase + "\\skeleton.bxdj", baseNode);

                BXDSettings.Instance.LastSkeletonDirectory = pathBase;
                BXDSettings.Save();
                MessageBox.Show("Finished Exporting Files!");
            }
        }
    }
}