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
            this.exportPage = new System.Windows.Forms.TabPage();
            this.fileNameLabel = new System.Windows.Forms.Label();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.FilePathTextBox = new System.Windows.Forms.TextBox();
            this.FileSeparatorLabel = new System.Windows.Forms.Label();
            this.ExportLocationLabel = new System.Windows.Forms.Label();
            this.ExportButton = new System.Windows.Forms.Button();
            this.physicsTab = new System.Windows.Forms.TabPage();
            this.physicsGroupsTabControl = new FieldExporter.Components.PhysicsGroupsTabControl();
            this.PropertyControl = new System.Windows.Forms.TabControl();
            this.exportPage.SuspendLayout();
            this.physicsTab.SuspendLayout();
            this.PropertyControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // applicationImages
            // 
            this.applicationImages.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("applicationImages.ImageStream")));
            this.applicationImages.TransparentColor = System.Drawing.Color.Transparent;
            this.applicationImages.Images.SetKeyName(0, "AddIcon.png");
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.Description = "Select the file path by which to export the BXDF file.";
            // 
            // exportPage
            // 
            this.exportPage.BackColor = System.Drawing.Color.White;
            this.exportPage.Controls.Add(this.fileNameLabel);
            this.exportPage.Controls.Add(this.BrowseButton);
            this.exportPage.Controls.Add(this.FilePathTextBox);
            this.exportPage.Controls.Add(this.FileSeparatorLabel);
            this.exportPage.Controls.Add(this.ExportLocationLabel);
            this.exportPage.Controls.Add(this.ExportButton);
            this.exportPage.Location = new System.Drawing.Point(4, 25);
            this.exportPage.Name = "exportPage";
            this.exportPage.Padding = new System.Windows.Forms.Padding(3);
            this.exportPage.Size = new System.Drawing.Size(614, 406);
            this.exportPage.TabIndex = 1;
            this.exportPage.Text = "Export";
            // 
            // fileNameLabel
            // 
            this.fileNameLabel.AutoSize = true;
            this.fileNameLabel.Location = new System.Drawing.Point(499, 3);
            this.fileNameLabel.Name = "fileNameLabel";
            this.fileNameLabel.Size = new System.Drawing.Size(107, 34);
            this.fileNameLabel.TabIndex = 6;
            this.fileNameLabel.Text = "description.bxdf\r\nmesh.bxda";
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(122, 6);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(70, 22);
            this.BrowseButton.TabIndex = 5;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // FilePathTextBox
            // 
            this.FilePathTextBox.Location = new System.Drawing.Point(198, 6);
            this.FilePathTextBox.Name = "FilePathTextBox";
            this.FilePathTextBox.ReadOnly = true;
            this.FilePathTextBox.Size = new System.Drawing.Size(277, 22);
            this.FilePathTextBox.TabIndex = 2;
            // 
            // FileSeparatorLabel
            // 
            this.FileSeparatorLabel.AutoSize = true;
            this.FileSeparatorLabel.Location = new System.Drawing.Point(481, 9);
            this.FileSeparatorLabel.Name = "FileSeparatorLabel";
            this.FileSeparatorLabel.Size = new System.Drawing.Size(12, 17);
            this.FileSeparatorLabel.TabIndex = 3;
            this.FileSeparatorLabel.Text = "\\";
            // 
            // ExportLocationLabel
            // 
            this.ExportLocationLabel.AutoSize = true;
            this.ExportLocationLabel.Location = new System.Drawing.Point(6, 9);
            this.ExportLocationLabel.Name = "ExportLocationLabel";
            this.ExportLocationLabel.Size = new System.Drawing.Size(110, 17);
            this.ExportLocationLabel.TabIndex = 1;
            this.ExportLocationLabel.Text = "Export Location:";
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(6, 368);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(602, 32);
            this.ExportButton.TabIndex = 0;
            this.ExportButton.Text = "Export";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // physicsTab
            // 
            this.physicsTab.BackColor = System.Drawing.Color.White;
            this.physicsTab.Controls.Add(this.physicsGroupsTabControl);
            this.physicsTab.Location = new System.Drawing.Point(4, 25);
            this.physicsTab.Name = "physicsTab";
            this.physicsTab.Padding = new System.Windows.Forms.Padding(3);
            this.physicsTab.Size = new System.Drawing.Size(614, 406);
            this.physicsTab.TabIndex = 0;
            this.physicsTab.Text = "PhysicsGroups";
            // 
            // physicsGroupsTabControl
            // 
            this.physicsGroupsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.physicsGroupsTabControl.Location = new System.Drawing.Point(3, 3);
            this.physicsGroupsTabControl.Name = "physicsGroupsTabControl";
            this.physicsGroupsTabControl.SelectedIndex = 0;
            this.physicsGroupsTabControl.Size = new System.Drawing.Size(608, 400);
            this.physicsGroupsTabControl.TabIndex = 0;
            // 
            // PropertyControl
            // 
            this.PropertyControl.Controls.Add(this.physicsTab);
            this.PropertyControl.Controls.Add(this.exportPage);
            this.PropertyControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyControl.Location = new System.Drawing.Point(0, 0);
            this.PropertyControl.Name = "PropertyControl";
            this.PropertyControl.SelectedIndex = 0;
            this.PropertyControl.Size = new System.Drawing.Size(622, 435);
            this.PropertyControl.TabIndex = 10;
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.PropertyControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Field Exporter";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.exportPage.ResumeLayout(false);
            this.exportPage.PerformLayout();
            this.physicsTab.ResumeLayout(false);
            this.PropertyControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.ImageList applicationImages;
        private System.Windows.Forms.TabPage exportPage;
        private System.Windows.Forms.Label fileNameLabel;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox FilePathTextBox;
        private System.Windows.Forms.Label FileSeparatorLabel;
        private System.Windows.Forms.Label ExportLocationLabel;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.TabPage physicsTab;
        private System.Windows.Forms.TabControl PropertyControl;
        private PhysicsGroupsTabControl physicsGroupsTabControl;
    }
}

