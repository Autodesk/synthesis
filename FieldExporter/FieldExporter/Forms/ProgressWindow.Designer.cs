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
            this.CancelButton = new System.Windows.Forms.Button();
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
            this.ProcessInfoLabel.Size = new System.Drawing.Size(90, 17);
            this.ProcessInfoLabel.TabIndex = 1;
            this.ProcessInfoLabel.Text = "Processing...";
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(194, 69);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(96, 32);
            this.CancelButton.TabIndex = 2;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ProgressWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(302, 113);
            this.ControlBox = false;
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.ProcessInfoLabel);
            this.Controls.Add(this.ProcessProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Processing...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ProgressBar ProcessProgressBar;
        public System.Windows.Forms.Label ProcessInfoLabel;
        public System.Windows.Forms.Button CancelButton;
    }
}