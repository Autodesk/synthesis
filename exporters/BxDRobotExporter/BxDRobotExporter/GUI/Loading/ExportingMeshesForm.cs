using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using BxDRobotExporter.Exporter.Mesh;
using BxDRobotExporter.Managers;
using BxDRobotExporter.Utilities;

namespace BxDRobotExporter.GUI.Loading
{
    public partial class ExportingMeshesForm : Form
    {
        private List<BXDAMesh> exportMeshes = null;
        private RobotDataManager robotDataManager;

        public ExportingMeshesForm()
        {
            InitializeComponent();
            ExporterWorker.DoWork += ExporterWorker_DoWork;
            ExporterWorker.RunWorkerCompleted += ExporterWorker_RunWorkerCompleted;

            Shown += (sender, args) =>
            {
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
                ExporterWorker.RunWorkerAsync();
            };

            FormClosing += (sender, args) => RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
        }
        
        public async Task<List<BXDAMesh>> ExportMeshes(RobotDataManager robotDataManager)
        {
            this.robotDataManager = robotDataManager;
            await Task.Run(ShowDialog).ConfigureAwait(false);
            return exportMeshes;
        }

        private void SetProgress(ProgressUpdate progressUpdate)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<ProgressUpdate>)SetProgress, progressUpdate);
                return;
            }
            
            if (progressUpdate.Message != null)
                ProgressLabel.Text = progressUpdate.Message;
            ProgressBar.Style = ProgressBarStyle.Continuous;
            ProgressBar.Maximum = progressUpdate.MaxProgress;
            ProgressBar.Value = progressUpdate.CurrentProgress;
        }

        private void ExporterWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (RobotExporterAddInServer.Instance.OpenAssemblyDocument == null)
            {
                MessageBox.Show("Couldn't detect an open assembly");
                return;
            }

            exportMeshes = MeshExporter.ExportMeshes(new Progress<ProgressUpdate>(SetProgress), robotDataManager.RobotBaseNode, robotDataManager.RobotWeightKg);
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (ExporterWorker.IsBusy)
                ExporterWorker.CancelAsync();
        
            if (ExporterWorker.CancellationPending)
                Dispose();
        }

        private void ExporterWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                ProgressLabel.Text = "Export Cancelled";
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