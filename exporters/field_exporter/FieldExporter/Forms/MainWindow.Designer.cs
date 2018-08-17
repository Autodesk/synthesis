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
            this.propertiesTabPage = new System.Windows.Forms.TabPage();
            this.propertySetsTabControl = new FieldExporter.Components.PropertySetsTabControl();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.metaTabPage = new System.Windows.Forms.TabPage();
            this.fieldMeta = new FieldExporter.Components.FieldMetaForm();
            this.exportTabPage = new System.Windows.Forms.TabPage();
            this.exportForm = new FieldExporter.Components.ExportForm();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetSizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.alwaysOnTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tutorialsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.propertiesTabPage.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.metaTabPage.SuspendLayout();
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
            // propertiesTabPage
            // 
            this.propertiesTabPage.BackColor = System.Drawing.Color.White;
            this.propertiesTabPage.Controls.Add(this.propertySetsTabControl);
            this.propertiesTabPage.Location = new System.Drawing.Point(4, 22);
            this.propertiesTabPage.Name = "propertiesTabPage";
            this.propertiesTabPage.Size = new System.Drawing.Size(616, 391);
            this.propertiesTabPage.TabIndex = 0;
            this.propertiesTabPage.Text = "Property Sets";
            // 
            // propertySetsTabControl
            // 
            this.propertySetsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertySetsTabControl.Location = new System.Drawing.Point(0, 0);
            this.propertySetsTabControl.Name = "propertySetsTabControl";
            this.propertySetsTabControl.SelectedIndex = 0;
            this.propertySetsTabControl.Size = new System.Drawing.Size(616, 391);
            this.propertySetsTabControl.TabIndex = 0;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.metaTabPage);
            this.tabControl.Controls.Add(this.propertiesTabPage);
            this.tabControl.Controls.Add(this.exportTabPage);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 24);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(624, 417);
            this.tabControl.TabIndex = 10;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            // 
            // metaTabPage
            // 
            this.metaTabPage.Controls.Add(this.fieldMeta);
            this.metaTabPage.Location = new System.Drawing.Point(4, 22);
            this.metaTabPage.Name = "metaTabPage";
            this.metaTabPage.Size = new System.Drawing.Size(616, 391);
            this.metaTabPage.TabIndex = 5;
            this.metaTabPage.Text = "Field Info";
            this.metaTabPage.UseVisualStyleBackColor = true;
            // 
            // fieldMeta
            // 
            this.fieldMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldMeta.Location = new System.Drawing.Point(0, 0);
            this.fieldMeta.Name = "fieldMeta";
            this.fieldMeta.Size = new System.Drawing.Size(616, 391);
            this.fieldMeta.TabIndex = 0;
            // 
            // exportTabPage
            // 
            this.exportTabPage.Controls.Add(this.exportForm);
            this.exportTabPage.Location = new System.Drawing.Point(4, 22);
            this.exportTabPage.Name = "exportTabPage";
            this.exportTabPage.Size = new System.Drawing.Size(616, 391);
            this.exportTabPage.TabIndex = 4;
            this.exportTabPage.Text = "Export";
            this.exportTabPage.UseVisualStyleBackColor = true;
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
            this.exportForm.Size = new System.Drawing.Size(616, 391);
            this.exportForm.TabIndex = 0;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(624, 24);
            this.menuStrip.TabIndex = 11;
            this.menuStrip.Text = "Menu";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetSizeToolStripMenuItem,
            this.alwaysOnTopToolStripMenuItem});
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // resetSizeToolStripMenuItem
            // 
            this.resetSizeToolStripMenuItem.Name = "resetSizeToolStripMenuItem";
            this.resetSizeToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.resetSizeToolStripMenuItem.Text = "Reset Size";
            this.resetSizeToolStripMenuItem.Click += new System.EventHandler(this.resetSizeToolStripMenuItem_Click);
            // 
            // alwaysOnTopToolStripMenuItem
            // 
            this.alwaysOnTopToolStripMenuItem.CheckOnClick = true;
            this.alwaysOnTopToolStripMenuItem.Name = "alwaysOnTopToolStripMenuItem";
            this.alwaysOnTopToolStripMenuItem.Size = new System.Drawing.Size(153, 22);
            this.alwaysOnTopToolStripMenuItem.Text = "Always On Top";
            this.alwaysOnTopToolStripMenuItem.Click += new System.EventHandler(this.alwaysOnTopToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tutorialsToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // tutorialsToolStripMenuItem
            // 
            this.tutorialsToolStripMenuItem.Name = "tutorialsToolStripMenuItem";
            this.tutorialsToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.tutorialsToolStripMenuItem.Text = "Tutorials";
            this.tutorialsToolStripMenuItem.Click += new System.EventHandler(this.tutorialsToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(158)))), ((int)(((byte)(205)))), ((int)(((byte)(163)))));
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.menuStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Synthesis Field Exporter";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.propertiesTabPage.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.metaTabPage.ResumeLayout(false);
            this.exportTabPage.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ImageList applicationImages;
        private System.Windows.Forms.TabPage propertiesTabPage;
        private System.Windows.Forms.TabControl tabControl;
        private PropertySetsTabControl propertySetsTabControl;
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
        private System.Windows.Forms.TabPage metaTabPage;
        private FieldMetaForm fieldMeta;
    }
}

