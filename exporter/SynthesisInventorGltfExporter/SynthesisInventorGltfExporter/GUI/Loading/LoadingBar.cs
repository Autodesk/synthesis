using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using SynthesisInventorGltfExporter.Utilities;

namespace SynthesisInventorGltfExporter.GUI.Loading
{
    public sealed partial class LoadingBar : Form
    {
        public LoadingBar(string windowTitle)
        {
            InitializeComponent();
            Text = windowTitle;
        }
        
        public void SetProgress(ProgressUpdate update)
        {
            // Allows function to be called by other threads
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
