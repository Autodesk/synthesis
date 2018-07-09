namespace BxDRobotExporter.Wizard
{
    partial class DefinePartPanel
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
            this.NodeGroupBox = new System.Windows.Forms.GroupBox();
            this.MainTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DriverLayout = new System.Windows.Forms.TableLayoutPanel();
            this.SelectDriverLabel = new System.Windows.Forms.Label();
            this.DriverComboBox = new System.Windows.Forms.ComboBox();
            this.JointLimitGroupBox = new System.Windows.Forms.GroupBox();
            this.TotalFreedomLabel = new System.Windows.Forms.Label();
            this.FreedomFactorLabel = new System.Windows.Forms.Label();
            this.UpperLimitUpDown = new System.Windows.Forms.NumericUpDown();
            this.UpperLimitLabel = new System.Windows.Forms.Label();
            this.LowerLimitLabel = new System.Windows.Forms.Label();
            this.LowerLimitUpDown = new System.Windows.Forms.NumericUpDown();
            this.PortsGroupBox = new System.Windows.Forms.GroupBox();
            this.PortTwoUpDown = new System.Windows.Forms.NumericUpDown();
            this.PortTwoLabel = new System.Windows.Forms.Label();
            this.PortOneLabel = new System.Windows.Forms.Label();
            this.PortOneUpDown = new System.Windows.Forms.NumericUpDown();
            this.MetaTabControl = new System.Windows.Forms.TabControl();
            this.PneumaticTab = new System.Windows.Forms.TabPage();
            this.PneumaticLayout = new System.Windows.Forms.TableLayoutPanel();
            this.PressureLabel = new System.Windows.Forms.Label();
            this.DiameterLabel = new System.Windows.Forms.Label();
            this.PneumaticPressureComboBox = new System.Windows.Forms.ComboBox();
            this.PneumaticDiameterComboBox = new System.Windows.Forms.ComboBox();
            this.NodeGroupBox.SuspendLayout();
            this.MainTableLayout.SuspendLayout();
            this.DriverLayout.SuspendLayout();
            this.JointLimitGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpperLimitUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.LowerLimitUpDown)).BeginInit();
            this.PortsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortTwoUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortOneUpDown)).BeginInit();
            this.MetaTabControl.SuspendLayout();
            this.PneumaticTab.SuspendLayout();
            this.PneumaticLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // NodeGroupBox
            // 
            this.NodeGroupBox.AutoSize = true;
            this.NodeGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.NodeGroupBox.Controls.Add(this.MainTableLayout);
            this.NodeGroupBox.Location = new System.Drawing.Point(0, 0);
            this.NodeGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.NodeGroupBox.Name = "NodeGroupBox";
            this.NodeGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.NodeGroupBox.Size = new System.Drawing.Size(556, 312);
            this.NodeGroupBox.TabIndex = 0;
            this.NodeGroupBox.TabStop = false;
            this.NodeGroupBox.Text = "Empty";
            // 
            // MainTableLayout
            // 
            this.MainTableLayout.AutoSize = true;
            this.MainTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainTableLayout.ColumnCount = 2;
            this.MainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainTableLayout.Controls.Add(this.DriverLayout, 0, 0);
            this.MainTableLayout.Controls.Add(this.JointLimitGroupBox, 0, 2);
            this.MainTableLayout.Controls.Add(this.PortsGroupBox, 0, 1);
            this.MainTableLayout.Controls.Add(this.MetaTabControl, 1, 2);
            this.MainTableLayout.Location = new System.Drawing.Point(8, 23);
            this.MainTableLayout.Margin = new System.Windows.Forms.Padding(4);
            this.MainTableLayout.Name = "MainTableLayout";
            this.MainTableLayout.RowCount = 3;
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.Size = new System.Drawing.Size(540, 266);
            this.MainTableLayout.TabIndex = 14;
            // 
            // DriverLayout
            // 
            this.DriverLayout.AutoSize = true;
            this.DriverLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DriverLayout.ColumnCount = 2;
            this.MainTableLayout.SetColumnSpan(this.DriverLayout, 2);
            this.DriverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DriverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DriverLayout.Controls.Add(this.SelectDriverLabel, 0, 0);
            this.DriverLayout.Controls.Add(this.DriverComboBox, 1, 0);
            this.DriverLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriverLayout.Location = new System.Drawing.Point(0, 0);
            this.DriverLayout.Margin = new System.Windows.Forms.Padding(0);
            this.DriverLayout.Name = "DriverLayout";
            this.DriverLayout.RowCount = 1;
            this.DriverLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.DriverLayout.Size = new System.Drawing.Size(540, 33);
            this.DriverLayout.TabIndex = 0;
            // 
            // SelectDriverLabel
            // 
            this.SelectDriverLabel.AutoSize = true;
            this.SelectDriverLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.SelectDriverLabel.Location = new System.Drawing.Point(4, 4);
            this.SelectDriverLabel.Margin = new System.Windows.Forms.Padding(4);
            this.SelectDriverLabel.Name = "SelectDriverLabel";
            this.SelectDriverLabel.Size = new System.Drawing.Size(93, 25);
            this.SelectDriverLabel.TabIndex = 1;
            this.SelectDriverLabel.Text = "Select Driver:";
            this.SelectDriverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DriverComboBox
            // 
            this.DriverComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriverComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DriverComboBox.FormattingEnabled = true;
            this.DriverComboBox.Items.AddRange(new object[] {
            "No Driver",
            "Motor",
            "Servo",
            "Bumper Pneumatic",
            "Relay Pneumatic",
            "Dual Motor"});
            this.DriverComboBox.Location = new System.Drawing.Point(105, 4);
            this.DriverComboBox.Margin = new System.Windows.Forms.Padding(4);
            this.DriverComboBox.Name = "DriverComboBox";
            this.DriverComboBox.Size = new System.Drawing.Size(431, 24);
            this.DriverComboBox.TabIndex = 2;
            this.DriverComboBox.SelectedIndexChanged += new System.EventHandler(this.DriverComboBox_SelectedIndexChanged);
            // 
            // JointLimitGroupBox
            // 
            this.JointLimitGroupBox.Controls.Add(this.TotalFreedomLabel);
            this.JointLimitGroupBox.Controls.Add(this.FreedomFactorLabel);
            this.JointLimitGroupBox.Controls.Add(this.UpperLimitUpDown);
            this.JointLimitGroupBox.Controls.Add(this.UpperLimitLabel);
            this.JointLimitGroupBox.Controls.Add(this.LowerLimitLabel);
            this.JointLimitGroupBox.Controls.Add(this.LowerLimitUpDown);
            this.JointLimitGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.JointLimitGroupBox.Location = new System.Drawing.Point(4, 108);
            this.JointLimitGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.JointLimitGroupBox.Name = "JointLimitGroupBox";
            this.JointLimitGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.JointLimitGroupBox.Size = new System.Drawing.Size(262, 102);
            this.JointLimitGroupBox.TabIndex = 13;
            this.JointLimitGroupBox.TabStop = false;
            this.JointLimitGroupBox.Text = "Joint Limits";
            // 
            // TotalFreedomLabel
            // 
            this.TotalFreedomLabel.AutoSize = true;
            this.TotalFreedomLabel.Location = new System.Drawing.Point(157, 34);
            this.TotalFreedomLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.TotalFreedomLabel.Name = "TotalFreedomLabel";
            this.TotalFreedomLabel.Size = new System.Drawing.Size(62, 17);
            this.TotalFreedomLabel.TabIndex = 5;
            this.TotalFreedomLabel.Text = "0.0 units";
            // 
            // FreedomFactorLabel
            // 
            this.FreedomFactorLabel.AutoSize = true;
            this.FreedomFactorLabel.Location = new System.Drawing.Point(157, 20);
            this.FreedomFactorLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.FreedomFactorLabel.Name = "FreedomFactorLabel";
            this.FreedomFactorLabel.Size = new System.Drawing.Size(68, 17);
            this.FreedomFactorLabel.TabIndex = 4;
            this.FreedomFactorLabel.Text = "Freedom:";
            // 
            // UpperLimitUpDown
            // 
            this.UpperLimitUpDown.Location = new System.Drawing.Point(96, 32);
            this.UpperLimitUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.UpperLimitUpDown.Name = "UpperLimitUpDown";
            this.UpperLimitUpDown.Size = new System.Drawing.Size(61, 22);
            this.UpperLimitUpDown.TabIndex = 3;
            this.UpperLimitUpDown.ValueChanged += new System.EventHandler(this.UpperLimitUpDown_ValueChanged);
            // 
            // UpperLimitLabel
            // 
            this.UpperLimitLabel.AutoSize = true;
            this.UpperLimitLabel.Location = new System.Drawing.Point(8, 34);
            this.UpperLimitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.UpperLimitLabel.Name = "UpperLimitLabel";
            this.UpperLimitLabel.Size = new System.Drawing.Size(80, 17);
            this.UpperLimitLabel.TabIndex = 2;
            this.UpperLimitLabel.Text = "Upper Limit";
            // 
            // LowerLimitLabel
            // 
            this.LowerLimitLabel.AutoSize = true;
            this.LowerLimitLabel.Location = new System.Drawing.Point(8, 68);
            this.LowerLimitLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.LowerLimitLabel.Name = "LowerLimitLabel";
            this.LowerLimitLabel.Size = new System.Drawing.Size(79, 17);
            this.LowerLimitLabel.TabIndex = 1;
            this.LowerLimitLabel.Text = "Lower Limit";
            // 
            // LowerLimitUpDown
            // 
            this.LowerLimitUpDown.Location = new System.Drawing.Point(96, 65);
            this.LowerLimitUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.LowerLimitUpDown.Name = "LowerLimitUpDown";
            this.LowerLimitUpDown.Size = new System.Drawing.Size(60, 22);
            this.LowerLimitUpDown.TabIndex = 0;
            this.LowerLimitUpDown.ValueChanged += new System.EventHandler(this.LowerLimitUpDown_ValueChanged);
            // 
            // PortsGroupBox
            // 
            this.MainTableLayout.SetColumnSpan(this.PortsGroupBox, 2);
            this.PortsGroupBox.Controls.Add(this.PortTwoUpDown);
            this.PortsGroupBox.Controls.Add(this.PortTwoLabel);
            this.PortsGroupBox.Controls.Add(this.PortOneLabel);
            this.PortsGroupBox.Controls.Add(this.PortOneUpDown);
            this.PortsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortsGroupBox.Location = new System.Drawing.Point(4, 37);
            this.PortsGroupBox.Margin = new System.Windows.Forms.Padding(4);
            this.PortsGroupBox.Name = "PortsGroupBox";
            this.PortsGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.PortsGroupBox.Size = new System.Drawing.Size(532, 63);
            this.PortsGroupBox.TabIndex = 6;
            this.PortsGroupBox.TabStop = false;
            this.PortsGroupBox.Text = "Ports";
            // 
            // PortTwoUpDown
            // 
            this.PortTwoUpDown.Location = new System.Drawing.Point(196, 23);
            this.PortTwoUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.PortTwoUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.PortTwoUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.PortTwoUpDown.Name = "PortTwoUpDown";
            this.PortTwoUpDown.Size = new System.Drawing.Size(63, 22);
            this.PortTwoUpDown.TabIndex = 3;
            this.PortTwoUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // PortTwoLabel
            // 
            this.PortTwoLabel.AutoSize = true;
            this.PortTwoLabel.Location = new System.Drawing.Point(137, 26);
            this.PortTwoLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PortTwoLabel.Name = "PortTwoLabel";
            this.PortTwoLabel.Size = new System.Drawing.Size(50, 17);
            this.PortTwoLabel.TabIndex = 2;
            this.PortTwoLabel.Text = "Port 2:";
            // 
            // PortOneLabel
            // 
            this.PortOneLabel.AutoSize = true;
            this.PortOneLabel.Location = new System.Drawing.Point(8, 26);
            this.PortOneLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PortOneLabel.Name = "PortOneLabel";
            this.PortOneLabel.Size = new System.Drawing.Size(50, 17);
            this.PortOneLabel.TabIndex = 1;
            this.PortOneLabel.Text = "Port 1:";
            // 
            // PortOneUpDown
            // 
            this.PortOneUpDown.Location = new System.Drawing.Point(67, 23);
            this.PortOneUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.PortOneUpDown.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.PortOneUpDown.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.PortOneUpDown.Name = "PortOneUpDown";
            this.PortOneUpDown.Size = new System.Drawing.Size(63, 22);
            this.PortOneUpDown.TabIndex = 0;
            this.PortOneUpDown.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // MetaTabControl
            // 
            this.MetaTabControl.Controls.Add(this.PneumaticTab);
            this.MetaTabControl.Dock = System.Windows.Forms.DockStyle.Top;
            this.MetaTabControl.Location = new System.Drawing.Point(273, 106);
            this.MetaTabControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MetaTabControl.Name = "MetaTabControl";
            this.MetaTabControl.SelectedIndex = 0;
            this.MetaTabControl.Size = new System.Drawing.Size(264, 158);
            this.MetaTabControl.TabIndex = 12;
            this.MetaTabControl.Visible = false;
            // 
            // PneumaticTab
            // 
            this.PneumaticTab.Controls.Add(this.PneumaticLayout);
            this.PneumaticTab.Location = new System.Drawing.Point(4, 25);
            this.PneumaticTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PneumaticTab.Name = "PneumaticTab";
            this.PneumaticTab.Size = new System.Drawing.Size(256, 129);
            this.PneumaticTab.TabIndex = 1;
            this.PneumaticTab.Text = "Pneumatic";
            this.PneumaticTab.UseVisualStyleBackColor = true;
            // 
            // PneumaticLayout
            // 
            this.PneumaticLayout.AutoSize = true;
            this.PneumaticLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PneumaticLayout.ColumnCount = 1;
            this.PneumaticLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PneumaticLayout.Controls.Add(this.PressureLabel, 0, 2);
            this.PneumaticLayout.Controls.Add(this.DiameterLabel, 0, 0);
            this.PneumaticLayout.Controls.Add(this.PneumaticPressureComboBox, 0, 3);
            this.PneumaticLayout.Controls.Add(this.PneumaticDiameterComboBox, 0, 1);
            this.PneumaticLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.PneumaticLayout.Location = new System.Drawing.Point(0, 0);
            this.PneumaticLayout.Margin = new System.Windows.Forms.Padding(4);
            this.PneumaticLayout.Name = "PneumaticLayout";
            this.PneumaticLayout.RowCount = 4;
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.Size = new System.Drawing.Size(256, 108);
            this.PneumaticLayout.TabIndex = 15;
            // 
            // PressureLabel
            // 
            this.PressureLabel.AutoSize = true;
            this.PressureLabel.Location = new System.Drawing.Point(4, 58);
            this.PressureLabel.Margin = new System.Windows.Forms.Padding(4);
            this.PressureLabel.Name = "PressureLabel";
            this.PressureLabel.Size = new System.Drawing.Size(65, 17);
            this.PressureLabel.TabIndex = 13;
            this.PressureLabel.Text = "Pressure";
            // 
            // DiameterLabel
            // 
            this.DiameterLabel.AutoSize = true;
            this.DiameterLabel.Location = new System.Drawing.Point(4, 4);
            this.DiameterLabel.Margin = new System.Windows.Forms.Padding(4);
            this.DiameterLabel.Name = "DiameterLabel";
            this.DiameterLabel.Size = new System.Drawing.Size(116, 17);
            this.DiameterLabel.TabIndex = 9;
            this.DiameterLabel.Text = "Internal Diameter";
            // 
            // PneumaticPressureComboBox
            // 
            this.PneumaticPressureComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.PneumaticPressureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PneumaticPressureComboBox.FormattingEnabled = true;
            this.PneumaticPressureComboBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PneumaticPressureComboBox.Items.AddRange(new object[] {
            "60 psi",
            "20 psi",
            "10 psi"});
            this.PneumaticPressureComboBox.Location = new System.Drawing.Point(3, 81);
            this.PneumaticPressureComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PneumaticPressureComboBox.Name = "PneumaticPressureComboBox";
            this.PneumaticPressureComboBox.Size = new System.Drawing.Size(250, 24);
            this.PneumaticPressureComboBox.TabIndex = 6;
            // 
            // PneumaticDiameterComboBox
            // 
            this.PneumaticDiameterComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.PneumaticDiameterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.PneumaticDiameterComboBox.FormattingEnabled = true;
            this.PneumaticDiameterComboBox.Items.AddRange(new object[] {
            "1 in",
            ".5 in",
            ".25 in"});
            this.PneumaticDiameterComboBox.Location = new System.Drawing.Point(3, 27);
            this.PneumaticDiameterComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PneumaticDiameterComboBox.Name = "PneumaticDiameterComboBox";
            this.PneumaticDiameterComboBox.Size = new System.Drawing.Size(250, 24);
            this.PneumaticDiameterComboBox.TabIndex = 12;
            // 
            // DefinePartPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.NodeGroupBox);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "DefinePartPanel";
            this.Size = new System.Drawing.Size(560, 316);
            this.NodeGroupBox.ResumeLayout(false);
            this.NodeGroupBox.PerformLayout();
            this.MainTableLayout.ResumeLayout(false);
            this.MainTableLayout.PerformLayout();
            this.DriverLayout.ResumeLayout(false);
            this.DriverLayout.PerformLayout();
            this.JointLimitGroupBox.ResumeLayout(false);
            this.JointLimitGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.UpperLimitUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.LowerLimitUpDown)).EndInit();
            this.PortsGroupBox.ResumeLayout(false);
            this.PortsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortTwoUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortOneUpDown)).EndInit();
            this.MetaTabControl.ResumeLayout(false);
            this.PneumaticTab.ResumeLayout(false);
            this.PneumaticTab.PerformLayout();
            this.PneumaticLayout.ResumeLayout(false);
            this.PneumaticLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox NodeGroupBox;
        private System.Windows.Forms.Label SelectDriverLabel;
        private System.Windows.Forms.ComboBox DriverComboBox;
        private System.Windows.Forms.GroupBox PortsGroupBox;
        private System.Windows.Forms.NumericUpDown PortTwoUpDown;
        private System.Windows.Forms.Label PortTwoLabel;
        private System.Windows.Forms.Label PortOneLabel;
        private System.Windows.Forms.NumericUpDown PortOneUpDown;
        private System.Windows.Forms.TabControl MetaTabControl;
        private System.Windows.Forms.TabPage PneumaticTab;
        private System.Windows.Forms.Label PressureLabel;
        private System.Windows.Forms.ComboBox PneumaticPressureComboBox;
        private System.Windows.Forms.ComboBox PneumaticDiameterComboBox;
        private System.Windows.Forms.Label DiameterLabel;
        private System.Windows.Forms.GroupBox JointLimitGroupBox;
        private System.Windows.Forms.NumericUpDown UpperLimitUpDown;
        private System.Windows.Forms.Label UpperLimitLabel;
        private System.Windows.Forms.Label LowerLimitLabel;
        private System.Windows.Forms.NumericUpDown LowerLimitUpDown;
        private System.Windows.Forms.Label TotalFreedomLabel;
        private System.Windows.Forms.Label FreedomFactorLabel;
        private System.Windows.Forms.TableLayoutPanel MainTableLayout;
        private System.Windows.Forms.TableLayoutPanel DriverLayout;
        private System.Windows.Forms.TableLayoutPanel PneumaticLayout;
    }
}
