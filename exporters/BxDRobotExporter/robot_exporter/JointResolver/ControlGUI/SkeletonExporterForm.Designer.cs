namespace JointResolver.ControlGUI
{
    partial class SkeletonExporterForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ProgressLabel = new System.Windows.Forms.Label();
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.ExporterWorker = new System.ComponentModel.BackgroundWorker();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.Controls.Add(this.ProgressLabel, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.ProgressBar, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(248, 71);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // ProgressLabel
            // 
            this.ProgressLabel.AutoSize = true;
            this.ProgressLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProgressLabel.Location = new System.Drawing.Point(13, 0);
            this.ProgressLabel.Name = "ProgressLabel";
            this.ProgressLabel.Size = new System.Drawing.Size(222, 35);
            this.ProgressLabel.TabIndex = 0;
            this.ProgressLabel.Text = "Building nodes...";
            this.ProgressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProgressBar
            // 
            this.ProgressBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.ProgressBar.Location = new System.Drawing.Point(13, 38);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(222, 23);
            this.ProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.ProgressBar.TabIndex = 1;
            // 
            // ExporterWorker
            // 
            this.ExporterWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ExporterWorker_DoWork);
            this.ExporterWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.ExporterWorker_RunWorkerCompleted);
            // 
            // SkeletonExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(248, 71);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.MaximizeBox = false;
            this.Name = "SkeletonExporterForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Analyzing Model";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label ProgressLabel;
        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.ComponentModel.BackgroundWorker ExporterWorker;
    }
}