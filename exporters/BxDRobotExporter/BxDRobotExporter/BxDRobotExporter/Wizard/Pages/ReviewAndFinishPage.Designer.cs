namespace BxDRobotExporter.Wizard
{
    partial class ReviewAndFinishPage
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
            this.TitleLabel = new System.Windows.Forms.Label();
            this.SimulatorGroupBox = new System.Windows.Forms.GroupBox();
            this.FieldSelectComboBox = new System.Windows.Forms.ComboBox();
            this.FieldLabel = new System.Windows.Forms.Label();
            this.LauchSimCheckBox = new System.Windows.Forms.CheckBox();
            this.SimulatorGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TitleLabel.Location = new System.Drawing.Point(0, 0);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(371, 20);
            this.TitleLabel.TabIndex = 0;
            this.TitleLabel.Text = "Step 4: Review Information and Finish Export";
            // 
            // SimulatorGroupBox
            // 
            this.SimulatorGroupBox.Controls.Add(this.FieldSelectComboBox);
            this.SimulatorGroupBox.Controls.Add(this.FieldLabel);
            this.SimulatorGroupBox.Controls.Add(this.LauchSimCheckBox);
            this.SimulatorGroupBox.Location = new System.Drawing.Point(0, 580);
            this.SimulatorGroupBox.Name = "SimulatorGroupBox";
            this.SimulatorGroupBox.Size = new System.Drawing.Size(460, 46);
            this.SimulatorGroupBox.TabIndex = 1;
            this.SimulatorGroupBox.TabStop = false;
            this.SimulatorGroupBox.Text = "Simulator";
            // 
            // FieldSelectComboBox
            // 
            this.FieldSelectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FieldSelectComboBox.Enabled = false;
            this.FieldSelectComboBox.FormattingEnabled = true;
            this.FieldSelectComboBox.Location = new System.Drawing.Point(215, 19);
            this.FieldSelectComboBox.Name = "FieldSelectComboBox";
            this.FieldSelectComboBox.Size = new System.Drawing.Size(239, 21);
            this.FieldSelectComboBox.TabIndex = 2;
            // 
            // FieldLabel
            // 
            this.FieldLabel.AutoSize = true;
            this.FieldLabel.Location = new System.Drawing.Point(141, 22);
            this.FieldLabel.Name = "FieldLabel";
            this.FieldLabel.Size = new System.Drawing.Size(68, 13);
            this.FieldLabel.TabIndex = 1;
            this.FieldLabel.Text = "Select Field: ";
            // 
            // LauchSimCheckBox
            // 
            this.LauchSimCheckBox.AutoSize = true;
            this.LauchSimCheckBox.Location = new System.Drawing.Point(6, 21);
            this.LauchSimCheckBox.Name = "LauchSimCheckBox";
            this.LauchSimCheckBox.Size = new System.Drawing.Size(110, 17);
            this.LauchSimCheckBox.TabIndex = 0;
            this.LauchSimCheckBox.Text = "Launch Synthesis";
            this.LauchSimCheckBox.UseVisualStyleBackColor = true;
            this.LauchSimCheckBox.CheckedChanged += new System.EventHandler(this.LauchSimCheckBox_CheckedChanged);
            // 
            // ReviewAndFinishPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.SimulatorGroupBox);
            this.Controls.Add(this.TitleLabel);
            this.Name = "ReviewAndFinishPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.SimulatorGroupBox.ResumeLayout(false);
            this.SimulatorGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.GroupBox SimulatorGroupBox;
        private System.Windows.Forms.CheckBox LauchSimCheckBox;
        private System.Windows.Forms.ComboBox FieldSelectComboBox;
        private System.Windows.Forms.Label FieldLabel;
    }
}
