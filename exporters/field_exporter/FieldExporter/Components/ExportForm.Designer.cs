namespace FieldExporter.Components
{
    partial class ExportForm
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportForm));
            this.fieldNameTextBox = new System.Windows.Forms.TextBox();
            this.fieldNameLabel = new System.Windows.Forms.Label();
            this.exportButton = new System.Windows.Forms.Button();
            this.exportProgressBar = new System.Windows.Forms.ProgressBar();
            this.statusLabel = new System.Windows.Forms.Label();
            this.exporter = new System.ComponentModel.BackgroundWorker();
            this.animatedLogo = new System.Windows.Forms.PictureBox();
            this.verticalLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.fieldNameLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.exportButtonLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.openFolderCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.animatedLogo)).BeginInit();
            this.verticalLayoutPanel.SuspendLayout();
            this.fieldNameLayoutPanel.SuspendLayout();
            this.exportButtonLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // fieldNameTextBox
            // 
            this.fieldNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fieldNameTextBox.Location = new System.Drawing.Point(72, 3);
            this.fieldNameTextBox.Name = "fieldNameTextBox";
            this.fieldNameTextBox.Size = new System.Drawing.Size(501, 20);
            this.fieldNameTextBox.TabIndex = 9;
            this.fieldNameTextBox.TextChanged += new System.EventHandler(this.fieldNameTextBox_TextChanged);
            // 
            // fieldNameLabel
            // 
            this.fieldNameLabel.AutoSize = true;
            this.fieldNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.fieldNameLabel.Location = new System.Drawing.Point(3, 3);
            this.fieldNameLabel.Margin = new System.Windows.Forms.Padding(3);
            this.fieldNameLabel.Name = "fieldNameLabel";
            this.fieldNameLabel.Size = new System.Drawing.Size(63, 20);
            this.fieldNameLabel.TabIndex = 8;
            this.fieldNameLabel.Text = "Field Name:";
            this.fieldNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exportButton.Enabled = false;
            this.exportButton.Location = new System.Drawing.Point(3, 3);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(63, 23);
            this.exportButton.TabIndex = 7;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // exportProgressBar
            // 
            this.exportProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exportProgressBar.Location = new System.Drawing.Point(3, 320);
            this.exportProgressBar.Name = "exportProgressBar";
            this.exportProgressBar.Size = new System.Drawing.Size(570, 23);
            this.exportProgressBar.Step = 1;
            this.exportProgressBar.TabIndex = 11;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.statusLabel.Location = new System.Drawing.Point(72, 3);
            this.statusLabel.Margin = new System.Windows.Forms.Padding(3);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(138, 23);
            this.statusLabel.TabIndex = 12;
            this.statusLabel.Text = "Please specify a field name.";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // exporter
            // 
            this.exporter.WorkerReportsProgress = true;
            this.exporter.WorkerSupportsCancellation = true;
            this.exporter.DoWork += new System.ComponentModel.DoWorkEventHandler(this.exporter_DoWork);
            this.exporter.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.exporter_ProgressChanged);
            this.exporter.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.exporter_RunWorkerCompleted);
            // 
            // animatedLogo
            // 
            this.animatedLogo.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("animatedLogo.BackgroundImage")));
            this.animatedLogo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.animatedLogo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.animatedLogo.InitialImage = ((System.Drawing.Image)(resources.GetObject("animatedLogo.InitialImage")));
            this.animatedLogo.Location = new System.Drawing.Point(3, 52);
            this.animatedLogo.Name = "animatedLogo";
            this.animatedLogo.Size = new System.Drawing.Size(570, 233);
            this.animatedLogo.TabIndex = 13;
            this.animatedLogo.TabStop = false;
            // 
            // verticalLayoutPanel
            // 
            this.verticalLayoutPanel.ColumnCount = 1;
            this.verticalLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.verticalLayoutPanel.Controls.Add(this.animatedLogo, 0, 2);
            this.verticalLayoutPanel.Controls.Add(this.fieldNameLayoutPanel, 0, 0);
            this.verticalLayoutPanel.Controls.Add(this.exportProgressBar, 0, 4);
            this.verticalLayoutPanel.Controls.Add(this.exportButtonLayoutPanel, 0, 3);
            this.verticalLayoutPanel.Controls.Add(this.openFolderCheckBox, 0, 1);
            this.verticalLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.verticalLayoutPanel.Name = "verticalLayoutPanel";
            this.verticalLayoutPanel.RowCount = 5;
            this.verticalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.verticalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.verticalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.verticalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.verticalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.verticalLayoutPanel.Size = new System.Drawing.Size(576, 346);
            this.verticalLayoutPanel.TabIndex = 14;
            // 
            // fieldNameLayoutPanel
            // 
            this.fieldNameLayoutPanel.AutoSize = true;
            this.fieldNameLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.fieldNameLayoutPanel.ColumnCount = 2;
            this.fieldNameLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.fieldNameLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.fieldNameLayoutPanel.Controls.Add(this.fieldNameLabel, 0, 0);
            this.fieldNameLayoutPanel.Controls.Add(this.fieldNameTextBox, 1, 0);
            this.fieldNameLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fieldNameLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.fieldNameLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.fieldNameLayoutPanel.Name = "fieldNameLayoutPanel";
            this.fieldNameLayoutPanel.RowCount = 1;
            this.fieldNameLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.fieldNameLayoutPanel.Size = new System.Drawing.Size(576, 26);
            this.fieldNameLayoutPanel.TabIndex = 14;
            // 
            // exportButtonLayoutPanel
            // 
            this.exportButtonLayoutPanel.AutoSize = true;
            this.exportButtonLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.exportButtonLayoutPanel.ColumnCount = 2;
            this.exportButtonLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.exportButtonLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.exportButtonLayoutPanel.Controls.Add(this.exportButton, 0, 0);
            this.exportButtonLayoutPanel.Controls.Add(this.statusLabel, 1, 0);
            this.exportButtonLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportButtonLayoutPanel.Location = new System.Drawing.Point(0, 288);
            this.exportButtonLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.exportButtonLayoutPanel.Name = "exportButtonLayoutPanel";
            this.exportButtonLayoutPanel.RowCount = 1;
            this.exportButtonLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.exportButtonLayoutPanel.Size = new System.Drawing.Size(576, 29);
            this.exportButtonLayoutPanel.TabIndex = 15;
            // 
            // openFolderCheckBox
            // 
            this.openFolderCheckBox.AutoSize = true;
            this.openFolderCheckBox.Location = new System.Drawing.Point(3, 29);
            this.openFolderCheckBox.Name = "openFolderCheckBox";
            this.openFolderCheckBox.Size = new System.Drawing.Size(169, 17);
            this.openFolderCheckBox.TabIndex = 16;
            this.openFolderCheckBox.Text = "Open export folder when done";
            this.openFolderCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExportForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Controls.Add(this.verticalLayoutPanel);
            this.DoubleBuffered = true;
            this.Name = "ExportForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(582, 352);
            ((System.ComponentModel.ISupportInitialize)(this.animatedLogo)).EndInit();
            this.verticalLayoutPanel.ResumeLayout(false);
            this.verticalLayoutPanel.PerformLayout();
            this.fieldNameLayoutPanel.ResumeLayout(false);
            this.fieldNameLayoutPanel.PerformLayout();
            this.exportButtonLayoutPanel.ResumeLayout(false);
            this.exportButtonLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox fieldNameTextBox;
        private System.Windows.Forms.Label fieldNameLabel;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.ProgressBar exportProgressBar;
        private System.Windows.Forms.Label statusLabel;
        private System.ComponentModel.BackgroundWorker exporter;
        private System.Windows.Forms.PictureBox animatedLogo;
        private System.Windows.Forms.TableLayoutPanel verticalLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel fieldNameLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel exportButtonLayoutPanel;
        private System.Windows.Forms.CheckBox openFolderCheckBox;
    }
}
