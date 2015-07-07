namespace FieldExporter
{
    partial class ScanProgressWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScanProgressWindow));
            this.ScanProgressBar = new System.Windows.Forms.ProgressBar();
            this.ScanInfoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ScanProgressBar
            // 
            this.ScanProgressBar.Location = new System.Drawing.Point(12, 40);
            this.ScanProgressBar.MarqueeAnimationSpeed = 2;
            this.ScanProgressBar.Name = "ScanProgressBar";
            this.ScanProgressBar.Size = new System.Drawing.Size(278, 23);
            this.ScanProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.ScanProgressBar.TabIndex = 0;
            // 
            // ScanInfoLabel
            // 
            this.ScanInfoLabel.AutoSize = true;
            this.ScanInfoLabel.Location = new System.Drawing.Point(13, 13);
            this.ScanInfoLabel.Name = "ScanInfoLabel";
            this.ScanInfoLabel.Size = new System.Drawing.Size(211, 17);
            this.ScanInfoLabel.TabIndex = 1;
            this.ScanInfoLabel.Text = "Scanning Assembly Document...";
            // 
            // ScanProgressWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 75);
            this.ControlBox = false;
            this.Controls.Add(this.ScanInfoLabel);
            this.Controls.Add(this.ScanProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScanProgressWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Scanning...";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar ScanProgressBar;
        private System.Windows.Forms.Label ScanInfoLabel;
    }
}