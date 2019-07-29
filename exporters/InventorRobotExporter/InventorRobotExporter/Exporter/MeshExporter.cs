using System;
using System.Collections.Generic;
using InventorRobotExporter.OGLViewer;
using InventorRobotExporter.RigidAnalyzer;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.Exporter
{
    public static class MeshExporter
    {
        public static List<BXDAMesh> ExportMeshes(IProgress<ProgressUpdate> progress, RigidNode_Base baseNode, float totalMassKg)
        {
            SurfaceExporter surfs = new SurfaceExporter();
            BXDJSkeleton.SetupFileNames(baseNode);

            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            baseNode.ListAllNodes(nodes);

            List<BXDAMesh> meshes = new List<BXDAMesh>();

            progress.Report(new ProgressUpdate("Exporting Model", 0, 0));

            for (int i = 0; i < nodes.Count; i++)
            {
                RigidNode_Base node = nodes[i];

                if (node is RigidNode && node.GetModel() != null && node.ModelFileName != null && node.GetModel() is CustomRigidGroup)
                {
                    try
                    {
                        CustomRigidGroup group = (CustomRigidGroup)node.GetModel();

                        BXDAMesh output = surfs.ExportAll(@group, node.GUID, (progressValue, total) =>
                        {
                            progress.Report(new ProgressUpdate(null, (int) (((double)progressValue / total / nodes.Count + (double)i / nodes.Count)*1000), 1000));
                        });
                    
                        output.colliders.Clear();
                        output.colliders.AddRange(ConvexHullCalculator.GetHull(output));

                        meshes.Add(output);
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error exporting mesh: " + node.GetModelID(), e);
                    }
                }
            }

            // Apply custom mass to mesh
            if (totalMassKg > 0) // Negative value indicates that default mass should be left alone (TODO: Make default mass more accurate)
            {
                float totalDefaultMass = 0;
                foreach (BXDAMesh mesh in meshes)
                {
                    totalDefaultMass += mesh.physics.mass;
                }
                for (int i = 0; i < meshes.Count; i++)
                {
                    meshes[i].physics.mass = totalMassKg * (float)(meshes[i].physics.mass / totalDefaultMass);
                }
            }

            // Add meshes to all nodes
            for (int i = 0; i < meshes.Count; i++)
            {
                ((OGL_RigidNode)nodes[i]).loadMeshes(meshes[i]);
            }

            // Get wheel information (radius, center, etc.) for all wheels
            foreach (RigidNode_Base node in nodes)
            {
                SkeletalJoint_Base joint = node.GetSkeletalJoint();

                // Joint will be null if the node has no connection.
                // cDriver will be null if there is no driver connected to the joint.
                if (joint != null && joint.cDriver != null)
                {
                    WheelDriverMeta wheelDriver = (WheelDriverMeta)joint.cDriver.GetInfo(typeof(WheelDriverMeta));

                    // Drivers without wheel metadata do not need radius, center, or width info.
                    if (wheelDriver != null)
                    {
                        (node as OGLViewer.OGL_RigidNode).GetWheelInfo(out float radius, out float width, out BXDVector3 center);
                        wheelDriver.radius = radius;
                        wheelDriver.center = center;
                        wheelDriver.width = width;

                        joint.cDriver.AddInfo(wheelDriver);
                    }
                }
            }

            return meshes;
        }
    }
}