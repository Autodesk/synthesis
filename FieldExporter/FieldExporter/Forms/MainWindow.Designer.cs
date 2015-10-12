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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.exportTabPage = new System.Windows.Forms.TabPage();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicsGroupsTabControl = new FieldExporter.Components.PhysicsGroupsTabControl();
            this.exportForm = new FieldExporter.Components.ExportForm();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tutorialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.physicsTabPage.SuspendLayout();
            this.tabControl.SuspendLayout();
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
            this.physicsTabPage.Size = new System.Drawing.Size(614, 378);
            this.physicsTabPage.TabIndex = 0;
            this.physicsTabPage.Text = "PhysicsGroups";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.physicsTabPage);
            this.tabControl.Controls.Add(this.exportTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 28);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(622, 407);
            this.tabControl.TabIndex = 10;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            // 
            // exportTabPage
            // 
            this.exportTabPage.Controls.Add(this.exportForm);
            this.exportTabPage.Location = new System.Drawing.Point(4, 25);
            this.exportTabPage.Name = "exportTabPage";
            this.exportTabPage.Size = new System.Drawing.Size(614, 378);
            this.exportTabPage.TabIndex = 4;
            this.exportTabPage.Text = "Export";
            this.exportTabPage.UseVisualStyleBackColor = true;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(622, 28);
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
            this.physicsGroupsTabControl.Size = new System.Drawing.Size(614, 378);
            this.physicsGroupsTabControl.TabIndex = 0;
            // 
            // exportForm
            // 
            this.exportForm.BackColor = System.Drawing.Color.White;
            this.exportForm.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("exportForm.BackgroundImage")));
            this.exportForm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.exportForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportForm.Location = new System.Drawing.Point(0, 0);
            this.exportForm.Name = "exportForm";
            this.exportForm.Padding = new System.Windows.Forms.Padding(3);
            this.exportForm.Size = new System.Drawing.Size(614, 378);
            this.exportForm.TabIndex = 0;
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tutorialsToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(53, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // tutorialsToolStripMenuItem
            // 
            this.tutorialsToolStripMenuItem.Name = "tutorialsToolStripMenuItem";
            this.tutorialsToolStripMenuItem.Size = new System.Drawing.Size(152, 24);
            this.tutorialsToolStripMenuItem.Text = "Tutorials";
            this.tutorialsToolStripMenuItem.Click += new System.EventHandler(this.tutorialsToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(205)))), ((int)(((byte)(163)))));
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Synthesis Field Exporter";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.physicsTabPage.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
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
        private System.Windows.Forms.TabControl tabControl;
        private PhysicsGroupsTabControl physicsGroupsTabControl;
        private System.Windows.Forms.TabPage exportTabPage;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem alwaysOnTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetSizeToolStripMenuItem;
        private ExportForm exportForm;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tutorialsToolStripMenuItem;
    }
}

