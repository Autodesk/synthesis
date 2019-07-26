using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BxDRobotExporter.Exporter;
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
            if (InventorManager.Instance == null)
            {
                MessageBox.Show("Couldn't detect a running instance of Inventor.");
                return null;
            }

            if (InventorManager.Instance.ActiveDocument == null || !(InventorManager.Instance.ActiveDocument is AssemblyDocument))
            {
                MessageBox.Show("Couldn't detect an open assembly");
                return null;
            }

            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

            RigidNode_Base skeleton = null;

            try
            {
                skeleton = ExportSkeleton(progress, InventorManager.Instance.ComponentOccurrences.OfType<ComponentOccurrence>().ToList());
            }
            catch (SkeletonBuilder.EmptyAssemblyException)
            {
//                SetProgressWindowVisible(false);

                string caption = "Empty Assembly";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult r = MessageBox.Show("Assembly has no parts to export.", caption, buttons);
            }
            catch (SkeletonBuilder.InvalidJointException ex)
            {
//                SetProgressWindowVisible(false);

                string caption = "Invalid Joint";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult r = MessageBox.Show(ex.Message, caption, buttons);
            }
            catch (SkeletonBuilder.NoGroundException)
            {
//                SetProgressWindowVisible(false);

                string caption = "No Ground";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult r = MessageBox.Show("Please ground a part in your assembly to export your robot.", caption, buttons);
            }

            return skeleton;
        }

        /// <summary>
        /// The lightweight equivalent of the 'Add From Inventor' button in the <see cref="ExporterForm"/>. Used in <see cref="ExportMeshesLite(RigidNode_Base)"/>
        /// </summary>
        /// <param name="occurrences"></param>
        /// <returns></returns>
        private static RigidNode_Base ExportSkeleton(IProgress<int> progress, IReadOnlyCollection<ComponentOccurrence> occurrences)
        {
            if (occurrences.Count == 0)
            {
                throw new SkeletonBuilder.EmptyAssemblyException();
            }

            //Getting Rigid Body Info...

            progress.Report(1);
            Thread.Sleep(1000);
//            SetProgress("Getting physics info...", occurrences.Count, occurrences.Count + 3);
            NameValueMap rigidGetOptions = InventorManager.Instance.TransientObjects.CreateNameValueMap();

            rigidGetOptions.Add("DoubleBearing", false);
            RigidBodyResults rawRigidResults = InventorManager.Instance.AssemblyDocument.ComponentDefinition.RigidBodyAnalysis(rigidGetOptions);

            //Getting Rigid Body Info...Done
            CustomRigidResults rigidResults = new CustomRigidResults(rawRigidResults);


            //Building Model...
            progress.Report(4);
            Thread.Sleep(1000);

//            SetProgress("Building model...", occurrences.Count + 1, occurrences.Count + 3);
            RigidBodyCleaner.CleanGroundedBodies(rigidResults);
            RigidNode baseNode = RigidBodyCleaner.BuildAndCleanDijkstra(rigidResults);

            //Building Model...Done

            //Cleaning Up...
            progress.Report(40);
            Thread.Sleep(1000);

//            SetProgress("Cleaning up...", occurrences.Count + 2, occurrences.Count + 3);
            List<RigidNode_Base> nodes = new List<RigidNode_Base>();
            baseNode.ListAllNodes(nodes);

            foreach (RigidNode_Base node in nodes)
            {
                node.ModelFileName = ((RigidNode)node).@group.ToString();
                node.ModelFullID = node.GetModelID();
            }
            //Cleaning Up...Done
            progress.Report(100);
            Thread.Sleep(1000);

//            SetProgress("Done", occurrences.Count + 3, occurrences.Count + 3);
            return baseNode;
        }
    }
}
