using System.Windows.Forms;

namespace JointResolver.ControlGUI
{
    public partial class ProgressBarForm : Form
    {
        public ProgressBarForm(string windowTitle)
        {
            InitializeComponent();
            Text = windowTitle;
        }

        public void SetProgress(string message, int current, int max)
        {
            ProgressLabel.Text = message;
            ProgressBar.Maximum = max;
            ProgressBar.Value = current;
        }
    }
}