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
            this.allOptionsTableLayout = new System.Windows.Forms.TableLayoutPanel();
            this.jointsGroupBox = new System.Windows.Forms.GroupBox();
            this.jointTypeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.jointTypeLabel = new System.Windows.Forms.Label();
            this.jointComboBox = new System.Windows.Forms.ComboBox();
            this.meshPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.meshPropertiesTable = new System.Windows.Forms.TableLayoutPanel();
            this.colliderTypeLayout = new System.Windows.Forms.TableLayoutPanel();
            this.colliderTypeCombobox = new System.Windows.Forms.ComboBox();
            this.colliderTypeLabel = new System.Windows.Forms.Label();
            this.propertySetOptionsBox = new System.Windows.Forms.GroupBox();
            this.propertyOptionsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.removeButton = new System.Windows.Forms.Button();
            this.changeNameButton = new System.Windows.Forms.Button();
            this.physicalPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.physicalPropertiesLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.frictionLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.frictionLabel = new System.Windows.Forms.Label();
            this.frictionTrackBar = new System.Windows.Forms.TrackBar();
            this.frictionLabelsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.iceLabel = new System.Windows.Forms.Label();
            this.carpetLabel = new System.Windows.Forms.Label();
            this.rubberLabel = new System.Windows.Forms.Label();
            this.dynamicGroupBox = new System.Windows.Forms.GroupBox();
            this.massLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.typeLabel = new System.Windows.Forms.Label();
            this.massLabel = new System.Windows.Forms.Label();
            this.massNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.massTypeComboBox = new System.Windows.Forms.ComboBox();
            this.explanationLabel = new System.Windows.Forms.Label();
            this.jointCheckBox = new System.Windows.Forms.CheckBox();
            this.dynamicCheckBox = new System.Windows.Forms.CheckBox();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.inventorTreeView = new InternalFieldExporter.InventorTreeView(this.components);
            this.inventorActionsPanel.SuspendLayout();
            this.propertiesScrollablePanel.SuspendLayout();
            this.allOptionsTableLayout.SuspendLayout();
            this.jointsGroupBox.SuspendLayout();
            this.jointTypeLayout.SuspendLayout();
            this.meshPropertiesGroupBox.SuspendLayout();
            this.meshPropertiesTable.SuspendLayout();
            this.colliderTypeLayout.SuspendLayout();
            this.propertySetOptionsBox.SuspendLayout();
            this.propertyOptionsLayoutPanel.SuspendLayout();
            this.physicalPropertiesGroupBox.SuspendLayout();
            this.physicalPropertiesLayoutPanel.SuspendLayout();
            this.frictionLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).BeginInit();
            this.frictionLabelsLayoutPanel.SuspendLayout();
            this.dynamicGroupBox.SuspendLayout();
            this.massLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).BeginInit();
            this.mainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // inventorActionsPanel
            // 
            this.inventorActionsPanel.AutoSize = true;
            this.inventorActionsPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.inventorActionsPanel.ColumnCount = 2;
            this.inventorActionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.Controls.Add(this.inventorSelectButton, 0, 0);
            this.inventorActionsPanel.Controls.Add(this.addSelectionButton, 1, 0);
            this.inventorActionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.inventorActionsPanel.Location = new System.Drawing.Point(3, 449);
            this.inventorActionsPanel.Name = "inventorActionsPanel";
            this.inventorActionsPanel.RowCount = 1;
            this.inventorActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.inventorActionsPanel.Size = new System.Drawing.Size(323, 29);
            this.inventorActionsPanel.TabIndex = 0;
            // 
            // inventorSelectButton
            // 
            this.inventorSelectButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.inventorSelectButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.inventorSelectButton.Location = new System.Drawing.Point(3, 3);
            this.inventorSelectButton.Name = "inventorSelectButton";
            this.inventorSelectButton.Size = new System.Drawing.Size(155, 23);
            this.inventorSelectButton.TabIndex = 0;
            this.inventorSelectButton.Text = "Select in Inventor";
            this.inventorSelectButton.UseVisualStyleBackColor = true;
            this.inventorSelectButton.Click += new System.EventHandler(this.inventorSelectButton_Click);
            // 
            // addSelectionButton
            // 
            this.addSelectionButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.addSelectionButton.Enabled = false;
            this.addSelectionButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.addSelectionButton.Location = new System.Drawing.Point(164, 3);
            this.addSelectionButton.Name = "addSelectionButton";
            this.addSelectionButton.Size = new System.Drawing.Size(156, 23);
            this.addSelectionButton.TabIndex = 1;
            this.addSelectionButton.Text = "Add Selection";
            this.addSelectionButton.UseVisualStyleBackColor = true;
            this.addSelectionButton.Click += new System.EventHandler(this.addSelectionButton_Click);
            // 
            // propertiesScrollablePanel
            // 
            this.propertiesScrollablePanel.AutoScroll = true;
            this.propertiesScrollablePanel.Controls.Add(this.allOptionsTableLayout);
            this.propertiesScrollablePanel.Controls.Add(this.jointCheckBox);
            this.propertiesScrollablePanel.Controls.Add(this.dynamicCheckBox);
            this.propertiesScrollablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesScrollablePanel.Location = new System.Drawing.Point(332, 3);
            this.propertiesScrollablePanel.Name = "propertiesScrollablePanel";
            this.mainLayout.SetRowSpan(this.propertiesScrollablePanel, 2);
            this.propertiesScrollablePanel.Size = new System.Drawing.Size(324, 475);
            this.propertiesScrollablePanel.TabIndex = 2;
            // 
            // allOptionsTableLayout
            // 
            this.allOptionsTableLayout.AutoSize = true;
            this.allOptionsTableLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.allOptionsTableLayout.ColumnCount = 1;
            this.allOptionsTableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.allOptionsTableLayout.Controls.Add(this.jointsGroupBox, 0, 3);
            this.allOptionsTableLayout.Controls.Add(this.meshPropertiesGroupBox, 0, 1);
            this.allOptionsTableLayout.Controls.Add(this.propertySetOptionsBox, 0, 0);
            this.allOptionsTableLayout.Controls.Add(this.physicalPropertiesGroupBox, 0, 2);
            this.allOptionsTableLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.allOptionsTableLayout.Location = new System.Drawing.Point(0, 0);
            this.allOptionsTableLayout.Name = "allOptionsTableLayout";
            this.allOptionsTableLayout.RowCount = 4;
            this.allOptionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.allOptionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.allOptionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.allOptionsTableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.allOptionsTableLayout.Size = new System.Drawing.Size(324, 387);
            this.allOptionsTableLayout.TabIndex = 0;
            // 
            // jointsGroupBox
            // 
            this.jointsGroupBox.AutoSize = true;
            this.jointsGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.jointsGroupBox.Controls.Add(this.jointTypeLayout);
            this.jointsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jointsGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointsGroupBox.Location = new System.Drawing.Point(3, 330);
            this.jointsGroupBox.Name = "jointsGroupBox";
            this.jointsGroupBox.Size = new System.Drawing.Size(318, 54);
            this.jointsGroupBox.TabIndex = 3;
            this.jointsGroupBox.TabStop = false;
            // 
            // jointTypeLayout
            // 
            this.jointTypeLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.jointTypeLayout.ColumnCount = 2;
            this.jointTypeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.jointTypeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.jointTypeLayout.Controls.Add(this.jointTypeLabel, 0, 0);
            this.jointTypeLayout.Controls.Add(this.jointComboBox, 1, 0);
            this.jointTypeLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.jointTypeLayout.Location = new System.Drawing.Point(3, 18);
            this.jointTypeLayout.Name = "jointTypeLayout";
            this.jointTypeLayout.RowCount = 1;
            this.jointTypeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.jointTypeLayout.Size = new System.Drawing.Size(312, 33);
            this.jointTypeLayout.TabIndex = 3;
            // 
            // jointTypeLabel
            // 
            this.jointTypeLabel.AutoSize = true;
            this.jointTypeLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.jointTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointTypeLabel.Location = new System.Drawing.Point(3, 3);
            this.jointTypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.jointTypeLabel.Name = "jointTypeLabel";
            this.jointTypeLabel.Size = new System.Drawing.Size(78, 27);
            this.jointTypeLabel.TabIndex = 2;
            this.jointTypeLabel.Text = "Joint Type:";
            this.jointTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.jointTypeLabel.Click += new System.EventHandler(this.jointTypeLabel_Click);
            // 
            // jointComboBox
            // 
            this.jointComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.jointComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointComboBox.Enabled = false;
            this.jointComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointComboBox.FormattingEnabled = true;
            this.jointComboBox.Items.AddRange(new object[] {
            "Rotational",
            "Linear"});
            this.jointComboBox.Location = new System.Drawing.Point(87, 3);
            this.jointComboBox.Name = "jointComboBox";
            this.jointComboBox.Size = new System.Drawing.Size(222, 24);
            this.jointComboBox.TabIndex = 1;
            this.jointComboBox.SelectedIndexChanged += new System.EventHandler(this.jointComboBox_SelectedIndexChanged);
            // 
            // meshPropertiesGroupBox
            // 
            this.meshPropertiesGroupBox.AutoSize = true;
            this.meshPropertiesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.meshPropertiesGroupBox.Controls.Add(this.meshPropertiesTable);
            this.meshPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.meshPropertiesGroupBox.Location = new System.Drawing.Point(3, 59);
            this.meshPropertiesGroupBox.Name = "meshPropertiesGroupBox";
            this.meshPropertiesGroupBox.Size = new System.Drawing.Size(318, 51);
            this.meshPropertiesGroupBox.TabIndex = 1;
            this.meshPropertiesGroupBox.TabStop = false;
            this.meshPropertiesGroupBox.Text = "Mesh Properties";
            // 
            // meshPropertiesTable
            // 
            this.meshPropertiesTable.AutoSize = true;
            this.meshPropertiesTable.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.meshPropertiesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.meshPropertiesTable.Controls.Add(this.colliderTypeLayout, 0, 0);
            this.meshPropertiesTable.Dock = System.Windows.Forms.DockStyle.Top;
            this.meshPropertiesTable.Location = new System.Drawing.Point(3, 18);
            this.meshPropertiesTable.Name = "meshPropertiesTable";
            this.meshPropertiesTable.RowCount = 2;
            this.meshPropertiesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.meshPropertiesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.meshPropertiesTable.Size = new System.Drawing.Size(312, 30);
            this.meshPropertiesTable.TabIndex = 0;
            // 
            // colliderTypeLayout
            // 
            this.colliderTypeLayout.AutoSize = true;
            this.colliderTypeLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.colliderTypeLayout.ColumnCount = 2;
            this.colliderTypeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.colliderTypeLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.colliderTypeLayout.Controls.Add(this.colliderTypeCombobox, 1, 0);
            this.colliderTypeLayout.Controls.Add(this.colliderTypeLabel, 0, 0);
            this.colliderTypeLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.colliderTypeLayout.Location = new System.Drawing.Point(0, 0);
            this.colliderTypeLayout.Margin = new System.Windows.Forms.Padding(0);
            this.colliderTypeLayout.Name = "colliderTypeLayout";
            this.colliderTypeLayout.RowCount = 1;
            this.colliderTypeLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.colliderTypeLayout.Size = new System.Drawing.Size(312, 30);
            this.colliderTypeLayout.TabIndex = 0;
            // 
            // colliderTypeCombobox
            // 
            this.colliderTypeCombobox.Dock = System.Windows.Forms.DockStyle.Top;
            this.colliderTypeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colliderTypeCombobox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colliderTypeCombobox.FormattingEnabled = true;
            this.colliderTypeCombobox.Items.AddRange(new object[] {
            "Box",
            "Sphere",
            "Mesh"});
            this.colliderTypeCombobox.Location = new System.Drawing.Point(104, 3);
            this.colliderTypeCombobox.Name = "colliderTypeCombobox";
            this.colliderTypeCombobox.Size = new System.Drawing.Size(205, 24);
            this.colliderTypeCombobox.TabIndex = 1;
            this.colliderTypeCombobox.SelectedIndexChanged += new System.EventHandler(this.colliderTypeCombobox_SelectedIndexChanged);
            // 
            // colliderTypeLabel
            // 
            this.colliderTypeLabel.AutoSize = true;
            this.colliderTypeLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.colliderTypeLabel.Location = new System.Drawing.Point(3, 3);
            this.colliderTypeLabel.Margin = new System.Windows.Forms.Padding(3);
            this.colliderTypeLabel.Name = "colliderTypeLabel";
            this.colliderTypeLabel.Size = new System.Drawing.Size(95, 17);
            this.colliderTypeLabel.TabIndex = 0;
            this.colliderTypeLabel.Text = "Collider Type:";
            // 
            // propertySetOptionsBox
            // 
            this.propertySetOptionsBox.AutoSize = true;
            this.propertySetOptionsBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertySetOptionsBox.Controls.Add(this.propertyOptionsLayoutPanel);
            this.propertySetOptionsBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertySetOptionsBox.Location = new System.Drawing.Point(3, 3);
            this.propertySetOptionsBox.Name = "propertySetOptionsBox";
            this.propertySetOptionsBox.Size = new System.Drawing.Size(318, 50);
            this.propertySetOptionsBox.TabIndex = 0;
            this.propertySetOptionsBox.TabStop = false;
            this.propertySetOptionsBox.Text = "Property Set Options";
            // 
            // propertyOptionsLayoutPanel
            // 
            this.propertyOptionsLayoutPanel.AutoSize = true;
            this.propertyOptionsLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertyOptionsLayoutPanel.ColumnCount = 2;
            this.propertyOptionsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.propertyOptionsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.propertyOptionsLayoutPanel.Controls.Add(this.removeButton, 0, 0);
            this.propertyOptionsLayoutPanel.Controls.Add(this.changeNameButton, 1, 0);
            this.propertyOptionsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertyOptionsLayoutPanel.Location = new System.Drawing.Point(3, 18);
            this.propertyOptionsLayoutPanel.Name = "propertyOptionsLayoutPanel";
            this.propertyOptionsLayoutPanel.RowCount = 1;
            this.propertyOptionsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertyOptionsLayoutPanel.Size = new System.Drawing.Size(312, 29);
            this.propertyOptionsLayoutPanel.TabIndex = 0;
            // 
            // removeButton
            // 
            this.removeButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.removeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.removeButton.Location = new System.Drawing.Point(3, 3);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(150, 23);
            this.removeButton.TabIndex = 0;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // changeNameButton
            // 
            this.changeNameButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.changeNameButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.changeNameButton.Location = new System.Drawing.Point(159, 3);
            this.changeNameButton.Name = "changeNameButton";
            this.changeNameButton.Size = new System.Drawing.Size(150, 23);
            this.changeNameButton.TabIndex = 1;
            this.changeNameButton.Text = "Change Name";
            this.changeNameButton.UseVisualStyleBackColor = true;
            this.changeNameButton.Click += new System.EventHandler(this.changeNameButton_Click);
            // 
            // physicalPropertiesGroupBox
            // 
            this.physicalPropertiesGroupBox.AutoSize = true;
            this.physicalPropertiesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.physicalPropertiesGroupBox.Controls.Add(this.physicalPropertiesLayoutPanel);
            this.physicalPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.physicalPropertiesGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.physicalPropertiesGroupBox.Location = new System.Drawing.Point(3, 116);
            this.physicalPropertiesGroupBox.Name = "physicalPropertiesGroupBox";
            this.physicalPropertiesGroupBox.Size = new System.Drawing.Size(318, 208);
            this.physicalPropertiesGroupBox.TabIndex = 2;
            this.physicalPropertiesGroupBox.TabStop = false;
            this.physicalPropertiesGroupBox.Text = "Physical Properties";
            // 
            // physicalPropertiesLayoutPanel
            // 
            this.physicalPropertiesLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.physicalPropertiesLayoutPanel.ColumnCount = 1;
            this.physicalPropertiesLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.physicalPropertiesLayoutPanel.Controls.Add(this.frictionLayoutPanel, 0, 0);
            this.physicalPropertiesLayoutPanel.Controls.Add(this.dynamicGroupBox, 0, 1);
            this.physicalPropertiesLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.physicalPropertiesLayoutPanel.Location = new System.Drawing.Point(3, 18);
            this.physicalPropertiesLayoutPanel.Name = "physicalPropertiesLayoutPanel";
            this.physicalPropertiesLayoutPanel.RowCount = 2;
            this.physicalPropertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.physicalPropertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.physicalPropertiesLayoutPanel.Size = new System.Drawing.Size(312, 187);
            this.physicalPropertiesLayoutPanel.TabIndex = 6;
            // 
            // frictionLayoutPanel
            // 
            this.frictionLayoutPanel.AutoSize = true;
            this.frictionLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.frictionLayoutPanel.ColumnCount = 2;
            this.frictionLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.frictionLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.frictionLayoutPanel.Controls.Add(this.frictionLabel, 0, 0);
            this.frictionLayoutPanel.Controls.Add(this.frictionTrackBar, 1, 0);
            this.frictionLayoutPanel.Controls.Add(this.frictionLabelsLayoutPanel, 1, 1);
            this.frictionLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.frictionLayoutPanel.Location = new System.Drawing.Point(3, 3);
            this.frictionLayoutPanel.Name = "frictionLayoutPanel";
            this.frictionLayoutPanel.RowCount = 2;
            this.frictionLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.frictionLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.frictionLayoutPanel.Size = new System.Drawing.Size(306, 66);
            this.frictionLayoutPanel.TabIndex = 5;
            // 
            // frictionLabel
            // 
            this.frictionLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.frictionLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.frictionLabel.Location = new System.Drawing.Point(3, 3);
            this.frictionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.frictionLabel.Name = "frictionLabel";
            this.frictionLabel.Size = new System.Drawing.Size(58, 34);
            this.frictionLabel.TabIndex = 0;
            this.frictionLabel.Text = "Friction: 50/100";
            this.frictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.frictionLabel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.frictionLabel_MouseDoubleClick);
            // 
            // frictionTrackBar
            // 
            this.frictionTrackBar.AutoSize = false;
            this.frictionTrackBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.frictionTrackBar.Location = new System.Drawing.Point(67, 3);
            this.frictionTrackBar.Maximum = 100;
            this.frictionTrackBar.Name = "frictionTrackBar";
            this.frictionTrackBar.Size = new System.Drawing.Size(236, 31);
            this.frictionTrackBar.TabIndex = 1;
            this.frictionTrackBar.Value = 50;
            this.frictionTrackBar.Scroll += new System.EventHandler(this.frictionTrackBar_Scroll);
            // 
            // frictionLabelsLayoutPanel
            // 
            this.frictionLabelsLayoutPanel.ColumnCount = 3;
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.frictionLabelsLayoutPanel.Controls.Add(this.iceLabel, 0, 0);
            this.frictionLabelsLayoutPanel.Controls.Add(this.carpetLabel, 1, 0);
            this.frictionLabelsLayoutPanel.Controls.Add(this.rubberLabel, 2, 0);
            this.frictionLabelsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.frictionLabelsLayoutPanel.Location = new System.Drawing.Point(67, 43);
            this.frictionLabelsLayoutPanel.Name = "frictionLabelsLayoutPanel";
            this.frictionLabelsLayoutPanel.RowCount = 1;
            this.frictionLabelsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.frictionLabelsLayoutPanel.Size = new System.Drawing.Size(236, 20);
            this.frictionLabelsLayoutPanel.TabIndex = 2;
            // 
            // iceLabel
            // 
            this.iceLabel.AutoSize = true;
            this.iceLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iceLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.iceLabel.Location = new System.Drawing.Point(3, 0);
            this.iceLabel.Name = "iceLabel";
            this.iceLabel.Size = new System.Drawing.Size(72, 20);
            this.iceLabel.TabIndex = 0;
            this.iceLabel.Text = "Ice";
            this.iceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // carpetLabel
            // 
            this.carpetLabel.AutoSize = true;
            this.carpetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.carpetLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.carpetLabel.Location = new System.Drawing.Point(81, 0);
            this.carpetLabel.Name = "carpetLabel";
            this.carpetLabel.Size = new System.Drawing.Size(72, 20);
            this.carpetLabel.TabIndex = 1;
            this.carpetLabel.Text = "Carpet";
            this.carpetLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rubberLabel
            // 
            this.rubberLabel.AutoSize = true;
            this.rubberLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rubberLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rubberLabel.Location = new System.Drawing.Point(159, 0);
            this.rubberLabel.Name = "rubberLabel";
            this.rubberLabel.Size = new System.Drawing.Size(74, 20);
            this.rubberLabel.TabIndex = 2;
            this.rubberLabel.Text = "Rubber";
            this.rubberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // dynamicGroupBox
            // 
            this.dynamicGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dynamicGroupBox.Controls.Add(this.massLayoutPanel);
            this.dynamicGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.dynamicGroupBox.Location = new System.Drawing.Point(3, 75);
            this.dynamicGroupBox.Name = "dynamicGroupBox";
            this.dynamicGroupBox.Size = new System.Drawing.Size(306, 109);
            this.dynamicGroupBox.TabIndex = 3;
            this.dynamicGroupBox.TabStop = false;
            // 
            // massLayoutPanel
            // 
            this.massLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.massLayoutPanel.ColumnCount = 2;
            this.massLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.massLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.massLayoutPanel.Controls.Add(this.typeLabel, 0, 1);
            this.massLayoutPanel.Controls.Add(this.massLabel, 0, 0);
            this.massLayoutPanel.Controls.Add(this.massNumericUpDown, 1, 0);
            this.massLayoutPanel.Controls.Add(this.massTypeComboBox, 1, 1);
            this.massLayoutPanel.Controls.Add(this.explanationLabel, 1, 2);
            this.massLayoutPanel.Location = new System.Drawing.Point(0, 12);
            this.massLayoutPanel.Name = "massLayoutPanel";
            this.massLayoutPanel.RowCount = 3;
            this.massLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.massLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.massLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.massLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.massLayoutPanel.Size = new System.Drawing.Size(300, 139);
            this.massLayoutPanel.TabIndex = 5;
            this.massLayoutPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.massLayoutPanel_Paint);
            // 
            // typeLabel
            // 
            this.typeLabel.AutoSize = true;
            this.typeLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.typeLabel.Location = new System.Drawing.Point(3, 28);
            this.typeLabel.Name = "typeLabel";
            this.typeLabel.Size = new System.Drawing.Size(48, 30);
            this.typeLabel.TabIndex = 3;
            this.typeLabel.Text = "Type: ";
            this.typeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // massLabel
            // 
            this.massLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.massLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.massLabel.Location = new System.Drawing.Point(3, 3);
            this.massLabel.Margin = new System.Windows.Forms.Padding(3);
            this.massLabel.Name = "massLabel";
            this.massLabel.Size = new System.Drawing.Size(63, 22);
            this.massLabel.TabIndex = 0;
            this.massLabel.Text = "Mass:";
            this.massLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // massNumericUpDown
            // 
            this.massNumericUpDown.DecimalPlaces = 2;
            this.massNumericUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.massNumericUpDown.Enabled = false;
            this.massNumericUpDown.Location = new System.Drawing.Point(72, 3);
            this.massNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.massNumericUpDown.Name = "massNumericUpDown";
            this.massNumericUpDown.Size = new System.Drawing.Size(225, 22);
            this.massNumericUpDown.TabIndex = 1;
            // 
            // massTypeComboBox
            // 
            this.massTypeComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.massTypeComboBox.FormattingEnabled = true;
            this.massTypeComboBox.Items.AddRange(new object[] {
            "Dynamic",
            "Jointed",
            "(none)"});
            this.massTypeComboBox.Location = new System.Drawing.Point(72, 31);
            this.massTypeComboBox.Name = "massTypeComboBox";
            this.massTypeComboBox.Size = new System.Drawing.Size(225, 24);
            this.massTypeComboBox.TabIndex = 4;
            // 
            // explanationLabel
            // 
            this.massLayoutPanel.SetColumnSpan(this.explanationLabel, 2);
            this.explanationLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.explanationLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.explanationLabel.Location = new System.Drawing.Point(3, 58);
            this.explanationLabel.Name = "explanationLabel";
            this.explanationLabel.Size = new System.Drawing.Size(294, 81);
            this.explanationLabel.TabIndex = 5;
            this.explanationLabel.Text = "If the property set includes a game object, select Dynamic as your mass type. If " +
    "it includes a jointed assembly, select Jointed. Otherwise, select none. ";
            this.explanationLabel.Click += new System.EventHandler(this.explanationLabel_Click);
            // 
            // jointCheckBox
            // 
            this.jointCheckBox.AutoSize = true;
            this.jointCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.jointCheckBox.Location = new System.Drawing.Point(95, 446);
            this.jointCheckBox.Name = "jointCheckBox";
            this.jointCheckBox.Size = new System.Drawing.Size(56, 21);
            this.jointCheckBox.TabIndex = 0;
            this.jointCheckBox.Text = "Joint";
            this.jointCheckBox.UseVisualStyleBackColor = true;
            this.jointCheckBox.CheckedChanged += new System.EventHandler(this.jointCheckBox_CheckChanged);
            // 
            // dynamicCheckBox
            // 
            this.dynamicCheckBox.AutoSize = true;
            this.dynamicCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.dynamicCheckBox.Location = new System.Drawing.Point(9, 446);
            this.dynamicCheckBox.Name = "dynamicCheckBox";
            this.dynamicCheckBox.Size = new System.Drawing.Size(80, 21);
            this.dynamicCheckBox.TabIndex = 4;
            this.dynamicCheckBox.Text = "Dynamic";
            this.dynamicCheckBox.UseVisualStyleBackColor = true;
            this.dynamicCheckBox.CheckedChanged += new System.EventHandler(this.dynamicCheckBox_CheckedChanged);
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 2;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.mainLayout.Controls.Add(this.propertiesScrollablePanel, 1, 0);
            this.mainLayout.Controls.Add(this.inventorActionsPanel, 0, 1);
            this.mainLayout.Controls.Add(this.inventorTreeView, 0, 0);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(0, 0);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 2;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayout.Size = new System.Drawing.Size(659, 481);
            this.mainLayout.TabIndex = 4;
            // 
            // inventorTreeView
            // 
            this.inventorTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventorTreeView.Location = new System.Drawing.Point(3, 3);
            this.inventorTreeView.Name = "inventorTreeView";
            this.inventorTreeView.Size = new System.Drawing.Size(323, 440);
            this.inventorTreeView.TabIndex = 3;
            // 
            // ComponentPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.mainLayout);
            this.Name = "ComponentPropertiesForm";
            this.Size = new System.Drawing.Size(659, 481);
            this.inventorActionsPanel.ResumeLayout(false);
            this.propertiesScrollablePanel.ResumeLayout(false);
            this.propertiesScrollablePanel.PerformLayout();
            this.allOptionsTableLayout.ResumeLayout(false);
            this.allOptionsTableLayout.PerformLayout();
            this.jointsGroupBox.ResumeLayout(false);
            this.jointTypeLayout.ResumeLayout(false);
            this.jointTypeLayout.PerformLayout();
            this.meshPropertiesGroupBox.ResumeLayout(false);
            this.meshPropertiesGroupBox.PerformLayout();
            this.meshPropertiesTable.ResumeLayout(false);
            this.meshPropertiesTable.PerformLayout();
            this.colliderTypeLayout.ResumeLayout(false);
            this.colliderTypeLayout.PerformLayout();
            this.propertySetOptionsBox.ResumeLayout(false);
            this.propertySetOptionsBox.PerformLayout();
            this.propertyOptionsLayoutPanel.ResumeLayout(false);
            this.physicalPropertiesGroupBox.ResumeLayout(false);
            this.physicalPropertiesLayoutPanel.ResumeLayout(false);
            this.physicalPropertiesLayoutPanel.PerformLayout();
            this.frictionLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).EndInit();
            this.frictionLabelsLayoutPanel.ResumeLayout(false);
            this.frictionLabelsLayoutPanel.PerformLayout();
            this.dynamicGroupBox.ResumeLayout(false);
            this.massLayoutPanel.ResumeLayout(false);
            this.massLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).EndInit();
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
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
        private System.Windows.Forms.TableLayoutPanel allOptionsTableLayout;
        private System.Windows.Forms.TableLayoutPanel colliderTypeLayout;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.TableLayoutPanel frictionLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel jointTypeLayout;
        private System.Windows.Forms.TableLayoutPanel physicalPropertiesLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel massLayoutPanel;
        private System.Windows.Forms.Label typeLabel;
        private System.Windows.Forms.ComboBox massTypeComboBox;
        private System.Windows.Forms.Label explanationLabel;
    }
}