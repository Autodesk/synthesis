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
            this.PortsGroupBox = new System.Windows.Forms.GroupBox();
            this.PortLayout = new System.Windows.Forms.TableLayoutPanel();
            this.PortOneLabel = new System.Windows.Forms.Label();
            this.PortTwoLabel = new System.Windows.Forms.Label();
            this.PortTwoUpDown = new System.Windows.Forms.NumericUpDown();
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
            this.PortsGroupBox.SuspendLayout();
            this.PortLayout.SuspendLayout();
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
            this.NodeGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.NodeGroupBox.Location = new System.Drawing.Point(0, 0);
            this.NodeGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.NodeGroupBox.Name = "NodeGroupBox";
            this.NodeGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.NodeGroupBox.Size = new System.Drawing.Size(533, 285);
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
            this.MainTableLayout.Controls.Add(this.PortsGroupBox, 0, 1);
            this.MainTableLayout.Controls.Add(this.MetaTabControl, 1, 2);
            this.MainTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainTableLayout.Location = new System.Drawing.Point(4, 19);
            this.MainTableLayout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MainTableLayout.Name = "MainTableLayout";
            this.MainTableLayout.RowCount = 3;
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainTableLayout.Size = new System.Drawing.Size(525, 262);
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
            this.DriverLayout.Size = new System.Drawing.Size(525, 32);
            this.DriverLayout.TabIndex = 0;
            // 
            // SelectDriverLabel
            // 
            this.SelectDriverLabel.AutoSize = true;
            this.SelectDriverLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.SelectDriverLabel.Location = new System.Drawing.Point(4, 4);
            this.SelectDriverLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectDriverLabel.Name = "SelectDriverLabel";
            this.SelectDriverLabel.Size = new System.Drawing.Size(80, 24);
            this.SelectDriverLabel.TabIndex = 1;
            this.SelectDriverLabel.Text = "Joint Driver";
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
            this.DriverComboBox.Location = new System.Drawing.Point(92, 4);
            this.DriverComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DriverComboBox.Name = "DriverComboBox";
            this.DriverComboBox.Size = new System.Drawing.Size(429, 24);
            this.DriverComboBox.TabIndex = 2;
            this.DriverComboBox.SelectedIndexChanged += new System.EventHandler(this.DriverComboBox_SelectedIndexChanged);
            // 
            // PortsGroupBox
            // 
            this.PortsGroupBox.AutoSize = true;
            this.PortsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainTableLayout.SetColumnSpan(this.PortsGroupBox, 2);
            this.PortsGroupBox.Controls.Add(this.PortLayout);
            this.PortsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortsGroupBox.Location = new System.Drawing.Point(4, 36);
            this.PortsGroupBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortsGroupBox.Name = "PortsGroupBox";
            this.PortsGroupBox.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortsGroupBox.Size = new System.Drawing.Size(517, 53);
            this.PortsGroupBox.TabIndex = 6;
            this.PortsGroupBox.TabStop = false;
            this.PortsGroupBox.Text = "Ports";
            // 
            // PortLayout
            // 
            this.PortLayout.AutoSize = true;
            this.PortLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.PortLayout.ColumnCount = 4;
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.PortLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.PortLayout.Controls.Add(this.PortOneLabel, 0, 0);
            this.PortLayout.Controls.Add(this.PortTwoLabel, 2, 0);
            this.PortLayout.Controls.Add(this.PortTwoUpDown, 3, 0);
            this.PortLayout.Controls.Add(this.PortOneUpDown, 1, 0);
            this.PortLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortLayout.Location = new System.Drawing.Point(4, 19);
            this.PortLayout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortLayout.Name = "PortLayout";
            this.PortLayout.RowCount = 1;
            this.PortLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.PortLayout.Size = new System.Drawing.Size(509, 30);
            this.PortLayout.TabIndex = 4;
            // 
            // PortOneLabel
            // 
            this.PortOneLabel.AutoSize = true;
            this.PortOneLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PortOneLabel.Location = new System.Drawing.Point(4, 4);
            this.PortOneLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortOneLabel.Name = "PortOneLabel";
            this.PortOneLabel.Size = new System.Drawing.Size(46, 22);
            this.PortOneLabel.TabIndex = 1;
            this.PortOneLabel.Text = "Port 1";
            this.PortOneLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PortTwoLabel
            // 
            this.PortTwoLabel.AutoSize = true;
            this.PortTwoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PortTwoLabel.Location = new System.Drawing.Point(258, 4);
            this.PortTwoLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortTwoLabel.Name = "PortTwoLabel";
            this.PortTwoLabel.Size = new System.Drawing.Size(46, 22);
            this.PortTwoLabel.TabIndex = 2;
            this.PortTwoLabel.Text = "Port 2";
            this.PortTwoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // PortTwoUpDown
            // 
            this.PortTwoUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortTwoUpDown.Location = new System.Drawing.Point(312, 4);
            this.PortTwoUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortTwoUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.PortTwoUpDown.Name = "PortTwoUpDown";
            this.PortTwoUpDown.Size = new System.Drawing.Size(193, 22);
            this.PortTwoUpDown.TabIndex = 3;
            this.PortTwoUpDown.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // PortOneUpDown
            // 
            this.PortOneUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.PortOneUpDown.Location = new System.Drawing.Point(58, 4);
            this.PortOneUpDown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PortOneUpDown.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.PortOneUpDown.Name = "PortOneUpDown";
            this.PortOneUpDown.Size = new System.Drawing.Size(192, 22);
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
            this.MetaTabControl.Location = new System.Drawing.Point(265, 95);
            this.MetaTabControl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MetaTabControl.Name = "MetaTabControl";
            this.MetaTabControl.SelectedIndex = 0;
            this.MetaTabControl.Size = new System.Drawing.Size(257, 165);
            this.MetaTabControl.TabIndex = 12;
            this.MetaTabControl.Visible = false;
            // 
            // PneumaticTab
            // 
            this.PneumaticTab.Controls.Add(this.PneumaticLayout);
            this.PneumaticTab.Location = new System.Drawing.Point(4, 25);
            this.PneumaticTab.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.PneumaticTab.Name = "PneumaticTab";
            this.PneumaticTab.Size = new System.Drawing.Size(249, 136);
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
            this.PneumaticLayout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PneumaticLayout.Name = "PneumaticLayout";
            this.PneumaticLayout.RowCount = 4;
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.PneumaticLayout.Size = new System.Drawing.Size(249, 114);
            this.PneumaticLayout.TabIndex = 15;
            // 
            // PressureLabel
            // 
            this.PressureLabel.AutoSize = true;
            this.PressureLabel.Location = new System.Drawing.Point(4, 61);
            this.PressureLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PressureLabel.Name = "PressureLabel";
            this.PressureLabel.Size = new System.Drawing.Size(65, 17);
            this.PressureLabel.TabIndex = 13;
            this.PressureLabel.Text = "Pressure";
            // 
            // DiameterLabel
            // 
            this.DiameterLabel.AutoSize = true;
            this.DiameterLabel.Location = new System.Drawing.Point(4, 4);
            this.DiameterLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
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
            this.PneumaticPressureComboBox.Location = new System.Drawing.Point(4, 86);
            this.PneumaticPressureComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PneumaticPressureComboBox.Name = "PneumaticPressureComboBox";
            this.PneumaticPressureComboBox.Size = new System.Drawing.Size(241, 24);
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
            this.PneumaticDiameterComboBox.Location = new System.Drawing.Point(4, 29);
            this.PneumaticDiameterComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PneumaticDiameterComboBox.Name = "PneumaticDiameterComboBox";
            this.PneumaticDiameterComboBox.Size = new System.Drawing.Size(241, 24);
            this.PneumaticDiameterComboBox.TabIndex = 12;
            // 
            // DefinePartPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.NodeGroupBox);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(533, 0);
            this.Name = "DefinePartPanel";
            this.Size = new System.Drawing.Size(533, 285);
            this.NodeGroupBox.ResumeLayout(false);
            this.NodeGroupBox.PerformLayout();
            this.MainTableLayout.ResumeLayout(false);
            this.MainTableLayout.PerformLayout();
            this.DriverLayout.ResumeLayout(false);
            this.DriverLayout.PerformLayout();
            this.PortsGroupBox.ResumeLayout(false);
            this.PortsGroupBox.PerformLayout();
            this.PortLayout.ResumeLayout(false);
            this.PortLayout.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel MainTableLayout;
        private System.Windows.Forms.TableLayoutPanel DriverLayout;
        private System.Windows.Forms.TableLayoutPanel PneumaticLayout;
        private System.Windows.Forms.TableLayoutPanel PortLayout;
    }
}
