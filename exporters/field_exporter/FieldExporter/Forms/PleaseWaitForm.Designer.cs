namespace FieldExporter.Forms
{
    partial class PleaseWaitForm
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
            this.pleaseWaitLabel = new System.Windows.Forms.Label();
            this.pleaseWaitBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // pleaseWaitLabel
            // 
            this.pleaseWaitLabel.Location = new System.Drawing.Point(13, 13);
            this.pleaseWaitLabel.Name = "pleaseWaitLabel";
            this.pleaseWaitLabel.Size = new System.Drawing.Size(191, 32);
            this.pleaseWaitLabel.TabIndex = 0;
            this.pleaseWaitLabel.Text = "Please wait...\r\nLoading...";
            this.pleaseWaitLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.pleaseWaitLabel.UseWaitCursor = true;
            // 
            // pleaseWaitBar
            // 
            this.pleaseWaitBar.Location = new System.Drawing.Point(12, 48);
            this.pleaseWaitBar.Name = "pleaseWaitBar";
            this.pleaseWaitBar.Size = new System.Drawing.Size(192, 23);
            this.pleaseWaitBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.pleaseWaitBar.TabIndex = 1;
            this.pleaseWaitBar.UseWaitCursor = true;
            // 
            // PleaseWaitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(216, 83);
            this.Controls.Add(this.pleaseWaitBar);
            this.Controls.Add(this.pleaseWaitLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "PleaseWaitForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PleaseWaitForm";
            this.UseWaitCursor = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label pleaseWaitLabel;
        private System.Windows.Forms.ProgressBar pleaseWaitBar;
    }
}