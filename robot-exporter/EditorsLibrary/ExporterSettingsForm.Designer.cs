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
            this.groupboxSkeleton = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.checkboxOCL = new System.Windows.Forms.CheckBox();
            this.groupboxGeneral = new System.Windows.Forms.GroupBox();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.buttonChooseText = new System.Windows.Forms.Button();
            this.buttonChooseBackground = new System.Windows.Forms.Button();
            this.labelBackgroundColor = new System.Windows.Forms.Label();
            this.labelTextColor = new System.Windows.Forms.Label();
            this.checkboxSaveLog = new System.Windows.Forms.CheckBox();
            this.textboxLogLocation = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupboxSkeleton.SuspendLayout();
            this.groupboxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(15, 273);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(160, 36);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(210, 273);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(160, 36);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // groupboxSkeleton
            // 
            this.groupboxSkeleton.Controls.Add(this.label1);
            this.groupboxSkeleton.Controls.Add(this.label2);
            this.groupboxSkeleton.Controls.Add(this.checkboxOCL);
            this.groupboxSkeleton.ForeColor = System.Drawing.Color.Gray;
            this.groupboxSkeleton.Location = new System.Drawing.Point(15, 178);
            this.groupboxSkeleton.Name = "groupboxSkeleton";
            this.groupboxSkeleton.Size = new System.Drawing.Size(355, 89);
            this.groupboxSkeleton.TabIndex = 12;
            this.groupboxSkeleton.TabStop = false;
            this.groupboxSkeleton.Text = "Export options";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(3, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(340, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Make sure that your GPU drivers are up to date before using this option";
            // 
            // checkboxOCL
            // 
            this.checkboxOCL.AutoSize = true;
            this.checkboxOCL.ForeColor = System.Drawing.Color.Black;
            this.checkboxOCL.Location = new System.Drawing.Point(6, 21);
            this.checkboxOCL.Name = "checkboxOCL";
            this.checkboxOCL.Size = new System.Drawing.Size(225, 21);
            this.checkboxOCL.TabIndex = 0;
            this.checkboxOCL.Text = "Enable OpenCL (Experimental)";
            this.checkboxOCL.UseVisualStyleBackColor = true;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(3, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Actual performance gains may vary";
            // 
            // ExporterSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 319);
            this.Controls.Add(this.groupboxGeneral);
            this.Controls.Add(this.groupboxSkeleton);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExporterSettingsForm";
            this.Text = "Exporter Settings";
            this.groupboxSkeleton.ResumeLayout(false);
            this.groupboxSkeleton.PerformLayout();
            this.groupboxGeneral.ResumeLayout(false);
            this.groupboxGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupboxSkeleton;
        private System.Windows.Forms.GroupBox groupboxGeneral;
        private System.Windows.Forms.Label labelBackgroundColor;
        private System.Windows.Forms.Label labelTextColor;
        private System.Windows.Forms.CheckBox checkboxSaveLog;
        private System.Windows.Forms.TextBox textboxLogLocation;
        private System.Windows.Forms.Button buttonChooseText;
        private System.Windows.Forms.Button buttonChooseBackground;
        private System.Windows.Forms.CheckBox checkboxOCL;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;


    }
}