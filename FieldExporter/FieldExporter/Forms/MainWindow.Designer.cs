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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.InventorSelectButton = new System.Windows.Forms.Button();
            this.PropertyControl = new System.Windows.Forms.TabControl();
            this.CollisionTab = new System.Windows.Forms.TabPage();
            this.AddSelectionButton = new System.Windows.Forms.Button();
            this.ExportPage = new System.Windows.Forms.TabPage();
            this.FileTypeLabel = new System.Windows.Forms.Label();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.FileNameTextBox = new System.Windows.Forms.TextBox();
            this.FileSeparatorLabel = new System.Windows.Forms.Label();
            this.FilePathTextBox = new System.Windows.Forms.TextBox();
            this.ExportLocationLabel = new System.Windows.Forms.Label();
            this.ExportButton = new System.Windows.Forms.Button();
            this.SelectionAdder = new System.ComponentModel.BackgroundWorker();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.Exporter = new System.ComponentModel.BackgroundWorker();
            this.inventorTreeView = new InventorTreeView();
            this.PropertyControl.SuspendLayout();
            this.CollisionTab.SuspendLayout();
            this.ExportPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // InventorSelectButton
            // 
            this.InventorSelectButton.Location = new System.Drawing.Point(6, 368);
            this.InventorSelectButton.Name = "InventorSelectButton";
            this.InventorSelectButton.Size = new System.Drawing.Size(298, 32);
            this.InventorSelectButton.TabIndex = 9;
            this.InventorSelectButton.Tag = "";
            this.InventorSelectButton.Text = "Select in Inventor";
            this.InventorSelectButton.UseVisualStyleBackColor = true;
            this.InventorSelectButton.Click += new System.EventHandler(this.InventorSelectButton_Click);
            // 
            // PropertyControl
            // 
            this.PropertyControl.Controls.Add(this.CollisionTab);
            this.PropertyControl.Controls.Add(this.ExportPage);
            this.PropertyControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PropertyControl.Location = new System.Drawing.Point(0, 0);
            this.PropertyControl.Name = "PropertyControl";
            this.PropertyControl.SelectedIndex = 0;
            this.PropertyControl.Size = new System.Drawing.Size(622, 435);
            this.PropertyControl.TabIndex = 10;
            // 
            // CollisionTab
            // 
            this.CollisionTab.Controls.Add(this.inventorTreeView);
            this.CollisionTab.Controls.Add(this.AddSelectionButton);
            this.CollisionTab.Controls.Add(this.InventorSelectButton);
            this.CollisionTab.Location = new System.Drawing.Point(4, 25);
            this.CollisionTab.Name = "CollisionTab";
            this.CollisionTab.Padding = new System.Windows.Forms.Padding(3);
            this.CollisionTab.Size = new System.Drawing.Size(614, 406);
            this.CollisionTab.TabIndex = 0;
            this.CollisionTab.Text = "Collision Objects";
            this.CollisionTab.UseVisualStyleBackColor = true;
            // 
            // AddSelectionButton
            // 
            this.AddSelectionButton.Enabled = false;
            this.AddSelectionButton.Location = new System.Drawing.Point(310, 368);
            this.AddSelectionButton.Name = "AddSelectionButton";
            this.AddSelectionButton.Size = new System.Drawing.Size(298, 32);
            this.AddSelectionButton.TabIndex = 10;
            this.AddSelectionButton.Text = "Add Selection";
            this.AddSelectionButton.UseVisualStyleBackColor = true;
            this.AddSelectionButton.Click += new System.EventHandler(this.AddSelectionButton_Click);
            // 
            // ExportPage
            // 
            this.ExportPage.Controls.Add(this.FileTypeLabel);
            this.ExportPage.Controls.Add(this.BrowseButton);
            this.ExportPage.Controls.Add(this.FileNameTextBox);
            this.ExportPage.Controls.Add(this.FileSeparatorLabel);
            this.ExportPage.Controls.Add(this.FilePathTextBox);
            this.ExportPage.Controls.Add(this.ExportLocationLabel);
            this.ExportPage.Controls.Add(this.ExportButton);
            this.ExportPage.Location = new System.Drawing.Point(4, 25);
            this.ExportPage.Name = "ExportPage";
            this.ExportPage.Padding = new System.Windows.Forms.Padding(3);
            this.ExportPage.Size = new System.Drawing.Size(614, 406);
            this.ExportPage.TabIndex = 1;
            this.ExportPage.Text = "Export";
            this.ExportPage.UseVisualStyleBackColor = true;
            // 
            // FileTypeLabel
            // 
            this.FileTypeLabel.AutoSize = true;
            this.FileTypeLabel.Location = new System.Drawing.Point(568, 9);
            this.FileTypeLabel.Name = "FileTypeLabel";
            this.FileTypeLabel.Size = new System.Drawing.Size(38, 17);
            this.FileTypeLabel.TabIndex = 6;
            this.FileTypeLabel.Text = ".bxdf";
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
            // FileNameTextBox
            // 
            this.FileNameTextBox.Location = new System.Drawing.Point(466, 6);
            this.FileNameTextBox.Name = "FileNameTextBox";
            this.FileNameTextBox.Size = new System.Drawing.Size(96, 22);
            this.FileNameTextBox.TabIndex = 4;
            // 
            // FileSeparatorLabel
            // 
            this.FileSeparatorLabel.AutoSize = true;
            this.FileSeparatorLabel.Location = new System.Drawing.Point(448, 9);
            this.FileSeparatorLabel.Name = "FileSeparatorLabel";
            this.FileSeparatorLabel.Size = new System.Drawing.Size(12, 17);
            this.FileSeparatorLabel.TabIndex = 3;
            this.FileSeparatorLabel.Text = "\\";
            // 
            // FilePathTextBox
            // 
            this.FilePathTextBox.Location = new System.Drawing.Point(198, 6);
            this.FilePathTextBox.Name = "FilePathTextBox";
            this.FilePathTextBox.ReadOnly = true;
            this.FilePathTextBox.Size = new System.Drawing.Size(244, 22);
            this.FilePathTextBox.TabIndex = 2;
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
            // SelectionAdder
            // 
            this.SelectionAdder.DoWork += new System.ComponentModel.DoWorkEventHandler(this.SelectionAdder_DoWork);
            this.SelectionAdder.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.SelectionAdder_RunWorkerCompleted);
            // 
            // FolderBrowserDialog
            // 
            this.FolderBrowserDialog.Description = "Select the file path by which to export the BXDF file.";
            // 
            // Exporter
            // 
            this.Exporter.DoWork += new System.ComponentModel.DoWorkEventHandler(this.Exporter_DoWork);
            this.Exporter.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.Exporter_RunWorkerCompleted);
            // 
            // inventorTreeView
            // 
            this.inventorTreeView.Location = new System.Drawing.Point(8, 6);
            this.inventorTreeView.Name = "inventorTreeView";
            this.inventorTreeView.Size = new System.Drawing.Size(598, 356);
            this.inventorTreeView.TabIndex = 11;
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(255)))), ((int)(((byte)(200)))));
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.PropertyControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Field Exporter";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.PropertyControl.ResumeLayout(false);
            this.CollisionTab.ResumeLayout(false);
            this.ExportPage.ResumeLayout(false);
            this.ExportPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl PropertyControl;
        private System.Windows.Forms.TabPage CollisionTab;
        private System.Windows.Forms.TabPage ExportPage;
        private System.Windows.Forms.Button AddSelectionButton;
        private System.Windows.Forms.Button InventorSelectButton;
        private System.ComponentModel.BackgroundWorker SelectionAdder;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.TextBox FilePathTextBox;
        private System.Windows.Forms.Label ExportLocationLabel;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.TextBox FileNameTextBox;
        private System.Windows.Forms.Label FileSeparatorLabel;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.Label FileTypeLabel;
        private System.ComponentModel.BackgroundWorker Exporter;
        private InventorTreeView inventorTreeView;
    }
}

