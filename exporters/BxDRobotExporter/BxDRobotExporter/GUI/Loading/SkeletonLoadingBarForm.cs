using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.Managers;
using static BxDRobotExporter.Skeleton.SkeletonBuilder;

namespace BxDRobotExporter.GUI.Loading
{
    public partial class SkeletonLoadingBarForm : Form
    {
        public SkeletonLoadingBarForm(RobotDataManager robotDataManager)
        {
            InitializeComponent();

            Shown += async (sender, args) =>
            {
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
                await Task.Run(() => robotDataManager.RobotBaseNode = ExporterWorker_DoWork(new Progress<SkeletonProgressUpdate>(SetProgress)));
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
                Close();
            };
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
