using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.Managers;
using BxDRobotExporter.Skeleton;

namespace BxDRobotExporter.GUI.Loading
{
    public partial class SkeletonLoadingBarForm : Form
    {
        public SkeletonLoadingBarForm(RobotDataManager robotDataManager)
        {
            InitializeComponent();

            FormClosing += (sender, args) => RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;

            var progress = new Progress<int>(v =>
            {
                SetProgress("Loading", v, 100);
            }); 
            
            Shown += async (sender, args) =>
            {
                await Task.Run(() => robotDataManager.RobotBaseNode = SkeletonBuilder.ExporterWorker_DoWork(progress));
                Close();
            };
        }

        private void SetProgress(string message, int current, int max)
        {
            // Allows function to be called by other threads TODO: is this neccecary?
            if (InvokeRequired)
            {
                BeginInvoke((Action<string, int, int>)SetProgress, message, current, max);
                return;
            }

            ProgressLabel.Text = message;
            ProgressBar.Maximum = max;
            ProgressBar.Value = current;
        }
    }
}
