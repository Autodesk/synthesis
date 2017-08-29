namespace BxDRobotExporter.Wizard
{
    partial class DefineWheelsPage
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
            this.DefineWheelsTitleLabel = new System.Windows.Forms.Label();
            this.NodeCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.DefineWheelsInstruction1 = new System.Windows.Forms.Label();
            this.WheelNodeGroupBox = new System.Windows.Forms.GroupBox();
            this.WheelProgressLabel = new System.Windows.Forms.Label();
            this.WheelPropertiesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.WarningLabel = new System.Windows.Forms.Label();
            this.WheelNodeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // DefineWheelsTitleLabel
            // 
            this.DefineWheelsTitleLabel.AutoSize = true;
            this.DefineWheelsTitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefineWheelsTitleLabel.Location = new System.Drawing.Point(-5, 0);
            this.DefineWheelsTitleLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DefineWheelsTitleLabel.Name = "DefineWheelsTitleLabel";
            this.DefineWheelsTitleLabel.Size = new System.Drawing.Size(280, 25);
            this.DefineWheelsTitleLabel.TabIndex = 0;
            this.DefineWheelsTitleLabel.Text = "Step 2: Define Your Wheels";
            // 
            // NodeCheckedListBox
            // 
            this.NodeCheckedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NodeCheckedListBox.FormattingEnabled = true;
            this.NodeCheckedListBox.Location = new System.Drawing.Point(0, 23);
            this.NodeCheckedListBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.NodeCheckedListBox.Name = "NodeCheckedListBox";
            this.NodeCheckedListBox.Size = new System.Drawing.Size(399, 106);
            this.NodeCheckedListBox.TabIndex = 1;
            // 
            // DefineWheelsInstruction1
            // 
            this.DefineWheelsInstruction1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DefineWheelsInstruction1.Location = new System.Drawing.Point(408, 23);
            this.DefineWheelsInstruction1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.DefineWheelsInstruction1.Name = "DefineWheelsInstruction1";
            this.DefineWheelsInstruction1.Size = new System.Drawing.Size(189, 69);
            this.DefineWheelsInstruction1.TabIndex = 2;
            this.DefineWheelsInstruction1.Text = "Check the boxes of all of the nodes which are drive wheels. Select a node to focu" +
    "s on it in Inventor.";
            // 
            // WheelNodeGroupBox
            // 
            this.WheelNodeGroupBox.Controls.Add(this.WheelProgressLabel);
            this.WheelNodeGroupBox.Controls.Add(this.NodeCheckedListBox);
            this.WheelNodeGroupBox.Controls.Add(this.DefineWheelsInstruction1);
            this.WheelNodeGroupBox.Location = new System.Drawing.Point(0, 87);
            this.WheelNodeGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WheelNodeGroupBox.Name = "WheelNodeGroupBox";
            this.WheelNodeGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WheelNodeGroupBox.Size = new System.Drawing.Size(613, 146);
            this.WheelNodeGroupBox.TabIndex = 3;
            this.WheelNodeGroupBox.TabStop = false;
            this.WheelNodeGroupBox.Text = "Select Wheel Nodes";
            // 
            // WheelProgressLabel
            // 
            this.WheelProgressLabel.AutoSize = true;
            this.WheelProgressLabel.Location = new System.Drawing.Point(408, 123);
            this.WheelProgressLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WheelProgressLabel.Name = "WheelProgressLabel";
            this.WheelProgressLabel.Size = new System.Drawing.Size(46, 17);
            this.WheelProgressLabel.TabIndex = 3;
            this.WheelProgressLabel.Text = "label1";
            // 
            // WheelPropertiesPanel
            // 
            this.WheelPropertiesPanel.AutoScroll = true;
            this.WheelPropertiesPanel.Location = new System.Drawing.Point(0, 241);
            this.WheelPropertiesPanel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.WheelPropertiesPanel.Name = "WheelPropertiesPanel";
            this.WheelPropertiesPanel.Size = new System.Drawing.Size(613, 562);
            this.WheelPropertiesPanel.TabIndex = 4;
            // 
            // WarningLabel
            // 
            this.WarningLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.WarningLabel.ForeColor = System.Drawing.Color.Red;
            this.WarningLabel.Location = new System.Drawing.Point(5, 25);
            this.WarningLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.WarningLabel.Name = "WarningLabel";
            this.WarningLabel.Size = new System.Drawing.Size(592, 59);
            this.WarningLabel.TabIndex = 5;
            // 
            // DefineWheelsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WarningLabel);
            this.Controls.Add(this.WheelPropertiesPanel);
            this.Controls.Add(this.WheelNodeGroupBox);
            this.Controls.Add(this.DefineWheelsTitleLabel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DefineWheelsPage";
            this.Size = new System.Drawing.Size(613, 804);
            this.WheelNodeGroupBox.ResumeLayout(false);
            this.WheelNodeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineWheelsTitleLabel;
        private System.Windows.Forms.CheckedListBox NodeCheckedListBox;
        private System.Windows.Forms.Label DefineWheelsInstruction1;
        private System.Windows.Forms.GroupBox WheelNodeGroupBox;
        private System.Windows.Forms.FlowLayoutPanel WheelPropertiesPanel;
        private System.Windows.Forms.Label WheelProgressLabel;
        private System.Windows.Forms.Label WarningLabel;
    }
}
