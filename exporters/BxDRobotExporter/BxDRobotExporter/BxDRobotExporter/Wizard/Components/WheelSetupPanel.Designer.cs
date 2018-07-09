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
            this.ConfigLayout = new System.Windows.Forms.TableLayoutPanel();
            this.WheelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.FrictionComboBox = new System.Windows.Forms.ComboBox();
            this.WheelTypeLabel = new System.Windows.Forms.Label();
            this.FrictionLabel = new System.Windows.Forms.Label();
            this.removeButton = new System.Windows.Forms.Button();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.MainGroupBox.SuspendLayout();
            this.ConfigLayout.SuspendLayout();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.AllowDrop = true;
            this.MainGroupBox.AutoSize = true;
            this.MainGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainGroupBox.Controls.Add(this.ConfigLayout);
            this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainGroupBox.Location = new System.Drawing.Point(3, 3);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = new System.Drawing.Size(194, 73);
            this.MainGroupBox.TabIndex = 0;
            this.MainGroupBox.TabStop = false;
            this.MainGroupBox.Text = "node__.bxda";
            // 
            // ConfigLayout
            // 
            this.ConfigLayout.AutoSize = true;
            this.ConfigLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ConfigLayout.ColumnCount = 2;
            this.ConfigLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.ConfigLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.ConfigLayout.Controls.Add(this.WheelTypeComboBox, 1, 0);
            this.ConfigLayout.Controls.Add(this.FrictionComboBox, 1, 1);
            this.ConfigLayout.Controls.Add(this.WheelTypeLabel, 0, 0);
            this.ConfigLayout.Controls.Add(this.FrictionLabel, 0, 1);
            this.ConfigLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.ConfigLayout.Location = new System.Drawing.Point(3, 16);
            this.ConfigLayout.Name = "ConfigLayout";
            this.ConfigLayout.RowCount = 2;
            this.ConfigLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.ConfigLayout.Size = new System.Drawing.Size(188, 54);
            this.ConfigLayout.TabIndex = 4;
            // 
            // WheelTypeComboBox
            // 
            this.WheelTypeComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.WheelTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WheelTypeComboBox.FormattingEnabled = true;
            this.WheelTypeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Omni",
            "Mecanum"});
            this.WheelTypeComboBox.Location = new System.Drawing.Point(97, 3);
            this.WheelTypeComboBox.Name = "WheelTypeComboBox";
            this.WheelTypeComboBox.Size = new System.Drawing.Size(88, 21);
            this.WheelTypeComboBox.TabIndex = 0;
            this.WheelTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.WheelTypeComboBox_SelectedIndexChanged);
            this.WheelTypeComboBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.WheelTypeComboBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // FrictionComboBox
            // 
            this.FrictionComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FrictionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FrictionComboBox.FormattingEnabled = true;
            this.FrictionComboBox.Items.AddRange(new object[] {
            "Low",
            "Medium",
            "High"});
            this.FrictionComboBox.Location = new System.Drawing.Point(97, 30);
            this.FrictionComboBox.Name = "FrictionComboBox";
            this.FrictionComboBox.Size = new System.Drawing.Size(88, 21);
            this.FrictionComboBox.TabIndex = 3;
            this.FrictionComboBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.FrictionComboBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // WheelTypeLabel
            // 
            this.WheelTypeLabel.AutoSize = true;
            this.WheelTypeLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.WheelTypeLabel.Location = new System.Drawing.Point(3, 3);
            this.WheelTypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.WheelTypeLabel.Name = "WheelTypeLabel";
            this.WheelTypeLabel.Size = new System.Drawing.Size(65, 21);
            this.WheelTypeLabel.TabIndex = 1;
            this.WheelTypeLabel.Text = "Wheel Type";
            this.WheelTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.WheelTypeLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.WheelTypeLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // FrictionLabel
            // 
            this.FrictionLabel.AutoSize = true;
            this.FrictionLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.FrictionLabel.Location = new System.Drawing.Point(3, 30);
            this.FrictionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.FrictionLabel.Name = "FrictionLabel";
            this.FrictionLabel.Size = new System.Drawing.Size(75, 21);
            this.FrictionLabel.TabIndex = 2;
            this.FrictionLabel.Text = "Wheel Friction";
            this.FrictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.FrictionLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.FrictionLabel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // removeButton
            // 
            this.removeButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.removeButton.Location = new System.Drawing.Point(3, 82);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(194, 22);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.remove_Click);
            this.removeButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.removeButton.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 1;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Controls.Add(this.MainGroupBox, 0, 0);
            this.MainLayout.Controls.Add(this.removeButton, 0, 1);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 2;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.Size = new System.Drawing.Size(200, 107);
            this.MainLayout.TabIndex = 3;
            // 
            // WheelSetupPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.MainLayout);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(200, 0);
            this.Name = "WheelSetupPanel";
            this.Size = new System.Drawing.Size(200, 107);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.WheelSetupPanel_MouseUp);
            this.MainGroupBox.ResumeLayout(false);
            this.MainGroupBox.PerformLayout();
            this.ConfigLayout.ResumeLayout(false);
            this.ConfigLayout.PerformLayout();
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox MainGroupBox;
        private System.Windows.Forms.ComboBox WheelTypeComboBox;
        private System.Windows.Forms.Label WheelTypeLabel;
        private System.Windows.Forms.ComboBox FrictionComboBox;
        private System.Windows.Forms.Label FrictionLabel;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.TableLayoutPanel ConfigLayout;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
    }
}
