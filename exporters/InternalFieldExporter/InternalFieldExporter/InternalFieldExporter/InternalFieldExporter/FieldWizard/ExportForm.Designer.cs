namespace InternalFieldExporter.FieldWizard
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportForm));
            this.exportLocationLabel = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.loadingAnimationPictureBox = new System.Windows.Forms.PictureBox();
            this.exportProgressBar = new System.Windows.Forms.ProgressBar();
            this.exportButton = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.loadingAnimationPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // exportLocationLabel
            // 
            this.exportLocationLabel.AutoSize = true;
            this.exportLocationLabel.Location = new System.Drawing.Point(6, 17);
            this.exportLocationLabel.Name = "exportLocationLabel";
            this.exportLocationLabel.Size = new System.Drawing.Size(114, 17);
            this.exportLocationLabel.TabIndex = 8;
            this.exportLocationLabel.Text = "Export Location: ";
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(122, 12);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(74, 26);
            this.browseButton.TabIndex = 10;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filePathTextBox.Location = new System.Drawing.Point(202, 12);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(355, 22);
            this.filePathTextBox.TabIndex = 9;
            // 
            // loadingAnimationPictureBox
            // 
            this.loadingAnimationPictureBox.Image = global::InternalFieldExporter.Resource.LoadingAnimation;
            this.loadingAnimationPictureBox.Location = new System.Drawing.Point(109, 54);
            this.loadingAnimationPictureBox.Name = "loadingAnimationPictureBox";
            this.loadingAnimationPictureBox.Size = new System.Drawing.Size(354, 354);
            this.loadingAnimationPictureBox.TabIndex = 11;
            this.loadingAnimationPictureBox.TabStop = false;
            // 
            // exportProgressBar
            // 
            this.exportProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exportProgressBar.Location = new System.Drawing.Point(9, 451);
            this.exportProgressBar.Name = "exportProgressBar";
            this.exportProgressBar.Size = new System.Drawing.Size(548, 28);
            this.exportProgressBar.Step = 1;
            this.exportProgressBar.TabIndex = 11;
            // 
            // exportButton
            // 
            this.exportButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exportButton.Location = new System.Drawing.Point(9, 414);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 31);
            this.exportButton.TabIndex = 12;
            this.exportButton.Text = "Export";
            this.exportButton.UseVisualStyleBackColor = true;
            this.exportButton.Click += new System.EventHandler(this.exportButton_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(106, 421);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(352, 17);
            this.statusLabel.TabIndex = 12;
            this.statusLabel.Text = "Please select an export location in the browser above. ";
            // 
            // folderBrowserDialog
            // 
            this.folderBrowserDialog.Description = "Select the file path by which to export the field.";
            this.folderBrowserDialog.ShowNewFolderButton = false;
            // 
            // ExportForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(563, 496);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.exportProgressBar);
            this.Controls.Add(this.loadingAnimationPictureBox);
            this.Controls.Add(this.filePathTextBox);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.exportLocationLabel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            ((System.ComponentModel.ISupportInitialize)(this.loadingAnimationPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label exportLocationLabel;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.PictureBox loadingAnimationPictureBox;
        private System.Windows.Forms.ProgressBar exportProgressBar;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
    }
}