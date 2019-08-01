namespace InventorRobotExporter.GUI.Editors.SimpleJointEditor
{
    partial class AdvancedJointSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedJointSettings));
            this.okButton = new System.Windows.Forms.Button();
            this.gearRatioInput = new System.Windows.Forms.NumericUpDown();
            this.sensorListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.portTypeInput = new System.Windows.Forms.ComboBox();
            this.addSensorButton = new System.Windows.Forms.Button();
            this.removeSensorButton = new System.Windows.Forms.Button();
            this.sensorBox = new System.Windows.Forms.GroupBox();
            this.gearRatioBox = new System.Windows.Forms.GroupBox();
            this.portBox = new System.Windows.Forms.GroupBox();
            this.portInput = new System.Windows.Forms.NumericUpDown();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gearRatioInput)).BeginInit();
            this.sensorBox.SuspendLayout();
            this.gearRatioBox.SuspendLayout();
            this.portBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portInput)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(315, 326);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 28);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // gearRatioInput
            // 
            this.gearRatioInput.DecimalPlaces = 4;
            this.gearRatioInput.Location = new System.Drawing.Point(8, 25);
            this.gearRatioInput.Margin = new System.Windows.Forms.Padding(4);
            this.gearRatioInput.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.gearRatioInput.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            262144});
            this.gearRatioInput.Name = "gearRatioInput";
            this.gearRatioInput.Size = new System.Drawing.Size(234, 22);
            this.gearRatioInput.TabIndex = 4;
            this.gearRatioInput.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // sensorListView
            // 
            this.sensorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.sensorListView.Location = new System.Drawing.Point(8, 23);
            this.sensorListView.Margin = new System.Windows.Forms.Padding(4);
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.Size = new System.Drawing.Size(487, 155);
            this.sensorListView.TabIndex = 7;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            this.sensorListView.View = System.Windows.Forms.View.Details;
            this.sensorListView.SelectedIndexChanged += new System.EventHandler(this.SensorListView_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Sensor Type";
            this.columnHeader1.Width = 136;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Port A";
            this.columnHeader2.Width = 113;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Port B";
            // 
            // portTypeInput
            // 
            this.portTypeInput.FormattingEnabled = true;
            this.portTypeInput.Items.AddRange(new object[] {
            "CAN",
            "PWM"});
            this.portTypeInput.Location = new System.Drawing.Point(166, 23);
            this.portTypeInput.Margin = new System.Windows.Forms.Padding(4);
            this.portTypeInput.Name = "portTypeInput";
            this.portTypeInput.Size = new System.Drawing.Size(71, 24);
            this.portTypeInput.TabIndex = 9;
            this.portTypeInput.Text = "CAN";
            // 
            // addSensorButton
            // 
            this.addSensorButton.Location = new System.Drawing.Point(288, 187);
            this.addSensorButton.Margin = new System.Windows.Forms.Padding(4);
            this.addSensorButton.Name = "addSensorButton";
            this.addSensorButton.Size = new System.Drawing.Size(100, 28);
            this.addSensorButton.TabIndex = 10;
            this.addSensorButton.Text = "Add";
            this.addSensorButton.UseVisualStyleBackColor = true;
            this.addSensorButton.Click += new System.EventHandler(this.AddSensorButton_Click);
            // 
            // removeSensorButton
            // 
            this.removeSensorButton.Location = new System.Drawing.Point(396, 187);
            this.removeSensorButton.Margin = new System.Windows.Forms.Padding(4);
            this.removeSensorButton.Name = "removeSensorButton";
            this.removeSensorButton.Size = new System.Drawing.Size(100, 28);
            this.removeSensorButton.TabIndex = 11;
            this.removeSensorButton.Text = "Remove";
            this.removeSensorButton.UseVisualStyleBackColor = true;
            this.removeSensorButton.Click += new System.EventHandler(this.RemoveSensorButton_Click);
            // 
            // sensorBox
            // 
            this.sensorBox.Controls.Add(this.sensorListView);
            this.sensorBox.Controls.Add(this.addSensorButton);
            this.sensorBox.Controls.Add(this.removeSensorButton);
            this.sensorBox.Location = new System.Drawing.Point(16, 15);
            this.sensorBox.Margin = new System.Windows.Forms.Padding(4);
            this.sensorBox.Name = "sensorBox";
            this.sensorBox.Padding = new System.Windows.Forms.Padding(4);
            this.sensorBox.Size = new System.Drawing.Size(504, 225);
            this.sensorBox.TabIndex = 12;
            this.sensorBox.TabStop = false;
            this.sensorBox.Text = "Sensors";
            // 
            // gearRatioBox
            // 
            this.gearRatioBox.Controls.Add(this.gearRatioInput);
            this.gearRatioBox.Location = new System.Drawing.Point(16, 254);
            this.gearRatioBox.Margin = new System.Windows.Forms.Padding(4);
            this.gearRatioBox.Name = "gearRatioBox";
            this.gearRatioBox.Padding = new System.Windows.Forms.Padding(4);
            this.gearRatioBox.Size = new System.Drawing.Size(250, 62);
            this.gearRatioBox.TabIndex = 13;
            this.gearRatioBox.TabStop = false;
            this.gearRatioBox.Text = "Gear Ratio";
            // 
            // portBox
            // 
            this.portBox.Controls.Add(this.portInput);
            this.portBox.Controls.Add(this.portTypeInput);
            this.portBox.Location = new System.Drawing.Point(274, 254);
            this.portBox.Margin = new System.Windows.Forms.Padding(4);
            this.portBox.Name = "portBox";
            this.portBox.Padding = new System.Windows.Forms.Padding(4);
            this.portBox.Size = new System.Drawing.Size(249, 62);
            this.portBox.TabIndex = 14;
            this.portBox.TabStop = false;
            this.portBox.Text = "Port";
            // 
            // portInput
            // 
            this.portInput.Location = new System.Drawing.Point(8, 25);
            this.portInput.Margin = new System.Windows.Forms.Padding(4);
            this.portInput.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.portInput.Minimum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.portInput.Name = "portInput";
            this.portInput.Size = new System.Drawing.Size(150, 22);
            this.portInput.TabIndex = 10;
            this.portInput.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(423, 326);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 28);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // AdvancedJointSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(536, 367);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.gearRatioBox);
            this.Controls.Add(this.sensorBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AdvancedJointSettings";
            this.Text = "Advanced Joint Settings";
            ((System.ComponentModel.ISupportInitialize)(this.gearRatioInput)).EndInit();
            this.sensorBox.ResumeLayout(false);
            this.gearRatioBox.ResumeLayout(false);
            this.portBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.portInput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.NumericUpDown gearRatioInput;
        private System.Windows.Forms.ListView sensorListView;
        private System.Windows.Forms.ComboBox portTypeInput;
        private System.Windows.Forms.Button addSensorButton;
        private System.Windows.Forms.Button removeSensorButton;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.GroupBox sensorBox;
        private System.Windows.Forms.GroupBox gearRatioBox;
        private System.Windows.Forms.GroupBox portBox;
        private System.Windows.Forms.NumericUpDown portInput;
        private System.Windows.Forms.Button cancelButton;
    }
}