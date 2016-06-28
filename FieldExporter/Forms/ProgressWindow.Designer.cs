namespace FieldExporter
{
    partial class ProgressWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProgressWindow));
            this.ProcessProgressBar = new System.Windows.Forms.ProgressBar();
            this.ProcessInfoLabel = new System.Windows.Forms.Label();
            this.cancelProgressButton = new System.Windows.Forms.Button();
            this.backgroundProcess = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // ProcessProgressBar
            // 
            this.ProcessProgressBar.Location = new System.Drawing.Point(12, 40);
            this.ProcessProgressBar.Name = "ProcessProgressBar";
            this.ProcessProgressBar.Size = new System.Drawing.Size(278, 23);
            this.ProcessProgressBar.TabIndex = 0;
            // 
            // ProcessInfoLabel
            // 
            this.ProcessInfoLabel.AutoSize = true;
            this.ProcessInfoLabel.Location = new System.Drawing.Point(13, 13);
            this.ProcessInfoLabel.Name = "ProcessInfoLabel";
            this.ProcessInfoLabel.Size = new System.Drawing.Size(69, 17);
            this.ProcessInfoLabel.TabIndex = 1;
            this.ProcessInfoLabel.Text = "Progress:";
            // 
            // cancelProgressButton
            // 
            this.cancelProgressButton.Location = new System.Drawing.Point(194, 69);
            this.cancelProgressButton.Name = "cancelProgressButton";
            this.cancelProgressButton.Size = new System.Drawing.Size(96, 32);
            this.cancelProgressButton.TabIndex = 2;
            this.cancelProgressButton.Text = "Cancel";
            this.cancelProgressButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // backgroundProcess
            // 
            this.backgroundProcess.WorkerReportsProgress = true;
            this.backgroundProcess.WorkerSupportsCancellation = true;
            this.backgroundProcess.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundProcess_DoWork);
            this.backgroundProcess.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundProcess_ProgressChanged);
            this.backgroundProcess.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundProcess_RunWorkerCompleted);
            // 
            // ProgressWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(302, 113);
            this.ControlBox = false;
            this.Controls.Add(this.cancelProgressButton);
            this.Controls.Add(this.ProcessInfoLabel);
            this.Controls.Add(this.ProcessProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Progress Window";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar ProcessProgressBar;
        private System.Windows.Forms.Button cancelProgressButton;
        private System.Windows.Forms.Label ProcessInfoLabel;
        private System.ComponentModel.BackgroundWorker backgroundProcess;
    }
}