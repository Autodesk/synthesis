namespace JointResolver.ControlGUI
{
    partial class SensorListForm
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
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.moduleColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.portColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sensorListView = new System.Windows.Forms.ListView();
            this.polynomialHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.deleteButton = new System.Windows.Forms.Button();
            this.addSensorButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // typeColumnHeader
            // 
            this.typeColumnHeader.Text = "Type";
            this.typeColumnHeader.Width = 115;
            // 
            // moduleColumnHeader
            // 
            this.moduleColumnHeader.Text = "Module #";
            this.moduleColumnHeader.Width = 76;
            // 
            // portColumnHeader
            // 
            this.portColumnHeader.Text = "Port #";
            // 
            // sensorListView
            // 
            this.sensorListView.AutoArrange = false;
            this.sensorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.typeColumnHeader,
            this.moduleColumnHeader,
            this.portColumnHeader,
            this.polynomialHeader});
            this.sensorListView.FullRowSelect = true;
            this.sensorListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.sensorListView.HoverSelection = true;
            this.sensorListView.Location = new System.Drawing.Point(12, 12);
            this.sensorListView.MultiSelect = false;
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.ShowGroups = false;
            this.sensorListView.Size = new System.Drawing.Size(516, 196);
            this.sensorListView.TabIndex = 5;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            this.sensorListView.View = System.Windows.Forms.View.Details;
            // 
            // polynomialHeader
            // 
            this.polynomialHeader.Text = "Polynomial";
            this.polynomialHeader.Width = 134;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(22, 220);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(129, 36);
            this.deleteButton.TabIndex = 6;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // addSensorButton
            // 
            this.addSensorButton.Location = new System.Drawing.Point(172, 220);
            this.addSensorButton.Name = "addSensorButton";
            this.addSensorButton.Size = new System.Drawing.Size(114, 36);
            this.addSensorButton.TabIndex = 7;
            this.addSensorButton.Text = "Add Sensor";
            this.addSensorButton.UseVisualStyleBackColor = true;
            this.addSensorButton.Click += new System.EventHandler(this.addSensorButton_Click);
            // 
            // SensorListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 269);
            this.Controls.Add(this.addSensorButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.sensorListView);
            this.Name = "SensorListForm";
            this.Text = "SensorListForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader typeColumnHeader;
        private System.Windows.Forms.ColumnHeader moduleColumnHeader;
        private System.Windows.Forms.ColumnHeader portColumnHeader;
        private System.Windows.Forms.ListView sensorListView;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button addSensorButton;
        private System.Windows.Forms.ColumnHeader polynomialHeader;

    }
}