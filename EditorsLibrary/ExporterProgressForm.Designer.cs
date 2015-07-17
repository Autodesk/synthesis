namespace EditorsLibrary
{
    partial class ExporterProgressForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExporterProgressForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.logPage = new System.Windows.Forms.TabPage();
            this.logText = new System.Windows.Forms.RichTextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonSaveLog = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.logPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.logPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(782, 451);
            this.tabControl1.TabIndex = 0;
            // 
            // logPage
            // 
            this.logPage.Controls.Add(this.logText);
            this.logPage.Location = new System.Drawing.Point(4, 25);
            this.logPage.Name = "logPage";
            this.logPage.Padding = new System.Windows.Forms.Padding(3);
            this.logPage.Size = new System.Drawing.Size(774, 422);
            this.logPage.TabIndex = 1;
            this.logPage.Text = "Exporter Log";
            this.logPage.UseVisualStyleBackColor = true;
            // 
            // logText
            // 
            this.logText.BackColor = System.Drawing.Color.Black;
            this.logText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.logText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logText.ForeColor = System.Drawing.Color.Lime;
            this.logText.Location = new System.Drawing.Point(3, 3);
            this.logText.Name = "logText";
            this.logText.ReadOnly = true;
            this.logText.Size = new System.Drawing.Size(768, 416);
            this.logText.TabIndex = 0;
            this.logText.Text = "";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(4, 495);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(774, 48);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 465);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 17);
            this.label1.TabIndex = 2;
            this.label1.Text = "Progress";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(649, 457);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(126, 34);
            this.buttonStart.TabIndex = 3;
            this.buttonStart.Text = "Start exporter";
            this.buttonStart.UseVisualStyleBackColor = true;
            // 
            // buttonSaveLog
            // 
            this.buttonSaveLog.Location = new System.Drawing.Point(517, 457);
            this.buttonSaveLog.Name = "buttonSaveLog";
            this.buttonSaveLog.Size = new System.Drawing.Size(126, 34);
            this.buttonSaveLog.TabIndex = 4;
            this.buttonSaveLog.Text = "Save log";
            this.buttonSaveLog.UseVisualStyleBackColor = true;
            // 
            // ExporterProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 555);
            this.Controls.Add(this.buttonSaveLog);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(800, 600);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "ExporterProgressForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Inventor Export";
            this.tabControl1.ResumeLayout(false);
            this.logPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage logPage;
        private System.Windows.Forms.RichTextBox logText;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonSaveLog;
    }
}