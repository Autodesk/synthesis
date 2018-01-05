namespace EditorsLibrary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SensorListForm));
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.moduleColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.portColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sensorListView = new System.Windows.Forms.ListView();
            this.polynomialHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.deleteButton = new System.Windows.Forms.Button();
            this.addSensorButton = new System.Windows.Forms.Button();
            this.saveButton = new System.Windows.Forms.Button();
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
            this.sensorListView.Location = new System.Drawing.Point(9, 10);
            this.sensorListView.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.sensorListView.MultiSelect = false;
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.ShowGroups = false;
            this.sensorListView.Size = new System.Drawing.Size(388, 160);
            this.sensorListView.TabIndex = 0;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            this.sensorListView.View = System.Windows.Forms.View.Details;
            this.sensorListView.SelectedIndexChanged += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            // 
            // polynomialHeader
            // 
            this.polynomialHeader.Text = "Polynomial";
            this.polynomialHeader.Width = 134;
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteButton.Location = new System.Drawing.Point(16, 179);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(97, 29);
            this.deleteButton.TabIndex = 1;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // addSensorButton
            // 
            this.addSensorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addSensorButton.Location = new System.Drawing.Point(129, 179);
            this.addSensorButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.addSensorButton.Name = "addSensorButton";
            this.addSensorButton.Size = new System.Drawing.Size(86, 29);
            this.addSensorButton.TabIndex = 2;
            this.addSensorButton.Text = "Add Sensor";
            this.addSensorButton.UseVisualStyleBackColor = true;
            this.addSensorButton.Click += new System.EventHandler(this.addSensorButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(303, 177);
            this.saveButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(93, 32);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // SensorListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(406, 221);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.addSensorButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.sensorListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MinimumSize = new System.Drawing.Size(416, 251);
            this.Name = "SensorListForm";
            this.Text = "SensorListForm";
            this.Load += new System.EventHandler(this.SensorListForm_Load);
            this.SizeChanged += new System.EventHandler(this.window_SizeChanged);
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
        private System.Windows.Forms.Button saveButton;

    }
}