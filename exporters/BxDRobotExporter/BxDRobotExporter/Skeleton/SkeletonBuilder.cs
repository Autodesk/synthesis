using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BxDRobotExporter.RigidAnalyzer;
using Inventor;

namespace BxDRobotExporter.Skeleton
{
    public class SkeletonBuilder
    {
        public class EmptyAssemblyException : ApplicationException
        {
            public EmptyAssemblyException() : base("No parts in assembly.") { }
        }

        public class InvalidJointException : ApplicationException
        {
            public InvalidJointException(string message) : base(message) { }
        }

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

        public static RigidNode_Base ExporterWorker_DoWork(IProgress<SkeletonProgressUpdate> progress)
        {
            if (RobotExporterAddInServer.Instance.OpenDocument == null || !(RobotExporterAddInServer.Instance.OpenDocument is AssemblyDocument))
            {
                MessageBox.Show("Couldn't detect an open assembly");
                return null;
            }

            var asmDocument = RobotExporterAddInServer.Instance.OpenDocument as AssemblyDocument;
            if (asmDocument.ComponentDefinition.Occurrences.OfType<ComponentOccurrence>().ToList().Count == 0)
            {
                MessageBox.Show("Cannot build skeleton from empty assembly");
                return null;
            }


            RigidNode_Base skeletonBase = null;

            try
            {
                skeletonBase = ExportSkeleton(progress);
            }
            catch (EmptyAssemblyException)
            {
                MessageBox.Show("Assembly has no parts to export.", "Empty Assembly", MessageBoxButtons.OK);
            }
            catch (InvalidJointException ex)
            {
                MessageBox.Show(ex.Message, "Invalid Joint", MessageBoxButtons.OK);
            }
            catch (NoGroundException)
            {
                MessageBox.Show("Please ground a part in your assembly to export your robot.", "No Ground", MessageBoxButtons.OK);
            }

            return skeletonBase;
        }

        private static RigidNode_Base ExportSkeleton(IProgress<SkeletonProgressUpdate> progress)
        {
            //Getting Rigid Body Info...
            progress.Report(new SkeletonProgressUpdate("Getting physics info...", 1, 4));
            var rigidGetOptions = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateNameValueMap();
            rigidGetOptions.Add("DoubleBearing", false);
            var asmDocument = RobotExporterAddInServer.Instance.OpenDocument as AssemblyDocument;
            var rawRigidResults = asmDocument.ComponentDefinition.RigidBodyAnalysis(rigidGetOptions);
            var rigidResults = new CustomRigidResults(rawRigidResults);

            //Building Model...
            progress.Report(new SkeletonProgressUpdate("Building model...", 2, 4));
            RigidBodyCleaner.CleanGroundedBodies(rigidResults);
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
