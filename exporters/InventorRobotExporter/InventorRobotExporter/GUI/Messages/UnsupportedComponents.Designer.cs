namespace InventorRobotExporter.GUI.Messages
{
    partial class UnsupportedComponents
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UnsupportedComponents));
            this.okButton = new System.Windows.Forms.Button();
            this.description = new System.Windows.Forms.Label();
            this.componentListView = new System.Windows.Forms.ListView();
            this.componentName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.componentCategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.componentType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.continueButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(452, 289);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // description
            // 
            this.description.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.description.Location = new System.Drawing.Point(12, 9);
            this.description.Name = "description";
            this.description.Size = new System.Drawing.Size(423, 32);
            this.description.TabIndex = 1;
            this.description.Text = "Your robot currently has unsupported components.  You may still export your robot" +
    ", but the following details will not be accounted for in Synthesis:";
            // 
            // componentListView
            // 
            this.componentListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.componentName,
            this.componentCategory,
            this.componentType});
            this.componentListView.Location = new System.Drawing.Point(15, 44);
            this.componentListView.MultiSelect = false;
            this.componentListView.Name = "componentListView";
            this.componentListView.Size = new System.Drawing.Size(420, 97);
            this.componentListView.TabIndex = 2;
            this.componentListView.UseCompatibleStateImageBehavior = false;
            this.componentListView.View = System.Windows.Forms.View.Details;
            this.componentListView.ColumnWidthChanging += new System.Windows.Forms.ColumnWidthChangingEventHandler(this.ComponentListView_ColumnWidthChanging);
            // 
            // componentName
            // 
            this.componentName.Text = "Name";
            this.componentName.Width = 178;
            // 
            // componentCategory
            // 
            this.componentCategory.Text = "Category";
            this.componentCategory.Width = 135;
            // 
            // componentType
            // 
            this.componentType.Text = "Type";
            this.componentType.Width = 84;
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(186, 147);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(75, 23);
            this.continueButton.TabIndex = 3;
            this.continueButton.Text = "Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.ContinueButton_Click);
            // 
            // UnsupportedComponents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 178);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.componentListView);
            this.Controls.Add(this.description);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(463, 217);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(463, 217);
            this.Name = "UnsupportedComponents";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Unsupported Components";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label description;
        private System.Windows.Forms.ListView componentListView;
        private System.Windows.Forms.ColumnHeader componentName;
        private System.Windows.Forms.ColumnHeader componentCategory;
        private System.Windows.Forms.Button continueButton;
        private System.Windows.Forms.ColumnHeader componentType;
    }
}