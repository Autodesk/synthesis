using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Inventor;
using System.Linq;
using System.Threading.Tasks;

namespace JointResolver.ControlGUI
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm(string windowTitle, DoWorkEventHandler workAction)
        {
            InitializeComponent();

            Text = windowTitle;
            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;
            FormClosing += (sender, args) =>
                InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;

            Shown += delegate
            {
                ExporterWorker.RunWorkerAsync();
            };
            
            ExporterWorker.DoWork += workAction;
        }

        public void SetProgress(string message, int current, int max)
        {
            // Allows function to be called by other threads
            if (InvokeRequired)
            {
                BeginInvoke((Action<string, int, int>) SetProgress, message, current, max);
                return;
            }

            ProgressLabel.Text = message;
            ProgressBar.Maximum = max;
            ProgressBar.Value = current;
        }

        // Hide function cannot be run by worker
        public void SetProgressWindowVisible(bool v)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<bool>) SetProgressWindowVisible, v);
                return;
            }

            Visible = v;
        }
    }
}