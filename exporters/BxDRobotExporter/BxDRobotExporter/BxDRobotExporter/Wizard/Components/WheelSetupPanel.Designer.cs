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
            this.WheelTypeLabel = new System.Windows.Forms.Label();
            this.WheelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.FrictionLabel = new System.Windows.Forms.Label();
            this.FrictionComboBox = new System.Windows.Forms.ComboBox();
            this.backgroundLabel = new System.Windows.Forms.Label();
            this.removeButton = new System.Windows.Forms.Button();
            this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.AllowDrop = true;
            this.MainGroupBox.Controls.Add(this.WheelTypeLabel);
            this.MainGroupBox.Controls.Add(this.WheelTypeComboBox);
            this.MainGroupBox.Controls.Add(this.FrictionLabel);
            this.MainGroupBox.Controls.Add(this.FrictionComboBox);
            this.MainGroupBox.Controls.Add(this.backgroundLabel);
            this.MainGroupBox.Location = new System.Drawing.Point(0, 0);
            this.MainGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.MainGroupBox.Size = new System.Drawing.Size(271, 87);
            this.MainGroupBox.TabIndex = 0;
            this.MainGroupBox.TabStop = false;
            this.MainGroupBox.Text = "node__.bxda";
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
            this.WheelTypeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.WheelTypeLabel.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.WheelTypeLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
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
            this.WheelTypeComboBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.WheelTypeComboBox.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.WheelTypeComboBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
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
            this.FrictionLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.FrictionLabel.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.FrictionLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
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
            this.FrictionComboBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.FrictionComboBox.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.FrictionComboBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // backgroundLabel
            // 
            this.backgroundLabel.BackColor = System.Drawing.SystemColors.Control;
            this.backgroundLabel.Location = new System.Drawing.Point(0, 0);
            this.backgroundLabel.MinimumSize = new System.Drawing.Size(271, 140);
            this.backgroundLabel.Name = "backgroundLabel";
            this.backgroundLabel.Size = new System.Drawing.Size(271, 140);
            this.backgroundLabel.TabIndex = 4;
            this.backgroundLabel.Tag = "";
            this.backgroundLabel.Text = "node__.bxda";
            this.backgroundLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.backgroundLabel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.backgroundLabel_MouseMove);
            this.backgroundLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(4, 87);
            this.removeButton.Margin = new System.Windows.Forms.Padding(4);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(263, 27);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.remove_Click);
            this.removeButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.removeButton.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.removeButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // WheelSetupPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.removeButton);
            this.Controls.Add(this.MainGroupBox);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "WheelSetupPanel";
            this.Size = new System.Drawing.Size(271, 140);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.MouseEnter += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.MouseHover += new System.EventHandler(this.WheelSetupPanel_MouseHover);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
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
        private System.Windows.Forms.Label backgroundLabel;
        private System.Windows.Forms.Button removeButton;
    }
}
