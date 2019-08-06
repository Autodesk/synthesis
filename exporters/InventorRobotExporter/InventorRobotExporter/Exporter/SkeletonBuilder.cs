using System;
using System.Collections.Generic;
using System.Linq;
using InventorRobotExporter.RigidAnalyzer;
using InventorRobotExporter.Utilities;
using Inventor;

namespace InventorRobotExporter.Exporter.Skeleton
{
    public static class SkeletonBuilder
    {
        public class NoGroundException : ApplicationException
        {
            public NoGroundException() : base("Assembly has no ground.") { }
        }

        public static RigidNode_Base ExportSkeleton(IProgress<ProgressUpdate> progress)
        {
            // If no components
            if (RobotExporterAddInServer.Instance.OpenAssemblyDocument.ComponentDefinition.Occurrences.OfType<ComponentOccurrence>().ToList().Count == 0)
                return null;
            
            //Getting Rigid Body Info...
            progress?.Report(new ProgressUpdate("Getting physics info...", 1, 4));
            var rigidGetOptions = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateNameValueMap();
            rigidGetOptions.Add("DoubleBearing", false);
            var asmDocument = RobotExporterAddInServer.Instance.OpenAssemblyDocument;
            var rawRigidResults = asmDocument.ComponentDefinition.RigidBodyAnalysis(rigidGetOptions);
            var rigidResults = new CustomRigidResults(rawRigidResults);

            //Building Model...
            progress?.Report(new ProgressUpdate("Building model...", 2, 4));

            try
            {
                RigidBodyCleaner.CleanGroundedBodies(rigidResults);
            }
            catch (NoGroundException)
            {
                WinFormsUtils.ShowErrorDialog("Please ground a part in your assembly to export your robot.", "No Ground");
                RobotExporterAddInServer.Instance.RobotDataManager.hasGround = false;
                return null;
            }

            var skeletonBase = RigidBodyCleaner.BuildAndCleanDijkstra(rigidResults);

            //Cleaning Up...
            progress?.Report(new ProgressUpdate("Cleaning up...", 3, 4));
            var nodes = new List<RigidNode_Base>();
            skeletonBase.ListAllNodes(nodes);
            foreach (var node in nodes)
            {
                node.ModelFileName = ((RigidNode)node).group.ToString();
                node.ModelFullID = node.GetModelID();
            }
            
            progress?.Report(new ProgressUpdate("Done", 4, 4));
            return skeletonBase;
        }
    }
}
