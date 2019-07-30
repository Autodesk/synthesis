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
            this.cancelButton = new System.Windows.Forms.Button();
            this.gearRatioInput = new System.Windows.Forms.NumericUpDown();
            this.portInput = new System.Windows.Forms.TextBox();
            this.sensorsTable = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.portTypeInput = new System.Windows.Forms.ComboBox();
            this.addSensorButton = new System.Windows.Forms.Button();
            this.removeSensorButton = new System.Windows.Forms.Button();
            this.sensorBox = new System.Windows.Forms.GroupBox();
            this.gearRatioBox = new System.Windows.Forms.GroupBox();
            this.portBox = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.gearRatioInput)).BeginInit();
            this.sensorBox.SuspendLayout();
            this.gearRatioBox.SuspendLayout();
            this.portBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(236, 262);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(317, 262);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // gearRatioInput
            // 
            this.gearRatioInput.Location = new System.Drawing.Point(6, 20);
            this.gearRatioInput.Name = "gearRatioInput";
            this.gearRatioInput.Size = new System.Drawing.Size(120, 20);
            this.gearRatioInput.TabIndex = 4;
            // 
            // portInput
            // 
            this.portInput.Location = new System.Drawing.Point(6, 19);
            this.portInput.Name = "portInput";
            this.portInput.Size = new System.Drawing.Size(46, 20);
            this.portInput.TabIndex = 6;
            // 
            // sensorsTable
            // 
            this.sensorsTable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.sensorsTable.LabelEdit = true;
            this.sensorsTable.Location = new System.Drawing.Point(6, 19);
            this.sensorsTable.Name = "sensorsTable";
            this.sensorsTable.Size = new System.Drawing.Size(366, 127);
            this.sensorsTable.TabIndex = 7;
            this.sensorsTable.UseCompatibleStateImageBehavior = false;
            this.sensorsTable.View = System.Windows.Forms.View.Details;
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
            this.portTypeInput.Location = new System.Drawing.Point(58, 18);
            this.portTypeInput.Name = "portTypeInput";
            this.portTypeInput.Size = new System.Drawing.Size(52, 21);
            this.portTypeInput.TabIndex = 9;
            // 
            // addSensorButton
            // 
            this.addSensorButton.Location = new System.Drawing.Point(216, 152);
            this.addSensorButton.Name = "addSensorButton";
            this.addSensorButton.Size = new System.Drawing.Size(75, 23);
            this.addSensorButton.TabIndex = 10;
            this.addSensorButton.Text = "Add";
            this.addSensorButton.UseVisualStyleBackColor = true;
            // 
            // removeSensorButton
            // 
            this.removeSensorButton.Location = new System.Drawing.Point(297, 152);
            this.removeSensorButton.Name = "removeSensorButton";
            this.removeSensorButton.Size = new System.Drawing.Size(75, 23);
            this.removeSensorButton.TabIndex = 11;
            this.removeSensorButton.Text = "Remove";
            this.removeSensorButton.UseVisualStyleBackColor = true;
            // 
            // sensorBox
            // 
            this.sensorBox.Controls.Add(this.sensorsTable);
            this.sensorBox.Controls.Add(this.addSensorButton);
            this.sensorBox.Controls.Add(this.removeSensorButton);
            this.sensorBox.Location = new System.Drawing.Point(12, 12);
            this.sensorBox.Name = "sensorBox";
            this.sensorBox.Size = new System.Drawing.Size(378, 183);
            this.sensorBox.TabIndex = 12;
            this.sensorBox.TabStop = false;
            this.sensorBox.Text = "Sensors";
            // 
            // gearRatioBox
            // 
            this.gearRatioBox.Controls.Add(this.gearRatioInput);
            this.gearRatioBox.Location = new System.Drawing.Point(12, 206);
            this.gearRatioBox.Name = "gearRatioBox";
            this.gearRatioBox.Size = new System.Drawing.Size(134, 50);
            this.gearRatioBox.TabIndex = 13;
            this.gearRatioBox.TabStop = false;
            this.gearRatioBox.Text = "Gear Ratio";
            // 
            // portBox
            // 
            this.portBox.Controls.Add(this.portTypeInput);
            this.portBox.Controls.Add(this.portInput);
            this.portBox.Location = new System.Drawing.Point(168, 206);
            this.portBox.Name = "portBox";
            this.portBox.Size = new System.Drawing.Size(117, 50);
            this.portBox.TabIndex = 14;
            this.portBox.TabStop = false;
            this.portBox.Text = "Port";
            // 
            // AdvancedJointSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(402, 297);
            this.Controls.Add(this.portBox);
            this.Controls.Add(this.gearRatioBox);
            this.Controls.Add(this.sensorBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AdvancedJointSettings";
            this.Text = "Advanced Joint Settings";
            ((System.ComponentModel.ISupportInitialize)(this.gearRatioInput)).EndInit();
            this.sensorBox.ResumeLayout(false);
            this.gearRatioBox.ResumeLayout(false);
            this.portBox.ResumeLayout(false);
            this.portBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.NumericUpDown gearRatioInput;
        private System.Windows.Forms.TextBox portInput;
        private System.Windows.Forms.ListView sensorsTable;
        private System.Windows.Forms.ComboBox portTypeInput;
        private System.Windows.Forms.Button addSensorButton;
        private System.Windows.Forms.Button removeSensorButton;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.GroupBox sensorBox;
        private System.Windows.Forms.GroupBox gearRatioBox;
        private System.Windows.Forms.GroupBox portBox;
    }
}