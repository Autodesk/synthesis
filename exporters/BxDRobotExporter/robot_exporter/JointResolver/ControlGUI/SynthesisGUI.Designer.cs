using EditorsLibrary;

partial class SynthesisGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SynthesisGUI));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.bxdaEditorPane1 = new EditorsLibrary.BXDAEditorPane();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.jointEditorPane1 = new EditorsLibrary.JointEditorPane();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolstripFile = new System.Windows.Forms.ToolStripDropDownButton();
            this.fileNew = new System.Windows.Forms.ToolStripMenuItem();
            this.fileOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.fileLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSave = new System.Windows.Forms.ToolStripMenuItem();
            this.fileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.fileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolstripSettings = new System.Windows.Forms.ToolStripDropDownButton();
            this.settingsExporter = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.toolstripHelp = new System.Windows.Forms.ToolStripDropDownButton();
            this.helpTutorials = new System.Windows.Forms.ToolStripMenuItem();
            this.helpAbout = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitContainer1.Location = new System.Drawing.Point(0, -300);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.bxdaEditorPane1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(684, 587);
            this.splitContainer1.SplitterDistance = 239;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            // 
            // bxdaEditorPane1
            // 
            this.bxdaEditorPane1.AutoSize = true;
            this.bxdaEditorPane1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.bxdaEditorPane1.AutoValidate = System.Windows.Forms.AutoValidate.EnableAllowFocusChange;
            this.bxdaEditorPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bxdaEditorPane1.Location = new System.Drawing.Point(0, 0);
            this.bxdaEditorPane1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bxdaEditorPane1.Name = "bxdaEditorPane1";
            this.bxdaEditorPane1.Size = new System.Drawing.Size(239, 587);
            this.bxdaEditorPane1.TabIndex = 0;
            this.bxdaEditorPane1.Units = "lb";
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.jointEditorPane1);
            this.splitContainer2.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer2.Size = new System.Drawing.Size(442, 587);
            this.splitContainer2.SplitterDistance = 391;
            this.splitContainer2.SplitterWidth = 3;
            this.splitContainer2.TabIndex = 0;
            // 
            // jointEditorPane1
            // 
            this.jointEditorPane1.AutoSize = true;
            this.jointEditorPane1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.jointEditorPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jointEditorPane1.Location = new System.Drawing.Point(0, 0);
            this.jointEditorPane1.Margin = new System.Windows.Forms.Padding(2);
            this.jointEditorPane1.Name = "jointEditorPane1";
            this.jointEditorPane1.Size = new System.Drawing.Size(442, 193);
            this.jointEditorPane1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolstripFile,
            this.toolstripSettings,
            this.toolstripHelp});
            this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(684, 22);
            this.toolStrip1.Stretch = true;
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolstripFile
            // 
            this.toolstripFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolstripFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileNew,
            this.fileOpen,
            this.fileLoad,
            this.fileSave,
            this.fileSaveAs,
            this.fileExit});
            this.toolstripFile.Image = ((System.Drawing.Image)(resources.GetObject("toolstripFile.Image")));
            this.toolstripFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripFile.Name = "toolstripFile";
            this.toolstripFile.Size = new System.Drawing.Size(38, 19);
            this.toolstripFile.Text = "File";
            // 
            // fileNew
            // 
            this.fileNew.Name = "fileNew";
            this.fileNew.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.fileNew.Size = new System.Drawing.Size(216, 22);
            this.fileNew.Text = "New";
            // 
            // fileOpen
            // 
            this.fileOpen.Name = "fileOpen";
            this.fileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.fileOpen.Size = new System.Drawing.Size(216, 22);
            this.fileOpen.Text = "Open";
            // 
            // fileLoad
            // 
            this.fileLoad.Name = "fileLoad";
            this.fileLoad.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.fileLoad.Size = new System.Drawing.Size(216, 22);
            this.fileLoad.Text = "Load from Inventor";
            // 
            // fileSave
            // 
            this.fileSave.Name = "fileSave";
            this.fileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.fileSave.Size = new System.Drawing.Size(216, 22);
            this.fileSave.Text = "Save";
            // 
            // fileSaveAs
            // 
            this.fileSaveAs.Name = "fileSaveAs";
            this.fileSaveAs.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
            this.fileSaveAs.Size = new System.Drawing.Size(216, 22);
            this.fileSaveAs.Text = "Save As";
            // 
            // fileExit
            // 
            this.fileExit.Name = "fileExit";
            this.fileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.fileExit.Size = new System.Drawing.Size(216, 22);
            this.fileExit.Text = "Exit";
            // 
            // toolstripSettings
            // 
            this.toolstripSettings.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolstripSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsExporter,
            this.settingsViewer});
            this.toolstripSettings.Image = ((System.Drawing.Image)(resources.GetObject("toolstripSettings.Image")));
            this.toolstripSettings.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripSettings.Name = "toolstripSettings";
            this.toolstripSettings.Size = new System.Drawing.Size(62, 19);
            this.toolstripSettings.Text = "Settings";
            // 
            // settingsExporter
            // 
            this.settingsExporter.Name = "settingsExporter";
            this.settingsExporter.Size = new System.Drawing.Size(162, 22);
            this.settingsExporter.Text = "Exporter Settings";
            // 
            // settingsViewer
            // 
            this.settingsViewer.Name = "settingsViewer";
            this.settingsViewer.Size = new System.Drawing.Size(162, 22);
            this.settingsViewer.Text = "Viewer Settings";
            // 
            // toolstripHelp
            // 
            this.toolstripHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolstripHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpTutorials,
            this.helpAbout});
            this.toolstripHelp.Image = ((System.Drawing.Image)(resources.GetObject("toolstripHelp.Image")));
            this.toolstripHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolstripHelp.Name = "toolstripHelp";
            this.toolstripHelp.Size = new System.Drawing.Size(45, 19);
            this.toolstripHelp.Text = "Help";
            // 
            // helpTutorials
            // 
            this.helpTutorials.Name = "helpTutorials";
            this.helpTutorials.Size = new System.Drawing.Size(158, 22);
            this.helpTutorials.Text = "Online Tutorials";
            this.helpTutorials.Click += new System.EventHandler(this.HelpTutorials_Click);
            // 
            // helpAbout
            // 
            this.helpAbout.Name = "helpAbout";
            this.helpAbout.Size = new System.Drawing.Size(158, 22);
            this.helpAbout.Text = "About";
            // 
            // SynthesisGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 287);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "SynthesisGUI";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "BXD Synthesis";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);

    }

    #endregion
    
    private System.Windows.Forms.SplitContainer splitContainer1;
    private System.Windows.Forms.SplitContainer splitContainer2;
    private System.Windows.Forms.ToolStrip toolStrip1;
    private System.Windows.Forms.ToolStripDropDownButton toolstripFile;
    private System.Windows.Forms.ToolStripMenuItem fileNew;
    private System.Windows.Forms.ToolStripMenuItem fileOpen;
    private System.Windows.Forms.ToolStripMenuItem fileSave;
    private System.Windows.Forms.ToolStripMenuItem fileSaveAs;
    private System.Windows.Forms.ToolStripMenuItem fileExit;
    private System.Windows.Forms.ToolStripMenuItem fileLoad;
    private System.Windows.Forms.ToolStripDropDownButton toolstripSettings;
    private System.Windows.Forms.ToolStripMenuItem settingsExporter;
    private System.Windows.Forms.ToolStripMenuItem settingsViewer;
    private System.Windows.Forms.ToolStripDropDownButton toolstripHelp;
    private System.Windows.Forms.ToolStripMenuItem helpTutorials;
    private System.Windows.Forms.ToolStripMenuItem helpAbout;
    public BXDAEditorPane bxdaEditorPane1;
    public JointEditorPane jointEditorPane1;
}
