using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.Exporter.Skeleton;
using BxDRobotExporter.Utilities;

namespace BxDRobotExporter.GUI.Loading
{
    public partial class BuildingSkeletonForm : Form
    {
        private RigidNode_Base rigidNodeBase;

        public BuildingSkeletonForm()
        {
            InitializeComponent();

            Shown += async (sender, args) =>
            {
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
                rigidNodeBase = await Task.Run(() => SkeletonBuilder.ExportSkeleton(new Progress<ProgressUpdate>(SetProgress)));
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
                Close();
            };
        }
        
        public async Task<RigidNode_Base> BuildSkeleton()
        {
            await Task.Run(ShowDialog).ConfigureAwait(false);
            return rigidNodeBase;
        }

        private void SetProgress(ProgressUpdate update)
        {
            // Allows function to be called by other threads TODO: is this neccecary?
            if (InvokeRequired)
            {
                BeginInvoke((Action<ProgressUpdate>)SetProgress, update);
                return;
            }

            ProgressLabel.Text = update.Message;
            ProgressBar.Maximum = update.MaxProgress;
            ProgressBar.Value = update.CurrentProgress;
        }
    }
}
