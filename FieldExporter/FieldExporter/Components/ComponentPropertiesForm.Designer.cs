using FieldExporter.Components;
namespace FieldExporter.Controls
{
    partial class ComponentPropertiesForm
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
            this.components = new System.ComponentModel.Container();
            this.inventorSelectButton = new System.Windows.Forms.Button();
            this.addSelectionButton = new System.Windows.Forms.Button();
            this.propertiesBox = new System.Windows.Forms.GroupBox();
            this.convexCheckBox = new System.Windows.Forms.CheckBox();
            this.frictionLabel = new System.Windows.Forms.Label();
            this.frictionTrackBar = new System.Windows.Forms.TrackBar();
            this.colliderTypeLabel = new System.Windows.Forms.Label();
            this.colliderTypeCombobox = new System.Windows.Forms.ComboBox();
            this.physicsGroupOptionsBox = new System.Windows.Forms.GroupBox();
            this.changeNameButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.nameLabel = new System.Windows.Forms.Label();
            this.inventorTreeView = new FieldExporter.Components.InventorTreeView(this.components);
            this.propertiesBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).BeginInit();
            this.physicsGroupOptionsBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // inventorSelectButton
            // 
            this.inventorSelectButton.Location = new System.Drawing.Point(3, 336);
            this.inventorSelectButton.Name = "inventorSelectButton";
            this.inventorSelectButton.Size = new System.Drawing.Size(144, 32);
            this.inventorSelectButton.TabIndex = 13;
            this.inventorSelectButton.Tag = "";
            this.inventorSelectButton.Text = "Select in Inventor";
            this.inventorSelectButton.UseVisualStyleBackColor = true;
            this.inventorSelectButton.Click += new System.EventHandler(this.inventorSelectButton_Click);
            // 
            // addSelectionButton
            // 
            this.addSelectionButton.Enabled = false;
            this.addSelectionButton.Location = new System.Drawing.Point(153, 336);
            this.addSelectionButton.Name = "addSelectionButton";
            this.addSelectionButton.Size = new System.Drawing.Size(144, 32);
            this.addSelectionButton.TabIndex = 14;
            this.addSelectionButton.Text = "Add Selection";
            this.addSelectionButton.UseVisualStyleBackColor = true;
            this.addSelectionButton.Click += new System.EventHandler(this.addSelectionButton_Click);
            // 
            // propertiesBox
            // 
            this.propertiesBox.Controls.Add(this.convexCheckBox);
            this.propertiesBox.Controls.Add(this.frictionLabel);
            this.propertiesBox.Controls.Add(this.frictionTrackBar);
            this.propertiesBox.Controls.Add(this.colliderTypeLabel);
            this.propertiesBox.Controls.Add(this.colliderTypeCombobox);
            this.propertiesBox.Location = new System.Drawing.Point(304, 96);
            this.propertiesBox.Name = "propertiesBox";
            this.propertiesBox.Size = new System.Drawing.Size(293, 272);
            this.propertiesBox.TabIndex = 15;
            this.propertiesBox.TabStop = false;
            this.propertiesBox.Text = "Physical Properties";
            // 
            // convexCheckBox
            // 
            this.convexCheckBox.AutoSize = true;
            this.convexCheckBox.Location = new System.Drawing.Point(9, 51);
            this.convexCheckBox.Name = "convexCheckBox";
            this.convexCheckBox.Size = new System.Drawing.Size(76, 21);
            this.convexCheckBox.TabIndex = 4;
            this.convexCheckBox.Text = "Convex";
            this.convexCheckBox.UseVisualStyleBackColor = true;
            // 
            // frictionLabel
            // 
            this.frictionLabel.AutoSize = true;
            this.frictionLabel.Location = new System.Drawing.Point(6, 78);
            this.frictionLabel.Name = "frictionLabel";
            this.frictionLabel.Size = new System.Drawing.Size(58, 34);
            this.frictionLabel.TabIndex = 3;
            this.frictionLabel.Text = "Friction:\r\n0";
            // 
            // frictionTrackBar
            // 
            this.frictionTrackBar.LargeChange = 1;
            this.frictionTrackBar.Location = new System.Drawing.Point(70, 78);
            this.frictionTrackBar.Name = "frictionTrackBar";
            this.frictionTrackBar.Size = new System.Drawing.Size(217, 56);
            this.frictionTrackBar.TabIndex = 2;
            this.frictionTrackBar.Scroll += new System.EventHandler(this.frictionTrackBar_Scroll);
            // 
            // colliderTypeLabel
            // 
            this.colliderTypeLabel.AutoSize = true;
            this.colliderTypeLabel.Location = new System.Drawing.Point(6, 24);
            this.colliderTypeLabel.Name = "colliderTypeLabel";
            this.colliderTypeLabel.Size = new System.Drawing.Size(95, 17);
            this.colliderTypeLabel.TabIndex = 1;
            this.colliderTypeLabel.Text = "Collider Type:";
            // 
            // colliderTypeCombobox
            // 
            this.colliderTypeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colliderTypeCombobox.FormattingEnabled = true;
            this.colliderTypeCombobox.Items.AddRange(new object[] {
            "Mesh Collider",
            "Box Collider"});
            this.colliderTypeCombobox.Location = new System.Drawing.Point(107, 21);
            this.colliderTypeCombobox.Name = "colliderTypeCombobox";
            this.colliderTypeCombobox.Size = new System.Drawing.Size(180, 24);
            this.colliderTypeCombobox.TabIndex = 0;
            this.colliderTypeCombobox.SelectedIndexChanged += new System.EventHandler(this.colliderTypeCombobox_SelectedIndexChanged);
            // 
            // physicsGroupOptionsBox
            // 
            this.physicsGroupOptionsBox.Controls.Add(this.changeNameButton);
            this.physicsGroupOptionsBox.Controls.Add(this.removeButton);
            this.physicsGroupOptionsBox.Controls.Add(this.nameLabel);
            this.physicsGroupOptionsBox.Location = new System.Drawing.Point(304, 4);
            this.physicsGroupOptionsBox.Name = "physicsGroupOptionsBox";
            this.physicsGroupOptionsBox.Size = new System.Drawing.Size(293, 86);
            this.physicsGroupOptionsBox.TabIndex = 16;
            this.physicsGroupOptionsBox.TabStop = false;
            this.physicsGroupOptionsBox.Text = "PhysicsGroup Options";
            // 
            // changeNameButton
            // 
            this.changeNameButton.Location = new System.Drawing.Point(159, 13);
            this.changeNameButton.Name = "changeNameButton";
            this.changeNameButton.Size = new System.Drawing.Size(128, 32);
            this.changeNameButton.TabIndex = 3;
            this.changeNameButton.Text = "Change Name";
            this.changeNameButton.UseVisualStyleBackColor = true;
            this.changeNameButton.Click += new System.EventHandler(this.changeNameButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(6, 48);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(281, 32);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove PhysicsGroup";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // nameLabel
            // 
            this.nameLabel.AutoEllipsis = true;
            this.nameLabel.Location = new System.Drawing.Point(6, 21);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(147, 17);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            // 
            // inventorTreeView
            // 
            this.inventorTreeView.Location = new System.Drawing.Point(3, 3);
            this.inventorTreeView.Name = "inventorTreeView";
            this.inventorTreeView.Size = new System.Drawing.Size(294, 327);
            this.inventorTreeView.TabIndex = 12;
            // 
            // ComponentPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.physicsGroupOptionsBox);
            this.Controls.Add(this.propertiesBox);
            this.Controls.Add(this.addSelectionButton);
            this.Controls.Add(this.inventorSelectButton);
            this.Controls.Add(this.inventorTreeView);
            this.Name = "ComponentPropertiesForm";
            this.Size = new System.Drawing.Size(600, 371);
            this.propertiesBox.ResumeLayout(false);
            this.propertiesBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).EndInit();
            this.physicsGroupOptionsBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button inventorSelectButton;
        private System.Windows.Forms.Button addSelectionButton;
        private System.Windows.Forms.GroupBox propertiesBox;
        private System.Windows.Forms.GroupBox physicsGroupOptionsBox;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button changeNameButton;
        private System.Windows.Forms.ComboBox colliderTypeCombobox;
        private System.Windows.Forms.Label colliderTypeLabel;
        private System.Windows.Forms.Label frictionLabel;
        private System.Windows.Forms.TrackBar frictionTrackBar;
        public InventorTreeView inventorTreeView;
        private System.Windows.Forms.CheckBox convexCheckBox;
    }
}
