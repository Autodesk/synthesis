namespace JointResolver.EditorsLibrary
{
    partial class DriveTrainTypeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DriveTrainTypeForm));
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.otherButton = new System.Windows.Forms.RadioButton();
            this.hdriveButton = new System.Windows.Forms.RadioButton();
            this.tankPicture = new System.Windows.Forms.PictureBox();
            this.tankButton = new System.Windows.Forms.RadioButton();
            this.otherPicture = new System.Windows.Forms.PictureBox();
            this.hdrivePicture = new System.Windows.Forms.PictureBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tankPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.otherPicture)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hdrivePicture)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.8F);
            this.label1.Location = new System.Drawing.Point(64, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(342, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "What type of drivetrain does this robot have?";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.otherButton, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.hdriveButton, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tankPicture, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tankButton, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.otherPicture, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.hdrivePicture, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(31, 43);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 154F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(415, 181);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // otherButton
            // 
            this.otherButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.otherButton.AutoSize = true;
            this.otherButton.Location = new System.Drawing.Point(313, 158);
            this.otherButton.Name = "otherButton";
            this.otherButton.Size = new System.Drawing.Size(65, 21);
            this.otherButton.TabIndex = 9;
            this.otherButton.TabStop = true;
            this.otherButton.Text = "Other";
            this.otherButton.UseVisualStyleBackColor = true;
            this.otherButton.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // hdriveButton
            // 
            this.hdriveButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.hdriveButton.AutoSize = true;
            this.hdriveButton.Location = new System.Drawing.Point(168, 158);
            this.hdriveButton.Name = "hdriveButton";
            this.hdriveButton.Size = new System.Drawing.Size(77, 21);
            this.hdriveButton.TabIndex = 8;
            this.hdriveButton.TabStop = true;
            this.hdriveButton.Text = "H-Drive";
            this.hdriveButton.UseVisualStyleBackColor = true;
            this.hdriveButton.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // tankPicture
            // 
            this.tankPicture.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tankPicture.Image = global::JointResolver.Properties.Resources.LoadAnimation;
            this.tankPicture.InitialImage = global::JointResolver.Properties.Resources.LoadAnimation;
            this.tankPicture.Location = new System.Drawing.Point(3, 3);
            this.tankPicture.Name = "tankPicture";
            this.tankPicture.Size = new System.Drawing.Size(132, 148);
            this.tankPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.tankPicture.TabIndex = 7;
            this.tankPicture.TabStop = false;
            this.tankPicture.Click += new System.EventHandler(this.tankPicture_Click);
            // 
            // tankButton
            // 
            this.tankButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tankButton.AutoSize = true;
            this.tankButton.Location = new System.Drawing.Point(38, 158);
            this.tankButton.Name = "tankButton";
            this.tankButton.Size = new System.Drawing.Size(61, 21);
            this.tankButton.TabIndex = 3;
            this.tankButton.TabStop = true;
            this.tankButton.Text = "Tank";
            this.tankButton.UseVisualStyleBackColor = true;
            this.tankButton.CheckedChanged += new System.EventHandler(this.radioButton_CheckedChanged);
            // 
            // otherPicture
            // 
            this.otherPicture.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.otherPicture.Image = global::JointResolver.Properties.Resources.LoadAnimation;
            this.otherPicture.InitialImage = global::JointResolver.Properties.Resources.LoadAnimation;
            this.otherPicture.Location = new System.Drawing.Point(279, 3);
            this.otherPicture.Name = "otherPicture";
            this.otherPicture.Size = new System.Drawing.Size(132, 148);
            this.otherPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.otherPicture.TabIndex = 6;
            this.otherPicture.TabStop = false;
            this.otherPicture.Click += new System.EventHandler(this.otherPicture_Click);
            // 
            // hdrivePicture
            // 
            this.hdrivePicture.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.hdrivePicture.BackColor = System.Drawing.SystemColors.Control;
            this.hdrivePicture.Image = global::JointResolver.Properties.Resources.LoadAnimation;
            this.hdrivePicture.InitialImage = global::JointResolver.Properties.Resources.LoadAnimation;
            this.hdrivePicture.Location = new System.Drawing.Point(141, 3);
            this.hdrivePicture.Name = "hdrivePicture";
            this.hdrivePicture.Size = new System.Drawing.Size(132, 148);
            this.hdrivePicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.hdrivePicture.TabIndex = 5;
            this.hdrivePicture.TabStop = false;
            this.hdrivePicture.Click += new System.EventHandler(this.hdrivePicture_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(236, 244);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(102, 26);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(344, 244);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(102, 26);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // DriveTrainTypeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 279);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DriveTrainTypeForm";
            this.Text = "Drive Train Type Selection";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tankPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.otherPicture)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hdrivePicture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox otherPicture;
        private System.Windows.Forms.PictureBox hdrivePicture;
        private System.Windows.Forms.PictureBox tankPicture;
        private System.Windows.Forms.RadioButton tankButton;
        private System.Windows.Forms.RadioButton otherButton;
        private System.Windows.Forms.RadioButton hdriveButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}