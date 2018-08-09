namespace InternalFieldExporter.FieldWizard
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.inventorActionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inventorSelectButton = new System.Windows.Forms.Button();
            this.addSelectionButton = new System.Windows.Forms.Button();
            this.propertiesScrollablePanel = new System.Windows.Forms.Panel();
            this.jointsGroupBox = new System.Windows.Forms.GroupBox();
            this.jointTypeLabel = new System.Windows.Forms.Label();
            this.jointComboBox = new System.Windows.Forms.ComboBox();
            this.jointCheckBox = new System.Windows.Forms.CheckBox();
            this.physicalPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.dynamicCheckBox = new System.Windows.Forms.CheckBox();
            this.dynamicGroupBox = new System.Windows.Forms.GroupBox();
            this.massNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.massLabel = new System.Windows.Forms.Label();
            this.frictionLabelsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.iceLabel = new System.Windows.Forms.Label();
            this.carpetLabel = new System.Windows.Forms.Label();
            this.rubberLabel = new System.Windows.Forms.Label();
            this.frictionTrackBar = new System.Windows.Forms.TrackBar();
            this.frictionLabel = new System.Windows.Forms.Label();
            this.meshPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.colliderTypePanel = new System.Windows.Forms.Panel();
            this.colliderTypeCombobox = new System.Windows.Forms.ComboBox();
            this.colliderTypeLabel = new System.Windows.Forms.Label();
            this.propertySetOptionsBox = new System.Windows.Forms.GroupBox();
            this.propertyOptionsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.removeButton = new System.Windows.Forms.Button();
            this.changeNameButton = new System.Windows.Forms.Button();
            this.meshPropertiesTable = new System.Windows.Forms.TableLayoutPanel();
            this.inventorTreeView = new InternalFieldExporter.InventorTreeView(this.components);
            this.inventorActionsPanel.SuspendLayout();
            this.propertiesScrollablePanel.SuspendLayout();
            this.jointsGroupBox.SuspendLayout();
            this.physicalPropertiesGroupBox.SuspendLayout();
            this.dynamicGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).BeginInit();
            this.frictionLabelsLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).BeginInit();
            this.meshPropertiesGroupBox.SuspendLayout();
            this.colliderTypePanel.SuspendLayout();
            this.propertySetOptionsBox.SuspendLayout();
            this.propertyOptionsLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // inventorActionsPanel
            // 
            this.inventorActionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inventorActionsPanel.ColumnCount = 2;
            this.inventorActionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.Controls.Add(this.inventorSelectButton, 0, 0);
            this.inventorActionsPanel.Controls.Add(this.addSelectionButton, 1, 0);
            this.inventorActionsPanel.Location = new System.Drawing.Point(0, 360);
            this.inventorActionsPanel.Name = "inventorActionsPanel";
            this.inventorActionsPanel.RowCount = 1;
            this.inventorActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.inventorActionsPanel.Size = new System.Drawing.Size(301, 40);
            this.inventorActionsPanel.TabIndex = 0;
            // 
            // inventorSelectButton
            // 
            this.inventorSelectButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventorSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.inventorSelectButton.Location = new System.Drawing.Point(3, 3);
            this.inventorSelectButton.Name = "inventorSelectButton";
            this.inventorSelectButton.Size = new System.Drawing.Size(144, 34);
            this.inventorSelectButton.TabIndex = 0;
            this.inventorSelectButton.Text = "Select in Inventor";
            this.inventorSelectButton.UseVisualStyleBackColor = true;
            this.inventorSelectButton.Click += new System.EventHandler(this.inventorSelectButton_Click);
            // 
            // addSelectionButton
            // 
            this.addSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.addSelectionButton.Enabled = false;
            this.addSelectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addSelectionButton.Location = new System.Drawing.Point(153, 3);
            this.addSelectionButton.Name = "addSelectionButton";
            this.addSelectionButton.Size = new System.Drawing.Size(145, 34);
            this.addSelectionButton.TabIndex = 1;
            this.addSelectionButton.Text = "Add Selection";
            this.addSelectionButton.UseVisualStyleBackColor = true;
            this.addSelectionButton.Click += new System.EventHandler(this.addSelectionButton_Click);
            // 
            // propertiesScrollablePanel
            // 
            this.propertiesScrollablePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertiesScrollablePanel.AutoScroll = true;
            this.propertiesScrollablePanel.Controls.Add(this.jointsGroupBox);
            this.propertiesScrollablePanel.Controls.Add(this.physicalPropertiesGroupBox);
            this.propertiesScrollablePanel.Controls.Add(this.meshPropertiesGroupBox);
            this.propertiesScrollablePanel.Controls.Add(this.propertySetOptionsBox);
            this.propertiesScrollablePanel.Location = new System.Drawing.Point(304, 0);
            this.propertiesScrollablePanel.Name = "propertiesScrollablePanel";
            this.propertiesScrollablePanel.Size = new System.Drawing.Size(296, 400);
            this.propertiesScrollablePanel.TabIndex = 2;
            // 
            // jointsGroupBox
            // 
            this.jointsGroupBox.Controls.Add(this.jointTypeLabel);
            this.jointsGroupBox.Controls.Add(this.jointComboBox);
            this.jointsGroupBox.Controls.Add(this.jointCheckBox);
            this.jointsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointsGroupBox.Location = new System.Drawing.Point(6, 270);
            this.jointsGroupBox.Name = "jointsGroupBox";
            this.jointsGroupBox.Size = new System.Drawing.Size(281, 122);
            this.jointsGroupBox.TabIndex = 3;
            this.jointsGroupBox.TabStop = false;
            // 
            // jointTypeLabel
            // 
            this.jointTypeLabel.AutoSize = true;
            this.jointTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointTypeLabel.Location = new System.Drawing.Point(5, 28);
            this.jointTypeLabel.Name = "jointTypeLabel";
            this.jointTypeLabel.Size = new System.Drawing.Size(78, 17);
            this.jointTypeLabel.TabIndex = 2;
            this.jointTypeLabel.Text = "Joint Type:";
            // 
            // jointComboBox
            // 
            this.jointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointComboBox.FormattingEnabled = true;
            this.jointComboBox.Items.AddRange(new object[] {
            "Rotational",
            "Linear"});
            this.jointComboBox.Location = new System.Drawing.Point(89, 21);
            this.jointComboBox.Name = "jointComboBox";
            this.jointComboBox.Size = new System.Drawing.Size(189, 24);
            this.jointComboBox.TabIndex = 1;
            this.jointComboBox.SelectedIndexChanged += new System.EventHandler(this.jointComboBox_SelectedIndexChanged);
            // 
            // jointCheckBox
            // 
            this.jointCheckBox.AutoSize = true;
            this.jointCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointCheckBox.Location = new System.Drawing.Point(9, 0);
            this.jointCheckBox.Name = "jointCheckBox";
            this.jointCheckBox.Size = new System.Drawing.Size(56, 21);
            this.jointCheckBox.TabIndex = 0;
            this.jointCheckBox.Text = "Joint";
            this.jointCheckBox.UseVisualStyleBackColor = true;
            this.jointCheckBox.CheckedChanged += new System.EventHandler(this.jointCheckBox_CheckChanged);
            // 
            // physicalPropertiesGroupBox
            // 
            this.physicalPropertiesGroupBox.Controls.Add(this.dynamicCheckBox);
            this.physicalPropertiesGroupBox.Controls.Add(this.dynamicGroupBox);
            this.physicalPropertiesGroupBox.Controls.Add(this.frictionLabelsLayoutPanel);
            this.physicalPropertiesGroupBox.Controls.Add(this.frictionTrackBar);
            this.physicalPropertiesGroupBox.Controls.Add(this.frictionLabel);
            this.physicalPropertiesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.physicalPropertiesGroupBox.Location = new System.Drawing.Point(3, 125);
            this.physicalPropertiesGroupBox.Name = "physicalPropertiesGroupBox";
            this.physicalPropertiesGroupBox.Size = new System.Drawing.Size(290, 139);
            this.physicalPropertiesGroupBox.TabIndex = 2;
            this.physicalPropertiesGroupBox.TabStop = false;
            this.physicalPropertiesGroupBox.Text = "Physical Properties";
            // 
            // dynamicCheckBox
            // 
            this.dynamicCheckBox.AutoSize = true;
            this.dynamicCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dynamicCheckBox.Location = new System.Drawing.Point(12, 82);
            this.dynamicCheckBox.Name = "dynamicCheckBox";
            this.dynamicCheckBox.Size = new System.Drawing.Size(80, 21);
            this.dynamicCheckBox.TabIndex = 4;
            this.dynamicCheckBox.Text = "Dynamic";
            this.dynamicCheckBox.UseVisualStyleBackColor = true;
            this.dynamicCheckBox.CheckedChanged += new System.EventHandler(this.dynamicCheckBox_CheckedChanged);
            // 
            // dynamicGroupBox
            // 
            this.dynamicGroupBox.Controls.Add(this.massNumericUpDown);
            this.dynamicGroupBox.Controls.Add(this.massLabel);
            this.dynamicGroupBox.Location = new System.Drawing.Point(6, 83);
            this.dynamicGroupBox.Name = "dynamicGroupBox";
            this.dynamicGroupBox.Size = new System.Drawing.Size(278, 81);
            this.dynamicGroupBox.TabIndex = 3;
            this.dynamicGroupBox.TabStop = false;
            // 
            // massNumericUpDown
            // 
            this.massNumericUpDown.DecimalPlaces = 2;
            this.massNumericUpDown.Location = new System.Drawing.Point(89, 21);
            this.massNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.massNumericUpDown.Name = "massNumericUpDown";
            this.massNumericUpDown.Size = new System.Drawing.Size(183, 22);
            this.massNumericUpDown.TabIndex = 1;
            // 
            // massLabel
            // 
            this.massLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.massLabel.Location = new System.Drawing.Point(6, 24);
            this.massLabel.Name = "massLabel";
            this.massLabel.Size = new System.Drawing.Size(77, 17);
            this.massLabel.TabIndex = 0;
            this.massLabel.Text = "Mass (lbs)";
            // 
            // frictionLabelsLayoutPanel
            // 
            this.frictionLabelsLayoutPanel.ColumnCount = 3;
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 48.34437F));
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 51.65563F));
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.frictionLabelsLayoutPanel.Controls.Add(this.iceLabel, 0, 0);
            this.frictionLabelsLayoutPanel.Controls.Add(this.carpetLabel, 1, 0);
            this.frictionLabelsLayoutPanel.Controls.Add(this.rubberLabel, 2, 0);
            this.frictionLabelsLayoutPanel.Location = new System.Drawing.Point(74, 57);
            this.frictionLabelsLayoutPanel.Name = "frictionLabelsLayoutPanel";
            this.frictionLabelsLayoutPanel.RowCount = 1;
            this.frictionLabelsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.frictionLabelsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.frictionLabelsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.frictionLabelsLayoutPanel.Size = new System.Drawing.Size(214, 20);
            this.frictionLabelsLayoutPanel.TabIndex = 2;
            // 
            // iceLabel
            // 
            this.iceLabel.AutoSize = true;
            this.iceLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iceLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.iceLabel.Location = new System.Drawing.Point(3, 0);
            this.iceLabel.Name = "iceLabel";
            this.iceLabel.Size = new System.Drawing.Size(63, 20);
            this.iceLabel.TabIndex = 0;
            this.iceLabel.Text = "Ice";
            this.iceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // carpetLabel
            // 
            this.carpetLabel.AutoSize = true;
            this.carpetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.carpetLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.carpetLabel.Location = new System.Drawing.Point(72, 0);
            this.carpetLabel.Name = "carpetLabel";
            this.carpetLabel.Size = new System.Drawing.Size(68, 20);
            this.carpetLabel.TabIndex = 1;
            this.carpetLabel.Text = "Carpet";
            this.carpetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rubberLabel
            // 
            this.rubberLabel.AutoSize = true;
            this.rubberLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rubberLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rubberLabel.Location = new System.Drawing.Point(146, 0);
            this.rubberLabel.Name = "rubberLabel";
            this.rubberLabel.Size = new System.Drawing.Size(65, 20);
            this.rubberLabel.TabIndex = 2;
            this.rubberLabel.Text = "Rubber";
            this.rubberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // frictionTrackBar
            // 
            this.frictionTrackBar.Location = new System.Drawing.Point(72, 21);
            this.frictionTrackBar.Maximum = 100;
            this.frictionTrackBar.Name = "frictionTrackBar";
            this.frictionTrackBar.Size = new System.Drawing.Size(214, 56);
            this.frictionTrackBar.TabIndex = 1;
            this.frictionTrackBar.Value = 50;
            this.frictionTrackBar.Scroll += new System.EventHandler(this.frictionTrackBar_Scroll);
            // 
            // frictionLabel
            // 
            this.frictionLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.frictionLabel.Location = new System.Drawing.Point(6, 18);
            this.frictionLabel.Name = "frictionLabel";
            this.frictionLabel.Size = new System.Drawing.Size(58, 34);
            this.frictionLabel.TabIndex = 0;
            this.frictionLabel.Text = "Friction:50/100";
            this.frictionLabel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.frictionLabel_MouseDoubleClick);
            // 
            // meshPropertiesGroupBox
            // 
            this.meshPropertiesGroupBox.AutoSize = true;
            this.meshPropertiesGroupBox.Controls.Add(this.colliderTypePanel);
            this.meshPropertiesGroupBox.Location = new System.Drawing.Point(6, 65);
            this.meshPropertiesGroupBox.Name = "meshPropertiesGroupBox";
            this.meshPropertiesGroupBox.Size = new System.Drawing.Size(290, 51);
            this.meshPropertiesGroupBox.TabIndex = 1;
            this.meshPropertiesGroupBox.TabStop = false;
            this.meshPropertiesGroupBox.Text = "Mesh Properties";
            // 
            // colliderTypePanel
            // 
            this.colliderTypePanel.Controls.Add(this.colliderTypeCombobox);
            this.colliderTypePanel.Controls.Add(this.colliderTypeLabel);
            this.colliderTypePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.colliderTypePanel.Location = new System.Drawing.Point(3, 18);
            this.colliderTypePanel.Name = "colliderTypePanel";
            this.colliderTypePanel.Size = new System.Drawing.Size(284, 30);
            this.colliderTypePanel.TabIndex = 0;
            // 
            // colliderTypeCombobox
            // 
            this.colliderTypeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colliderTypeCombobox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colliderTypeCombobox.FormattingEnabled = true;
            this.colliderTypeCombobox.Items.AddRange(new object[] {
            "Box",
            "Sphere",
            "Mesh"});
            this.colliderTypeCombobox.Location = new System.Drawing.Point(104, 3);
            this.colliderTypeCombobox.Name = "colliderTypeCombobox";
            this.colliderTypeCombobox.Size = new System.Drawing.Size(177, 24);
            this.colliderTypeCombobox.TabIndex = 1;
            this.colliderTypeCombobox.SelectedIndexChanged += new System.EventHandler(this.colliderTypeCombobox_SelectedIndexChanged);
            // 
            // colliderTypeLabel
            // 
            this.colliderTypeLabel.AutoSize = true;
            this.colliderTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colliderTypeLabel.Location = new System.Drawing.Point(3, 6);
            this.colliderTypeLabel.Name = "colliderTypeLabel";
            this.colliderTypeLabel.Size = new System.Drawing.Size(95, 17);
            this.colliderTypeLabel.TabIndex = 0;
            this.colliderTypeLabel.Text = "Collider Type:";
            // 
            // propertySetOptionsBox
            // 
            this.propertySetOptionsBox.Controls.Add(this.propertyOptionsLayoutPanel);
            this.propertySetOptionsBox.Location = new System.Drawing.Point(0, 0);
            this.propertySetOptionsBox.Name = "propertySetOptionsBox";
            this.propertySetOptionsBox.Size = new System.Drawing.Size(290, 59);
            this.propertySetOptionsBox.TabIndex = 0;
            this.propertySetOptionsBox.TabStop = false;
            this.propertySetOptionsBox.Text = "Property Set Options";
            // 
            // propertyOptionsLayoutPanel
            // 
            this.propertyOptionsLayoutPanel.ColumnCount = 2;
            this.propertyOptionsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.propertyOptionsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 142F));
            this.propertyOptionsLayoutPanel.Controls.Add(this.removeButton, 0, 0);
            this.propertyOptionsLayoutPanel.Controls.Add(this.changeNameButton, 1, 0);
            this.propertyOptionsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyOptionsLayoutPanel.Location = new System.Drawing.Point(3, 18);
            this.propertyOptionsLayoutPanel.Name = "propertyOptionsLayoutPanel";
            this.propertyOptionsLayoutPanel.RowCount = 1;
            this.propertyOptionsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.propertyOptionsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.propertyOptionsLayoutPanel.Size = new System.Drawing.Size(284, 38);
            this.propertyOptionsLayoutPanel.TabIndex = 0;
            // 
            // removeButton
            // 
            this.removeButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeButton.Location = new System.Drawing.Point(3, 3);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(136, 32);
            this.removeButton.TabIndex = 0;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // changeNameButton
            // 
            this.changeNameButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changeNameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.changeNameButton.Location = new System.Drawing.Point(145, 3);
            this.changeNameButton.Name = "changeNameButton";
            this.changeNameButton.Size = new System.Drawing.Size(136, 32);
            this.changeNameButton.TabIndex = 1;
            this.changeNameButton.Text = "Change Name";
            this.changeNameButton.UseVisualStyleBackColor = true;
            this.changeNameButton.Click += new System.EventHandler(this.changeNameButton_Click);
            // 
            // meshPropertiesTable
            // 
            this.meshPropertiesTable.Location = new System.Drawing.Point(0, 0);
            this.meshPropertiesTable.Name = "meshPropertiesTable";
            this.meshPropertiesTable.Size = new System.Drawing.Size(200, 100);
            this.meshPropertiesTable.TabIndex = 0;
            // 
            // inventorTreeView
            // 
            this.inventorTreeView.Location = new System.Drawing.Point(3, 3);
            this.inventorTreeView.Name = "inventorTreeView";
            this.inventorTreeView.Size = new System.Drawing.Size(295, 354);
            this.inventorTreeView.TabIndex = 3;
            // 
            // ComponentPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.inventorTreeView);
            this.Controls.Add(this.inventorActionsPanel);
            this.Controls.Add(this.propertiesScrollablePanel);
            this.Name = "ComponentPropertiesForm";
            this.Size = new System.Drawing.Size(600, 400);
            this.inventorActionsPanel.ResumeLayout(false);
            this.propertiesScrollablePanel.ResumeLayout(false);
            this.propertiesScrollablePanel.PerformLayout();
            this.jointsGroupBox.ResumeLayout(false);
            this.jointsGroupBox.PerformLayout();
            this.physicalPropertiesGroupBox.ResumeLayout(false);
            this.physicalPropertiesGroupBox.PerformLayout();
            this.dynamicGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).EndInit();
            this.frictionLabelsLayoutPanel.ResumeLayout(false);
            this.frictionLabelsLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).EndInit();
            this.meshPropertiesGroupBox.ResumeLayout(false);
            this.colliderTypePanel.ResumeLayout(false);
            this.colliderTypePanel.PerformLayout();
            this.propertySetOptionsBox.ResumeLayout(false);
            this.propertyOptionsLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }


        #endregion

        private System.Windows.Forms.TableLayoutPanel inventorActionsPanel;
        private System.Windows.Forms.Button inventorSelectButton;
        private System.Windows.Forms.Button addSelectionButton;
        private System.Windows.Forms.Panel propertiesScrollablePanel;
        private System.Windows.Forms.GroupBox propertySetOptionsBox;
        private System.Windows.Forms.TableLayoutPanel propertyOptionsLayoutPanel;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.TableLayoutPanel meshPropertiesTable;
        private System.Windows.Forms.Button changeNameButton;
        private System.Windows.Forms.GroupBox meshPropertiesGroupBox;
        private System.Windows.Forms.Panel colliderTypePanel;
        private System.Windows.Forms.ComboBox colliderTypeCombobox;
        private System.Windows.Forms.Label colliderTypeLabel;
        private System.Windows.Forms.GroupBox physicalPropertiesGroupBox;
        private System.Windows.Forms.Label frictionLabel;
        private System.Windows.Forms.TableLayoutPanel frictionLabelsLayoutPanel;
        private System.Windows.Forms.Label iceLabel;
        private System.Windows.Forms.Label carpetLabel;
        private System.Windows.Forms.Label rubberLabel;
        private System.Windows.Forms.TrackBar frictionTrackBar;
        private System.Windows.Forms.GroupBox dynamicGroupBox;
        private System.Windows.Forms.CheckBox dynamicCheckBox;
        private System.Windows.Forms.NumericUpDown massNumericUpDown;
        private System.Windows.Forms.Label massLabel;
        public InventorTreeView inventorTreeView;
        private System.Windows.Forms.GroupBox jointsGroupBox;
        private System.Windows.Forms.CheckBox jointCheckBox;
        private System.Windows.Forms.Label jointTypeLabel;
        private System.Windows.Forms.ComboBox jointComboBox;
    }
}