namespace SynthesisLauncher
{
    partial class LaunchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LaunchForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tutorialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.robotExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fieldExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.simulatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.emulatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.synthesisButton = new System.Windows.Forms.Button();
            this.fExporterButton = new System.Windows.Forms.Button();
            this.rExporterButton = new System.Windows.Forms.Button();
            this.codeViewerButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.buildLabel = new System.Windows.Forms.Label();
            this.updateLabel = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.tutorialsToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            resources.ApplyResources(this.closeToolStripMenuItem, "closeToolStripMenuItem");
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // tutorialsToolStripMenuItem
            // 
            this.tutorialsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.robotExportToolStripMenuItem,
            this.fieldExportToolStripMenuItem,
            this.simulatorToolStripMenuItem,
            this.emulatorToolStripMenuItem});
            this.tutorialsToolStripMenuItem.Name = "tutorialsToolStripMenuItem";
            resources.ApplyResources(this.tutorialsToolStripMenuItem, "tutorialsToolStripMenuItem");
            // 
            // robotExportToolStripMenuItem
            // 
            this.robotExportToolStripMenuItem.Name = "robotExportToolStripMenuItem";
            resources.ApplyResources(this.robotExportToolStripMenuItem, "robotExportToolStripMenuItem");
            this.robotExportToolStripMenuItem.Click += new System.EventHandler(this.robotExportToolStripMenuItem_Click);
            // 
            // fieldExportToolStripMenuItem
            // 
            this.fieldExportToolStripMenuItem.Name = "fieldExportToolStripMenuItem";
            resources.ApplyResources(this.fieldExportToolStripMenuItem, "fieldExportToolStripMenuItem");
            this.fieldExportToolStripMenuItem.Click += new System.EventHandler(this.fieldExportToolStripMenuItem_Click);
            // 
            // simulatorToolStripMenuItem
            // 
            this.simulatorToolStripMenuItem.Name = "simulatorToolStripMenuItem";
            resources.ApplyResources(this.simulatorToolStripMenuItem, "simulatorToolStripMenuItem");
            this.simulatorToolStripMenuItem.Click += new System.EventHandler(this.jointsToolStripMenuItem_Click);
            // 
            // emulatorToolStripMenuItem
            // 
            this.emulatorToolStripMenuItem.Name = "emulatorToolStripMenuItem";
            resources.ApplyResources(this.emulatorToolStripMenuItem, "emulatorToolStripMenuItem");
            this.emulatorToolStripMenuItem.Click += new System.EventHandler(this.emulatorToolStripMenuItem_Click);
            // 
            // synthesisButton
            // 
            resources.ApplyResources(this.synthesisButton, "synthesisButton");
            this.synthesisButton.Name = "synthesisButton";
            this.synthesisButton.UseVisualStyleBackColor = true;
            this.synthesisButton.Click += new System.EventHandler(this.synthesis_Click);
            // 
            // fExporterButton
            // 
            resources.ApplyResources(this.fExporterButton, "fExporterButton");
            this.fExporterButton.Name = "fExporterButton";
            this.fExporterButton.UseVisualStyleBackColor = true;
            this.fExporterButton.Click += new System.EventHandler(this.fExporter_Click);
            // 
            // rExporterButton
            // 
            resources.ApplyResources(this.rExporterButton, "rExporterButton");
            this.rExporterButton.Name = "rExporterButton";
            this.rExporterButton.UseVisualStyleBackColor = true;
            this.rExporterButton.Click += new System.EventHandler(this.rExporter_Click);
            // 
            // codeViewerButton
            // 
            resources.ApplyResources(this.codeViewerButton, "codeViewerButton");
            this.codeViewerButton.Name = "codeViewerButton";
            this.codeViewerButton.UseVisualStyleBackColor = true;
            this.codeViewerButton.Click += new System.EventHandler(this.codeViewer_Click);
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // label5
            // 
            resources.ApplyResources(this.label5, "label5");
            this.label5.Name = "label5";
            // 
            // buildLabel
            // 
            resources.ApplyResources(this.buildLabel, "buildLabel");
            this.buildLabel.BackColor = System.Drawing.Color.Transparent;
            this.buildLabel.Name = "buildLabel";
            // 
            // updateLabel
            // 
            resources.ApplyResources(this.updateLabel, "updateLabel");
            this.updateLabel.Name = "updateLabel";
            // 
            // LaunchForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.updateLabel);
            this.Controls.Add(this.buildLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.codeViewerButton);
            this.Controls.Add(this.rExporterButton);
            this.Controls.Add(this.fExporterButton);
            this.Controls.Add(this.synthesisButton);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "LaunchForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tutorialsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem robotExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fieldExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem simulatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem emulatorToolStripMenuItem;
        private System.Windows.Forms.Button synthesisButton;
        private System.Windows.Forms.Button fExporterButton;
        private System.Windows.Forms.Button rExporterButton;
        private System.Windows.Forms.Button codeViewerButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label buildLabel;
        private System.Windows.Forms.Label updateLabel;
    }
}

