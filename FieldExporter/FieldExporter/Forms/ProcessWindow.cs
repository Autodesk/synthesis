using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter
{
    public partial class ProcessWindow : Form
    {
        /// <summary>
        /// Used for storing the state of a process.
        /// </summary>
        public enum ProcessState
        {
            INCOMPLETE,
            SUCCEEDED,
            CANCELLED
        }

        /// <summary>
        /// Stores the current state of the process.
        /// </summary>
        public ProcessState currentState
        {
            get;
            private set;
        }

        /// <summary>
        /// The Action called during the main process.
        /// </summary>
        private Action Work;

        /// <summary>
        /// The Action called when the main process terminates.
        /// </summary>
        private Action Complete;

        /// <summary>
        /// Constructs a new ProgessWindow with the given owner, title, label, ProgressBar information,
        /// and actions.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="title"></param>
        /// <param name="labelText"></param>
        /// <param name="minProgress"></param>
        /// <param name="maxProgress"></param>
        /// <param name="progressBarStyle"></param>
        /// <param name="Work"></param>
        /// <param name="Complete"></param>
        public ProcessWindow(IWin32Window owner, string title, string labelText,
            int minProgress, int maxProgress,
            Action Work, Action Complete)
        {
            InitializeComponent();

            Text = title;
            ProcessInfoLabel.Text = labelText;

            ProcessProgressBar.Minimum = minProgress;
            ProcessProgressBar.Maximum = maxProgress;

            this.Work = Work;
            this.Complete = Complete;

            currentState = ProcessState.INCOMPLETE;
            
            Show(owner);
        }

        /// <summary>
        /// Starts the main process.
        /// </summary>
        public void StartProcess()
        {
            if (!backgroundProcess.IsBusy)
            {
                backgroundProcess.RunWorkerAsync();
            }
        }

        /// <summary>
        /// Sets the progress of the ProgressBar.
        /// </summary>
        /// <param name="progress"></param>
        public void SetProgress(int progress, string label)
        {
            if (backgroundProcess.IsBusy)
            {
                backgroundProcess.ReportProgress(progress, label);
            }
        }

        /// <summary>
        /// Closes the window when the "Cancel" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (backgroundProcess.IsBusy)
            {
                backgroundProcess.CancelAsync();
                currentState = ProcessState.CANCELLED;
            }
        }

        /// <summary>
        /// Executes the work action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            Work();

            if (!backgroundProcess.CancellationPending)
                currentState = ProcessState.SUCCEEDED;
        }

        /// <summary>
        /// Updates the progress bar and label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Invoke(new Action(() =>
                {
                    ProcessProgressBar.Value = e.ProgressPercentage;
                    ProcessInfoLabel.Text = e.UserState.ToString();
                }));
        }

        /// <summary>
        /// Executes the complete action and disposes the form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Complete();
            Dispose();
        }
    }
}
