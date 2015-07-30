using FieldExporter.Components;
namespace FieldExporter
{
    partial class MainWindow
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.applicationImages = new System.Windows.Forms.ImageList(this.components);
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.physicsTabPage = new System.Windows.Forms.TabPage();
            this.PropertyControl = new System.Windows.Forms.TabControl();
            this.exportTabPage = new System.Windows.Forms.TabPage();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicsGroupsTabControl = new FieldExporter.Components.PhysicsGroupsTabControl();
            this.exportForm1 = new FieldExporter.Components.ExportForm();
            this.physicsTabPage.SuspendLayout();
            this.PropertyControl.SuspendLayout();
            this.exportTabPage.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // applicationImages
            // 
            this.applicationImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("applicationImages.ImageStream")));
            this.applicationImages.TransparentColor = System.Drawing.Color.Transparent;
            this.applicationImages.Images.SetKeyName(0, "Synthesis Logo.png");
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.Description = "Select the file path by which to export the BXDF file.";
            // 
            // physicsTabPage
            // 
            this.physicsTabPage.BackColor = System.Drawing.Color.White;
            this.physicsTabPage.Controls.Add(this.physicsGroupsTabControl);
            this.physicsTabPage.Location = new System.Drawing.Point(4, 25);
            this.physicsTabPage.Name = "physicsTabPage";
            this.physicsTabPage.Size = new System.Drawing.Size(934, 618);
            this.physicsTabPage.TabIndex = 0;
            this.physicsTabPage.Text = "PhysicsGroups";
            // 
            // PropertyControl
            // 
            this.PropertyControl.Controls.Add(this.physicsTabPage);
            this.PropertyControl.Controls.Add(this.exportTabPage);
            this.PropertyControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyControl.Location = new System.Drawing.Point(0, 28);
            this.PropertyControl.Name = "PropertyControl";
            this.PropertyControl.SelectedIndex = 0;
            this.PropertyControl.Size = new System.Drawing.Size(942, 647);
            this.PropertyControl.TabIndex = 10;
            // 
            // exportTabPage
            // 
            this.exportTabPage.Controls.Add(this.exportForm1);
            this.exportTabPage.Location = new System.Drawing.Point(4, 25);
            this.exportTabPage.Name = "exportTabPage";
            this.exportTabPage.Size = new System.Drawing.Size(934, 618);
            this.exportTabPage.TabIndex = 4;
            this.exportTabPage.Text = "Export";
            this.exportTabPage.UseVisualStyleBackColor = true;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(942, 28);
            this.menuStrip.TabIndex = 11;
            this.menuStrip.Text = "Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(102, 24);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetSizeToolStripMenuItem,
            this.alwaysOnTopToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(76, 24);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // resetSizeToolStripMenuItem
            // 
            this.resetSizeToolStripMenuItem.Name = "resetSizeToolStripMenuItem";
            this.resetSizeToolStripMenuItem.Size = new System.Drawing.Size(177, 24);
            this.resetSizeToolStripMenuItem.Text = "Reset Size";
            this.resetSizeToolStripMenuItem.Click += new System.EventHandler(this.resetSizeToolStripMenuItem_Click);
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.CheckOnClick = true;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(177, 24);
            this.alwaysOnTopToolStripMenuItem.Text = "Always On Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // physicsGroupsTabControl
            // 
            this.physicsGroupsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.physicsGroupsTabControl.Location = new System.Drawing.Point(0, 0);
            this.physicsGroupsTabControl.Name = "physicsGroupsTabControl";
            this.physicsGroupsTabControl.SelectedIndex = 0;
            this.physicsGroupsTabControl.Size = new System.Drawing.Size(934, 618);
            this.physicsGroupsTabControl.TabIndex = 0;
            // 
            // exportForm1
            // 
            this.exportForm1.BackColor = System.Drawing.Color.White;
            this.exportForm1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("exportForm1.BackgroundImage")));
            this.exportForm1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.exportForm1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportForm1.Location = new System.Drawing.Point(0, 0);
            this.exportForm1.Name = "exportForm1";
            this.exportForm1.Padding = new System.Windows.Forms.Padding(3);
            this.exportForm1.Size = new System.Drawing.Size(934, 618);
            this.exportForm1.TabIndex = 0;
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(205)))), ((int)(((byte)(163)))));
            this.ClientSize = new System.Drawing.Size(942, 675);
            this.Controls.Add(this.PropertyControl);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Field Exporter";
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.physicsTabPage.ResumeLayout(false);
            this.PropertyControl.ResumeLayout(false);
            this.exportTabPage.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ImageList applicationImages;
        private System.Windows.Forms.TabPage physicsTabPage;
        private System.Windows.Forms.TabControl PropertyControl;
        private PhysicsGroupsTabControl physicsGroupsTabControl;
        private System.Windows.Forms.TabPage exportTabPage;
        private ExportForm exportForm1;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSizeToolStripMenuItem;
    }
}

