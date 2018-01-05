namespace BxDRobotExporter.Forms
{
    partial class ChooseExportModeForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseExportModeForm));
            this.OneClickExportButton = new System.Windows.Forms.Button();
            this.GuidedExportButton = new System.Windows.Forms.Button();
            this.AdvancedExportButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ExportModeInfoLabel = new System.Windows.Forms.Label();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // OneClickExportButton
            // 
            this.OneClickExportButton.Location = new System.Drawing.Point(12, 12);
            this.OneClickExportButton.Name = "OneClickExportButton";
            this.OneClickExportButton.Size = new System.Drawing.Size(150, 50);
            this.OneClickExportButton.TabIndex = 0;
            this.OneClickExportButton.Text = "One Click Export";
            this.OneClickExportButton.UseVisualStyleBackColor = true;
            this.OneClickExportButton.Click += new System.EventHandler(this.OneClickExportButton_Click);
            // 
            // GuidedExportButton
            // 
            this.GuidedExportButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GuidedExportButton.Location = new System.Drawing.Point(12, 68);
            this.GuidedExportButton.Name = "GuidedExportButton";
            this.GuidedExportButton.Size = new System.Drawing.Size(150, 50);
            this.GuidedExportButton.TabIndex = 1;
            this.GuidedExportButton.Text = "Guided Export (Recommended)";
            this.GuidedExportButton.UseVisualStyleBackColor = true;
            this.GuidedExportButton.Click += new System.EventHandler(this.GuidedExportButton_Click);
            // 
            // AdvancedExportButton
            // 
            this.AdvancedExportButton.Location = new System.Drawing.Point(12, 124);
            this.AdvancedExportButton.Name = "AdvancedExportButton";
            this.AdvancedExportButton.Size = new System.Drawing.Size(150, 50);
            this.AdvancedExportButton.TabIndex = 2;
            this.AdvancedExportButton.Text = "Advanced Export";
            this.AdvancedExportButton.UseVisualStyleBackColor = true;
            this.AdvancedExportButton.Click += new System.EventHandler(this.AdvancedExportButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ExportModeInfoLabel);
            this.groupBox1.Location = new System.Drawing.Point(169, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(205, 162);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Export Mode Info";
            // 
            // ExportModeInfoLabel
            // 
            this.ExportModeInfoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ExportModeInfoLabel.Location = new System.Drawing.Point(6, 16);
            this.ExportModeInfoLabel.Name = "ExportModeInfoLabel";
            this.ExportModeInfoLabel.Size = new System.Drawing.Size(193, 143);
            this.ExportModeInfoLabel.TabIndex = 0;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(12, 179);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(362, 23);
            this.ButtonCancel.TabIndex = 4;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ChooseExportModeForm
            // 
            this.AcceptButton = this.ButtonCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(386, 214);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.AdvancedExportButton);
            this.Controls.Add(this.GuidedExportButton);
            this.Controls.Add(this.OneClickExportButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChooseExportModeForm";
            this.Text = "Choose Export Mode";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button OneClickExportButton;
        private System.Windows.Forms.Button GuidedExportButton;
        private System.Windows.Forms.Button AdvancedExportButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label ExportModeInfoLabel;
        private System.Windows.Forms.Button ButtonCancel;
    }
}