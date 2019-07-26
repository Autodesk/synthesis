using System;
using System.Collections.Generic;
using System.Linq;
using BxDRobotExporter.RigidAnalyzer;
using BxDRobotExporter.Utilities;
using Inventor;

namespace BxDRobotExporter.Skeleton
{
    public static class SkeletonBuilder
    {
        public class NoGroundException : ApplicationException
        {
            public NoGroundException() : base("Assembly has no ground.") { }
        }

        public class SkeletonProgressUpdate
        {
            public readonly string Message;
            public readonly int CurrentProgress;
            public readonly int MaxProgress;

            public SkeletonProgressUpdate(string message, int currentProgress, int maxProgress)
            {
                Message = message;
                CurrentProgress = currentProgress;
                MaxProgress = maxProgress;
            }
        }

        public static RigidNode_Base ExportSkeleton(IProgress<SkeletonProgressUpdate> progress)
        {
            // If no components
            if (RobotExporterAddInServer.Instance.OpenAssemblyDocument.ComponentDefinition.Occurrences.OfType<ComponentOccurrence>().ToList().Count == 0)
                return null;
            
            //Getting Rigid Body Info...
            progress.Report(new SkeletonProgressUpdate("Getting physics info...", 1, 4));
            var rigidGetOptions = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateNameValueMap();
            rigidGetOptions.Add("DoubleBearing", false);
            var asmDocument = RobotExporterAddInServer.Instance.OpenAssemblyDocument;
            var rawRigidResults = asmDocument.ComponentDefinition.RigidBodyAnalysis(rigidGetOptions);
            var rigidResults = new CustomRigidResults(rawRigidResults);

            //Building Model...
            progress.Report(new SkeletonProgressUpdate("Building model...", 2, 4));
            try
            {
                RigidBodyCleaner.CleanGroundedBodies(rigidResults);
            }
            catch (NoGroundException)
            {
                WinFormsUtils.ShowErrorDialog("Please ground a part in your assembly to export your robot.", "No Ground");
                return null;
            }
            var skeletonBase = RigidBodyCleaner.BuildAndCleanDijkstra(rigidResults);

            //Cleaning Up...
            progress.Report(new SkeletonProgressUpdate("Cleaning up...", 3, 4));
            var nodes = new List<RigidNode_Base>();
            skeletonBase.ListAllNodes(nodes);
            foreach (var node in nodes)
            {
                node.ModelFileName = ((RigidNode)node).group.ToString();
                node.ModelFullID = node.GetModelID();
            }
            
            progress.Report(new SkeletonProgressUpdate("Done", 4, 4));
            return skeletonBase;
        }
    }
}
