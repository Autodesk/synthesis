using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorRobotExporter.Exporter.Skeleton;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Loading
{
    public partial class LoadingBar : Form
    {
        public LoadingBar()
        {
            InitializeComponent();
            Shown += (sender, args) => RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
            FormClosing += (sender, args) => RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
        }
        
        public void SetProgress(ProgressUpdate update)
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
