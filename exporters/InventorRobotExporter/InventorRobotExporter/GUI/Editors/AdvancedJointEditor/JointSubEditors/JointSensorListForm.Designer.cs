namespace InventorRobotExporter.GUI.Editors.JointSubEditors
{
    partial class JointSensorListForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JointSensorListForm));
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.moduleColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.portColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sensorListView = new System.Windows.Forms.ListView();
            this.deleteButton = new System.Windows.Forms.Button();
            this.addSensorButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // typeColumnHeader
            // 
            this.typeColumnHeader.Text = "Sensor Type";
            this.typeColumnHeader.Width = 134;
            // 
            // moduleColumnHeader
            // 
            this.moduleColumnHeader.Text = "Port A";
            this.moduleColumnHeader.Width = 76;
            // 
            // portColumnHeader
            // 
            this.portColumnHeader.Text = "Port B";
            // 
            // sensorListView
            // 
            this.sensorListView.AutoArrange = false;
            this.sensorListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.typeColumnHeader,
            this.moduleColumnHeader,
            this.portColumnHeader});
            this.sensorListView.FullRowSelect = true;
            this.sensorListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.sensorListView.HoverSelection = true;
            this.sensorListView.Location = new System.Drawing.Point(12, 12);
            this.sensorListView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.sensorListView.MultiSelect = false;
            this.sensorListView.Name = "sensorListView";
            this.sensorListView.ShowGroups = false;
            this.sensorListView.Size = new System.Drawing.Size(507, 196);
            this.sensorListView.TabIndex = 0;
            this.sensorListView.UseCompatibleStateImageBehavior = false;
            this.sensorListView.View = System.Windows.Forms.View.Details;
            this.sensorListView.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.sensorListView_ColumnWidthChanging);
            this.sensorListView.SelectedIndexChanged += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.sensorListView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.sensorListView_SelectedIndexChanged);
            // 
            // deleteButton
            // 
            this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.deleteButton.Location = new System.Drawing.Point(12, 222);
            this.deleteButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(129, 36);
            this.deleteButton.TabIndex = 1;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            this.deleteButton.Click += new System.EventHandler(this.deleteButton_Click);
            // 
            // addSensorButton
            // 
            this.addSensorButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.addSensorButton.Location = new System.Drawing.Point(404, 222);
            this.addSensorButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.addSensorButton.Name = "addSensorButton";
            this.addSensorButton.Size = new System.Drawing.Size(115, 36);
            this.addSensorButton.TabIndex = 2;
            this.addSensorButton.Text = "Add Sensor";
            this.addSensorButton.UseVisualStyleBackColor = true;
            this.addSensorButton.Click += new System.EventHandler(this.addSensorButton_Click);
            // 
            // SensorListForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(531, 271);
            this.Controls.Add(this.addSensorButton);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.sensorListView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(549, 298);
            this.Name = "JointSensorListForm";
            this.Text = "Sensor List";
            this.Activated += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.Deactivate += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.SizeChanged += new System.EventHandler(this.window_SizeChanged);
            this.Enter += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.Leave += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.MouseEnter += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.MouseHover += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.sensorListView_SelectedIndexChanged);
            this.Validating += new System.ComponentModel.CancelEventHandler(this.sensorListView_SelectedIndexChanged);
            this.Validated += new System.EventHandler(this.sensorListView_SelectedIndexChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader typeColumnHeader;
        private System.Windows.Forms.ColumnHeader moduleColumnHeader;
        private System.Windows.Forms.ColumnHeader portColumnHeader;
        private System.Windows.Forms.ListView sensorListView;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button addSensorButton;

    }
}