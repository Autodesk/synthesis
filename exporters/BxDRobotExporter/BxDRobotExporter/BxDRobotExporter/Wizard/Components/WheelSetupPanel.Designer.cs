namespace BxDRobotExporter.Wizard
{
    partial class WheelSetupPanel
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
            this.MainGroupBox = new System.Windows.Forms.GroupBox();
            this.FrictionComboBox = new System.Windows.Forms.ComboBox();
            this.FrictionLabel = new System.Windows.Forms.Label();
            this.WheelTypeLabel = new System.Windows.Forms.Label();
            this.WheelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.BackColor = System.Drawing.Color.Transparent;
            this.MainGroupBox.Controls.Add(this.FrictionComboBox);
            this.MainGroupBox.Controls.Add(this.FrictionLabel);
            this.MainGroupBox.Controls.Add(this.WheelTypeLabel);
            this.MainGroupBox.Controls.Add(this.WheelTypeComboBox);
            this.MainGroupBox.Location = new System.Drawing.Point(0, 0);
            this.MainGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.MainGroupBox.Size = new System.Drawing.Size(271, 87);
            this.MainGroupBox.TabIndex = 0;
            this.MainGroupBox.TabStop = false;
            this.MainGroupBox.Text = "node__.bxda";
            this.MainGroupBox.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            // 
            // FrictionComboBox
            // 
            this.FrictionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FrictionComboBox.FormattingEnabled = true;
            this.FrictionComboBox.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
            this.FrictionComboBox.Location = new System.Drawing.Point(108, 55);
            this.FrictionComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.FrictionComboBox.Name = "FrictionComboBox";
            this.FrictionComboBox.Size = new System.Drawing.Size(153, 24);
            this.FrictionComboBox.TabIndex = 3;
            this.FrictionComboBox.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            // 
            // FrictionLabel
            // 
            this.FrictionLabel.AutoSize = true;
            this.FrictionLabel.Location = new System.Drawing.Point(5, 59);
            this.FrictionLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FrictionLabel.Name = "FrictionLabel";
            this.FrictionLabel.Size = new System.Drawing.Size(102, 17);
            this.FrictionLabel.TabIndex = 2;
            this.FrictionLabel.Text = "Wheel Friction:";
            this.FrictionLabel.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            // 
            // WheelTypeLabel
            // 
            this.WheelTypeLabel.AutoSize = true;
            this.WheelTypeLabel.Location = new System.Drawing.Point(5, 25);
            this.WheelTypeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WheelTypeLabel.Name = "WheelTypeLabel";
            this.WheelTypeLabel.Size = new System.Drawing.Size(92, 17);
            this.WheelTypeLabel.TabIndex = 1;
            this.WheelTypeLabel.Text = "Wheel Type: ";
            this.WheelTypeLabel.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            // 
            // WheelTypeComboBox
            // 
            this.WheelTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WheelTypeComboBox.FormattingEnabled = true;
            this.WheelTypeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Omni",
            "Mecanum"});
            this.WheelTypeComboBox.Location = new System.Drawing.Point(108, 21);
            this.WheelTypeComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.WheelTypeComboBox.Name = "WheelTypeComboBox";
            this.WheelTypeComboBox.Size = new System.Drawing.Size(153, 24);
            this.WheelTypeComboBox.TabIndex = 0;
            this.WheelTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.WheelTypeComboBox_SelectedIndexChanged);
            this.WheelTypeComboBox.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(4, 87);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(263, 27);
            this.button1.TabIndex = 2;
            this.button1.Text = "Remove";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            this.button1.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            // 
            // WheelSetupPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.button1);
            this.Controls.Add(this.MainGroupBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "WheelSetupPanel";
            this.Size = new System.Drawing.Size(271, 124);
            this.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox MainGroupBox;
        private System.Windows.Forms.ComboBox WheelTypeComboBox;
        private System.Windows.Forms.Label WheelTypeLabel;
        private System.Windows.Forms.ComboBox FrictionComboBox;
        private System.Windows.Forms.Label FrictionLabel;
        private System.Windows.Forms.Button button1;
    }
}
