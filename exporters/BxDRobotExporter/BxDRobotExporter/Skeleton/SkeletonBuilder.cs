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

        public static RigidNode_Base ExporterWorker_DoWork(IProgress<int> progress)
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

            RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;

            RigidNode_Base skeleton = null;

            try
            {
                skeleton = ExportSkeleton(progress);
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

            return skeleton;
        }

        /// <summary>
        /// The lightweight equivalent of the 'Add From Inventor' button in the <see cref="ExporterForm"/>. Used in <see cref="ExportMeshesLite(RigidNode_Base)"/>
        /// </summary>
        /// <param name="occurrences"></param>
        /// <returns></returns>
        private static RigidNode_Base ExportSkeleton(IProgress<int> progress)
        {
            //Getting Rigid Body Info...

            progress.Report(1);
//            SetProgress("Getting physics info...", occurrences.Count, occurrences.Count + 3);
            var rigidGetOptions = RobotExporterAddInServer.Instance.Application.TransientObjects.CreateNameValueMap();

            rigidGetOptions.Add("DoubleBearing", false);
            var asmDocument = RobotExporterAddInServer.Instance.OpenDocument as AssemblyDocument;
            var rawRigidResults = asmDocument.ComponentDefinition.RigidBodyAnalysis(rigidGetOptions);

            //Getting Rigid Body Info...Done
            var rigidResults = new CustomRigidResults(rawRigidResults);


            //Building Model...
            progress.Report(4);
//            SetProgress("Building model...", occurrences.Count + 1, occurrences.Count + 3);
            RigidBodyCleaner.CleanGroundedBodies(rigidResults);
            var baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(rigidResults);

            //Building Model...Done

            //Cleaning Up...
            progress.Report(40);
//            SetProgress("Cleaning up...", occurrences.Count + 2, occurrences.Count + 3);
            var nodes = new List<RigidNode_Base>();
            baseNode.ListAllNodes(nodes);

            foreach (var node in nodes)
            {
                node.ModelFileName = ((RigidNode)node).@group.ToString();
                node.ModelFullID = node.GetModelID();
            }
            //Cleaning Up...Done
            progress.Report(100);
//            SetProgress("Done", occurrences.Count + 3, occurrences.Count + 3);
            return baseNode;
        }
    }
}
