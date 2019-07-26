using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorRobotExporter.Exporter.Skeleton;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Loading
{
    public partial class BuildingSkeletonForm : Form
    {
        private RigidNode_Base rigidNodeBase;

        public BuildingSkeletonForm()
        {
            InitializeComponent();
            BuildSkeletonWorker.DoWork += BuildSkeletonWorker_DoWork;
            BuildSkeletonWorker.RunWorkerCompleted += BuildSkeletonWorker_RunWorkerCompleted;

            Shown += (sender, args) =>
            {
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
                BuildSkeletonWorker.RunWorkerAsync();
            };
            
            FormClosing += (sender, args) => RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
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
        
        private void BuildSkeletonWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            rigidNodeBase = SkeletonBuilder.ExportSkeleton(new Progress<ProgressUpdate>(SetProgress));
        }

        private void BuildSkeletonWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                ProgressLabel.Text = "Skeleton Loading Cancelled";
            else if (e.Error != null)
            {
                ProgressLabel.Text = "An error occurred.";
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
