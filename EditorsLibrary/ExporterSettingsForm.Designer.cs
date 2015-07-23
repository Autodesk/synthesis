namespace EditorsLibrary
{
    partial class ExporterSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExporterSettingsForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.trackbarMeshResolution = new System.Windows.Forms.TrackBar();
            this.labelMeshResolution = new System.Windows.Forms.Label();
            this.flavorText1 = new System.Windows.Forms.Label();
            this.flavorText2 = new System.Windows.Forms.Label();
            this.checkboxFancyColors = new System.Windows.Forms.CheckBox();
            this.groupboxMesh = new System.Windows.Forms.GroupBox();
            this.groupboxSkeleton = new System.Windows.Forms.GroupBox();
            this.labelFuture = new System.Windows.Forms.Label();
            this.checkboxSoftBodies = new System.Windows.Forms.CheckBox();
            this.groupboxGeneral = new System.Windows.Forms.GroupBox();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.buttonChooseText = new System.Windows.Forms.Button();
            this.buttonChooseBackground = new System.Windows.Forms.Button();
            this.labelBackgroundColor = new System.Windows.Forms.Label();
            this.labelTextColor = new System.Windows.Forms.Label();
            this.checkboxSaveLog = new System.Windows.Forms.CheckBox();
            this.textboxLogLocation = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.trackbarMeshResolution)).BeginInit();
            this.groupboxMesh.SuspendLayout();
            this.groupboxSkeleton.SuspendLayout();
            this.groupboxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(12, 507);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(160, 36);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(210, 507);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(160, 36);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // trackbarMeshResolution
            // 
            this.trackbarMeshResolution.BackColor = System.Drawing.SystemColors.Control;
            this.trackbarMeshResolution.Location = new System.Drawing.Point(6, 85);
            this.trackbarMeshResolution.Maximum = 1;
            this.trackbarMeshResolution.Name = "trackbarMeshResolution";
            this.trackbarMeshResolution.Size = new System.Drawing.Size(346, 56);
            this.trackbarMeshResolution.TabIndex = 6;
            // 
            // labelMeshResolution
            // 
            this.labelMeshResolution.AutoSize = true;
            this.labelMeshResolution.ForeColor = System.Drawing.Color.Black;
            this.labelMeshResolution.Location = new System.Drawing.Point(122, 43);
            this.labelMeshResolution.Name = "labelMeshResolution";
            this.labelMeshResolution.Size = new System.Drawing.Size(108, 17);
            this.labelMeshResolution.TabIndex = 7;
            this.labelMeshResolution.Text = "Mesh resolution";
            // 
            // flavorText1
            // 
            this.flavorText1.AutoSize = true;
            this.flavorText1.ForeColor = System.Drawing.Color.Black;
            this.flavorText1.Location = new System.Drawing.Point(6, 124);
            this.flavorText1.Name = "flavorText1";
            this.flavorText1.Size = new System.Drawing.Size(138, 17);
            this.flavorText1.TabIndex = 8;
            this.flavorText1.Text = "1999 State of the art";
            // 
            // flavorText2
            // 
            this.flavorText2.AutoSize = true;
            this.flavorText2.ForeColor = System.Drawing.Color.Black;
            this.flavorText2.Location = new System.Drawing.Point(250, 124);
            this.flavorText2.Name = "flavorText2";
            this.flavorText2.Size = new System.Drawing.Size(102, 17);
            this.flavorText2.TabIndex = 9;
            this.flavorText2.Text = "Is this real life?";
            // 
            // checkboxFancyColors
            // 
            this.checkboxFancyColors.AutoSize = true;
            this.checkboxFancyColors.Checked = true;
            this.checkboxFancyColors.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxFancyColors.ForeColor = System.Drawing.Color.Black;
            this.checkboxFancyColors.Location = new System.Drawing.Point(6, 178);
            this.checkboxFancyColors.Name = "checkboxFancyColors";
            this.checkboxFancyColors.Size = new System.Drawing.Size(135, 21);
            this.checkboxFancyColors.TabIndex = 10;
            this.checkboxFancyColors.Text = "Use fancy colors";
            this.checkboxFancyColors.UseVisualStyleBackColor = true;
            // 
            // groupboxMesh
            // 
            this.groupboxMesh.Controls.Add(this.flavorText2);
            this.groupboxMesh.Controls.Add(this.flavorText1);
            this.groupboxMesh.Controls.Add(this.trackbarMeshResolution);
            this.groupboxMesh.Controls.Add(this.checkboxFancyColors);
            this.groupboxMesh.Controls.Add(this.labelMeshResolution);
            this.groupboxMesh.ForeColor = System.Drawing.Color.Gray;
            this.groupboxMesh.Location = new System.Drawing.Point(12, 273);
            this.groupboxMesh.Name = "groupboxMesh";
            this.groupboxMesh.Size = new System.Drawing.Size(358, 204);
            this.groupboxMesh.TabIndex = 11;
            this.groupboxMesh.TabStop = false;
            this.groupboxMesh.Text = "Mesh options";
            // 
            // groupboxSkeleton
            // 
            this.groupboxSkeleton.Controls.Add(this.labelFuture);
            this.groupboxSkeleton.Controls.Add(this.checkboxSoftBodies);
            this.groupboxSkeleton.ForeColor = System.Drawing.Color.Gray;
            this.groupboxSkeleton.Location = new System.Drawing.Point(15, 178);
            this.groupboxSkeleton.Name = "groupboxSkeleton";
            this.groupboxSkeleton.Size = new System.Drawing.Size(355, 89);
            this.groupboxSkeleton.TabIndex = 12;
            this.groupboxSkeleton.TabStop = false;
            this.groupboxSkeleton.Text = "Skeleton options";
            // 
            // labelFuture
            // 
            this.labelFuture.AutoSize = true;
            this.labelFuture.ForeColor = System.Drawing.Color.Black;
            this.labelFuture.Location = new System.Drawing.Point(3, 29);
            this.labelFuture.Name = "labelFuture";
            this.labelFuture.Size = new System.Drawing.Size(281, 17);
            this.labelFuture.TabIndex = 1;
            this.labelFuture.Text = "To be implemented sometime in the future?";
            // 
            // checkboxSoftBodies
            // 
            this.checkboxSoftBodies.AutoSize = true;
            this.checkboxSoftBodies.Enabled = false;
            this.checkboxSoftBodies.ForeColor = System.Drawing.Color.Black;
            this.checkboxSoftBodies.Location = new System.Drawing.Point(6, 61);
            this.checkboxSoftBodies.Name = "checkboxSoftBodies";
            this.checkboxSoftBodies.Size = new System.Drawing.Size(143, 21);
            this.checkboxSoftBodies.TabIndex = 0;
            this.checkboxSoftBodies.Text = "Export soft bodies";
            this.checkboxSoftBodies.UseVisualStyleBackColor = true;
            // 
            // groupboxGeneral
            // 
            this.groupboxGeneral.Controls.Add(this.buttonChooseFolder);
            this.groupboxGeneral.Controls.Add(this.buttonChooseText);
            this.groupboxGeneral.Controls.Add(this.buttonChooseBackground);
            this.groupboxGeneral.Controls.Add(this.labelBackgroundColor);
            this.groupboxGeneral.Controls.Add(this.labelTextColor);
            this.groupboxGeneral.Controls.Add(this.checkboxSaveLog);
            this.groupboxGeneral.Controls.Add(this.textboxLogLocation);
            this.groupboxGeneral.ForeColor = System.Drawing.Color.Gray;
            this.groupboxGeneral.Location = new System.Drawing.Point(15, 13);
            this.groupboxGeneral.Name = "groupboxGeneral";
            this.groupboxGeneral.Size = new System.Drawing.Size(349, 159);
            this.groupboxGeneral.TabIndex = 13;
            this.groupboxGeneral.TabStop = false;
            this.groupboxGeneral.Text = "General options";
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.ForeColor = System.Drawing.Color.Black;
            this.buttonChooseFolder.Location = new System.Drawing.Point(314, 51);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(29, 22);
            this.buttonChooseFolder.TabIndex = 6;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // buttonChooseText
            // 
            this.buttonChooseText.BackColor = System.Drawing.Color.Black;
            this.buttonChooseText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChooseText.Location = new System.Drawing.Point(168, 88);
            this.buttonChooseText.Name = "buttonChooseText";
            this.buttonChooseText.Size = new System.Drawing.Size(102, 24);
            this.buttonChooseText.TabIndex = 5;
            this.buttonChooseText.UseVisualStyleBackColor = false;
            this.buttonChooseText.Click += new System.EventHandler(this.buttonChooseText_Click);
            // 
            // buttonChooseBackground
            // 
            this.buttonChooseBackground.BackColor = System.Drawing.Color.Black;
            this.buttonChooseBackground.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChooseBackground.Location = new System.Drawing.Point(168, 126);
            this.buttonChooseBackground.Name = "buttonChooseBackground";
            this.buttonChooseBackground.Size = new System.Drawing.Size(102, 24);
            this.buttonChooseBackground.TabIndex = 4;
            this.buttonChooseBackground.UseVisualStyleBackColor = false;
            this.buttonChooseBackground.Click += new System.EventHandler(this.buttonChooseBackground_Click);
            // 
            // labelBackgroundColor
            // 
            this.labelBackgroundColor.AutoSize = true;
            this.labelBackgroundColor.ForeColor = System.Drawing.Color.Black;
            this.labelBackgroundColor.Location = new System.Drawing.Point(3, 130);
            this.labelBackgroundColor.Name = "labelBackgroundColor";
            this.labelBackgroundColor.Size = new System.Drawing.Size(150, 17);
            this.labelBackgroundColor.TabIndex = 3;
            this.labelBackgroundColor.Text = "Log background color:";
            // 
            // labelTextColor
            // 
            this.labelTextColor.AutoSize = true;
            this.labelTextColor.ForeColor = System.Drawing.Color.Black;
            this.labelTextColor.Location = new System.Drawing.Point(3, 92);
            this.labelTextColor.Name = "labelTextColor";
            this.labelTextColor.Size = new System.Drawing.Size(97, 17);
            this.labelTextColor.TabIndex = 2;
            this.labelTextColor.Text = "Log text color:";
            // 
            // checkboxSaveLog
            // 
            this.checkboxSaveLog.AutoSize = true;
            this.checkboxSaveLog.ForeColor = System.Drawing.Color.Black;
            this.checkboxSaveLog.Location = new System.Drawing.Point(6, 24);
            this.checkboxSaveLog.Name = "checkboxSaveLog";
            this.checkboxSaveLog.Size = new System.Drawing.Size(145, 21);
            this.checkboxSaveLog.TabIndex = 1;
            this.checkboxSaveLog.Text = "Save log to folder:";
            this.checkboxSaveLog.UseVisualStyleBackColor = true;
            this.checkboxSaveLog.CheckedChanged += new System.EventHandler(this.checkboxSaveLog_CheckedChanged);
            // 
            // textboxLogLocation
            // 
            this.textboxLogLocation.Location = new System.Drawing.Point(6, 51);
            this.textboxLogLocation.Name = "textboxLogLocation";
            this.textboxLogLocation.ReadOnly = true;
            this.textboxLogLocation.Size = new System.Drawing.Size(302, 22);
            this.textboxLogLocation.TabIndex = 0;
            // 
            // ExporterSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 555);
            this.Controls.Add(this.groupboxGeneral);
            this.Controls.Add(this.groupboxSkeleton);
            this.Controls.Add(this.groupboxMesh);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExporterSettings";
            this.Text = "Exporter Settings";
            ((System.ComponentModel.ISupportInitialize)(this.trackbarMeshResolution)).EndInit();
            this.groupboxMesh.ResumeLayout(false);
            this.groupboxMesh.PerformLayout();
            this.groupboxSkeleton.ResumeLayout(false);
            this.groupboxSkeleton.PerformLayout();
            this.groupboxGeneral.ResumeLayout(false);
            this.groupboxGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.TrackBar trackbarMeshResolution;
        private System.Windows.Forms.Label labelMeshResolution;
        private System.Windows.Forms.Label flavorText1;
        private System.Windows.Forms.Label flavorText2;
        private System.Windows.Forms.CheckBox checkboxFancyColors;
        private System.Windows.Forms.GroupBox groupboxMesh;
        private System.Windows.Forms.GroupBox groupboxSkeleton;
        private System.Windows.Forms.GroupBox groupboxGeneral;
        private System.Windows.Forms.Label labelBackgroundColor;
        private System.Windows.Forms.Label labelTextColor;
        private System.Windows.Forms.CheckBox checkboxSaveLog;
        private System.Windows.Forms.TextBox textboxLogLocation;
        private System.Windows.Forms.Button buttonChooseText;
        private System.Windows.Forms.Button buttonChooseBackground;
        private System.Windows.Forms.Label labelFuture;
        private System.Windows.Forms.CheckBox checkboxSoftBodies;
        private System.Windows.Forms.Button buttonChooseFolder;


    }
}