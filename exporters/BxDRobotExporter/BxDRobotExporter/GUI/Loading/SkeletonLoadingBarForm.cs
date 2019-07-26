using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BxDRobotExporter.Skeleton.SkeletonBuilder;

namespace BxDRobotExporter.GUI.Loading
{
    public partial class SkeletonLoadingBarForm : Form
    {
        private RigidNode_Base rigidNodeBase;

        public SkeletonLoadingBarForm()
        {
            InitializeComponent();

            Shown += async (sender, args) =>
            {
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
                rigidNodeBase = await Task.Run(() => ExportSkeleton(new Progress<SkeletonProgressUpdate>(SetProgress)));
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
                Close();
            };
        }
        
        public async Task<RigidNode_Base> BuildSkeleton()
        {
            await Task.Run(ShowDialog).ConfigureAwait(false);
            return rigidNodeBase;
        }

        private void SetProgress(SkeletonProgressUpdate update)
        {
            // Allows function to be called by other threads TODO: is this neccecary?
            if (InvokeRequired)
            {
                BeginInvoke((Action<SkeletonProgressUpdate>)SetProgress, update);
                return;
            }

            ProgressLabel.Text = update.Message;
            ProgressBar.Maximum = update.MaxProgress;
            ProgressBar.Value = update.CurrentProgress;
        }
    }
}
