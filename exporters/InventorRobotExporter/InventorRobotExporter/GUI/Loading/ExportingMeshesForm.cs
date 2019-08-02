using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using InventorRobotExporter.Exporter;
using InventorRobotExporter.Managers;
using InventorRobotExporter.Utilities;

namespace InventorRobotExporter.GUI.Loading
{
    public partial class ExportingMeshesForm : Form
    {
        private List<BXDAMesh> exportMeshes = null;
        private RobotDataManager robotDataManager;
        private BackgroundWorker ExportMeshWorker;

        public ExportingMeshesForm()
        {
            InitializeComponent();
            ExportMeshWorker = new BackgroundWorker();
            ExportMeshWorker.WorkerReportsProgress = true;
            ExportMeshWorker.WorkerSupportsCancellation = true;
            ExportMeshWorker.DoWork += ExportMeshWorker_DoWork;
            ExportMeshWorker.RunWorkerCompleted += ExportMeshWorker_RunWorkerCompleted;
            
            DialogResult = DialogResult.OK;

            Shown += (sender, args) =>
            {
                RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = true;
                ExportMeshWorker.RunWorkerAsync();
            };

            FormClosing += (sender, args) =>
            {
                if (!ExportMeshWorker.IsBusy) return;
                args.Cancel = true;
                CancelExportWorker();
            };
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

        private void ExportMeshWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            exportMeshes = MeshExporter.ExportMeshes(new Progress<ProgressUpdate>(SetProgress), robotDataManager.RobotBaseNode, robotDataManager.RobotWeightKg, ExportMeshWorker);
        }

        private void CancelExportWorker(object sender=null, EventArgs e=null)
        {
            ExportMeshWorker.CancelAsync();
            ProgressLabel.Text = "Cancelling export, please wait...";
            DialogResult = DialogResult.Cancel;
        }

        private void ExportMeshWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                ProgressLabel.Text = "An error occurred.";
                MessageBox.Show(e.Error.Message);
                DialogResult = DialogResult.Abort;
            }
            Close();
            RobotExporterAddInServer.Instance.Application.UserInterfaceManager.UserInteractionDisabled = false;
        }
    }
}