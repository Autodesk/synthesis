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
            this.inventorActionsPanel = new System.Windows.Forms.TableLayoutPanel();
            this.physicalPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.physicalLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.frictionLabel = new System.Windows.Forms.Label();
            this.frictionLabelsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rubberLabel = new System.Windows.Forms.Label();
            this.iceLabel = new System.Windows.Forms.Label();
            this.carpetLabel = new System.Windows.Forms.Label();
            this.dynamicGroupBox = new System.Windows.Forms.GroupBox();
            this.dynamicLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.massLabel = new System.Windows.Forms.Label();
            this.massNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.gamepieceProperties = new FieldExporter.Components.GamepiecePropertiesForm();
            this.dynamicCheckBox = new System.Windows.Forms.CheckBox();
            this.frictionTrackBar = new System.Windows.Forms.TrackBar();
            this.meshPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.meshPropertiesTable = new System.Windows.Forms.TableLayoutPanel();
            this.colliderTypePanel = new System.Windows.Forms.Panel();
            this.colliderTypeLabel = new System.Windows.Forms.Label();
            this.colliderTypeCombobox = new System.Windows.Forms.ComboBox();
            this.propertySetOptionsBox = new System.Windows.Forms.GroupBox();
            this.propertyOptionsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.changeNameButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.propertiesLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.propertiesScrollablePanel = new System.Windows.Forms.Panel();
            this.inventorTreeView = new FieldExporter.Components.InventorTreeView(this.components);
            this.selectionLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.inventorActionsPanel.SuspendLayout();
            this.physicalPropertiesGroupBox.SuspendLayout();
            this.physicalLayoutPanel.SuspendLayout();
            this.frictionLabelsLayoutPanel.SuspendLayout();
            this.dynamicGroupBox.SuspendLayout();
            this.dynamicLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).BeginInit();
            this.meshPropertiesGroupBox.SuspendLayout();
            this.meshPropertiesTable.SuspendLayout();
            this.colliderTypePanel.SuspendLayout();
            this.propertySetOptionsBox.SuspendLayout();
            this.propertyOptionsLayoutPanel.SuspendLayout();
            this.propertiesLayoutPanel.SuspendLayout();
            this.propertiesScrollablePanel.SuspendLayout();
            this.selectionLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // inventorSelectButton
            // 
            this.inventorSelectButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.inventorSelectButton.Location = new System.Drawing.Point(3, 3);
            this.inventorSelectButton.Name = "inventorSelectButton";
            this.inventorSelectButton.Size = new System.Drawing.Size(146, 23);
            this.inventorSelectButton.TabIndex = 13;
            this.inventorSelectButton.Tag = "";
            this.inventorSelectButton.Text = "Select in Inventor";
            this.inventorSelectButton.UseVisualStyleBackColor = true;
            this.inventorSelectButton.Click += new System.EventHandler(this.inventorSelectButton_Click);
            // 
            // addSelectionButton
            // 
            this.addSelectionButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.addSelectionButton.Enabled = false;
            this.addSelectionButton.Location = new System.Drawing.Point(155, 3);
            this.addSelectionButton.Name = "addSelectionButton";
            this.addSelectionButton.Size = new System.Drawing.Size(146, 23);
            this.addSelectionButton.TabIndex = 14;
            this.addSelectionButton.Text = "Add Selection";
            this.addSelectionButton.UseVisualStyleBackColor = true;
            this.addSelectionButton.Click += new System.EventHandler(this.addSelectionButton_Click);
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
            this.inventorActionsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.inventorActionsPanel.Location = new System.Drawing.Point(0, 371);
            this.inventorActionsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.inventorActionsPanel.Name = "inventorActionsPanel";
            this.inventorActionsPanel.RowCount = 1;
            this.inventorActionsPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.inventorActionsPanel.Size = new System.Drawing.Size(304, 29);
            this.inventorActionsPanel.TabIndex = 8;
            // 
            // physicalPropertiesGroupBox
            // 
            this.physicalPropertiesGroupBox.AutoSize = true;
            this.physicalPropertiesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.physicalPropertiesGroupBox.Controls.Add(this.physicalLayoutPanel);
            this.physicalPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.physicalPropertiesGroupBox.Location = new System.Drawing.Point(3, 109);
            this.physicalPropertiesGroupBox.Name = "physicalPropertiesGroupBox";
            this.physicalPropertiesGroupBox.Size = new System.Drawing.Size(270, 243);
            this.physicalPropertiesGroupBox.TabIndex = 15;
            this.physicalPropertiesGroupBox.TabStop = false;
            this.physicalPropertiesGroupBox.Text = "Physical Properties";
            // 
            // physicalLayoutPanel
            // 
            this.physicalLayoutPanel.AutoSize = true;
            this.physicalLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.physicalLayoutPanel.ColumnCount = 2;
            this.physicalLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.physicalLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.physicalLayoutPanel.Controls.Add(this.frictionLabel, 0, 0);
            this.physicalLayoutPanel.Controls.Add(this.frictionLabelsLayoutPanel, 1, 1);
            this.physicalLayoutPanel.Controls.Add(this.dynamicGroupBox, 0, 2);
            this.physicalLayoutPanel.Controls.Add(this.frictionTrackBar, 1, 0);
            this.physicalLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.physicalLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.physicalLayoutPanel.Name = "physicalLayoutPanel";
            this.physicalLayoutPanel.RowCount = 3;
            this.physicalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.physicalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.physicalLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.physicalLayoutPanel.Size = new System.Drawing.Size(264, 224);
            this.physicalLayoutPanel.TabIndex = 11;
            // 
            // frictionLabel
            // 
            this.frictionLabel.AutoSize = true;
            this.frictionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.frictionLabel.Location = new System.Drawing.Point(3, 3);
            this.frictionLabel.Margin = new System.Windows.Forms.Padding(3);
            this.frictionLabel.Name = "frictionLabel";
            this.physicalLayoutPanel.SetRowSpan(this.frictionLabel, 2);
            this.frictionLabel.Size = new System.Drawing.Size(69, 61);
            this.frictionLabel.TabIndex = 3;
            this.frictionLabel.Text = "Friction:\r\n50/100";
            this.frictionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.frictionLabel.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.frictionLabel_MouseDoubleClick);
            // 
            // frictionLabelsLayoutPanel
            // 
            this.frictionLabelsLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frictionLabelsLayoutPanel.ColumnCount = 3;
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.frictionLabelsLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.frictionLabelsLayoutPanel.Controls.Add(this.rubberLabel, 2, 0);
            this.frictionLabelsLayoutPanel.Controls.Add(this.iceLabel, 0, 0);
            this.frictionLabelsLayoutPanel.Controls.Add(this.carpetLabel, 1, 0);
            this.frictionLabelsLayoutPanel.Location = new System.Drawing.Point(78, 44);
            this.frictionLabelsLayoutPanel.Name = "frictionLabelsLayoutPanel";
            this.frictionLabelsLayoutPanel.RowCount = 1;
            this.frictionLabelsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.frictionLabelsLayoutPanel.Size = new System.Drawing.Size(183, 20);
            this.frictionLabelsLayoutPanel.TabIndex = 10;
            // 
            // rubberLabel
            // 
            this.rubberLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rubberLabel.Location = new System.Drawing.Point(125, 0);
            this.rubberLabel.Name = "rubberLabel";
            this.rubberLabel.Size = new System.Drawing.Size(55, 20);
            this.rubberLabel.TabIndex = 6;
            this.rubberLabel.Text = "Rubber";
            this.rubberLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // iceLabel
            // 
            this.iceLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iceLabel.Location = new System.Drawing.Point(3, 0);
            this.iceLabel.Name = "iceLabel";
            this.iceLabel.Size = new System.Drawing.Size(55, 20);
            this.iceLabel.TabIndex = 5;
            this.iceLabel.Text = "Ice";
            // 
            // carpetLabel
            // 
            this.carpetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.carpetLabel.Location = new System.Drawing.Point(64, 0);
            this.carpetLabel.Name = "carpetLabel";
            this.carpetLabel.Size = new System.Drawing.Size(55, 20);
            this.carpetLabel.TabIndex = 7;
            this.carpetLabel.Text = "Carpet";
            this.carpetLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // dynamicGroupBox
            // 
            this.dynamicGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dynamicGroupBox.AutoSize = true;
            this.dynamicGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.physicalLayoutPanel.SetColumnSpan(this.dynamicGroupBox, 2);
            this.dynamicGroupBox.Controls.Add(this.dynamicLayoutPanel);
            this.dynamicGroupBox.Controls.Add(this.dynamicCheckBox);
            this.dynamicGroupBox.Location = new System.Drawing.Point(3, 70);
            this.dynamicGroupBox.Name = "dynamicGroupBox";
            this.dynamicGroupBox.Size = new System.Drawing.Size(258, 151);
            this.dynamicGroupBox.TabIndex = 9;
            this.dynamicGroupBox.TabStop = false;
            // 
            // dynamicLayoutPanel
            // 
            this.dynamicLayoutPanel.AutoSize = true;
            this.dynamicLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dynamicLayoutPanel.ColumnCount = 2;
            this.dynamicLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.dynamicLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.dynamicLayoutPanel.Controls.Add(this.massLabel, 0, 0);
            this.dynamicLayoutPanel.Controls.Add(this.massNumericUpDown, 1, 0);
            this.dynamicLayoutPanel.Controls.Add(this.gamepieceProperties, 0, 1);
            this.dynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.dynamicLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.dynamicLayoutPanel.Name = "dynamicLayoutPanel";
            this.dynamicLayoutPanel.RowCount = 2;
            this.dynamicLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.dynamicLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.dynamicLayoutPanel.Size = new System.Drawing.Size(252, 132);
            this.dynamicLayoutPanel.TabIndex = 11;
            // 
            // massLabel
            // 
            this.massLabel.AutoSize = true;
            this.massLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.massLabel.Location = new System.Drawing.Point(3, 3);
            this.massLabel.Margin = new System.Windows.Forms.Padding(3);
            this.massLabel.Name = "massLabel";
            this.massLabel.Size = new System.Drawing.Size(57, 20);
            this.massLabel.TabIndex = 9;
            this.massLabel.Text = "Mass (lbs):";
            this.massLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // massNumericUpDown
            // 
            this.massNumericUpDown.DecimalPlaces = 3;
            this.massNumericUpDown.Dock = System.Windows.Forms.DockStyle.Top;
            this.massNumericUpDown.Location = new System.Drawing.Point(66, 3);
            this.massNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.massNumericUpDown.Name = "massNumericUpDown";
            this.massNumericUpDown.Size = new System.Drawing.Size(183, 20);
            this.massNumericUpDown.TabIndex = 10;
            // 
            // gamepieceProperties
            // 
            this.gamepieceProperties.AutoSize = true;
            this.gamepieceProperties.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.dynamicLayoutPanel.SetColumnSpan(this.gamepieceProperties, 2);
            this.gamepieceProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.gamepieceProperties.Location = new System.Drawing.Point(3, 29);
            this.gamepieceProperties.Name = "gamepieceProperties";
            this.gamepieceProperties.Size = new System.Drawing.Size(246, 100);
            this.gamepieceProperties.TabIndex = 11;
            // 
            // dynamicCheckBox
            // 
            this.dynamicCheckBox.AutoSize = true;
            this.dynamicCheckBox.Location = new System.Drawing.Point(6, 0);
            this.dynamicCheckBox.Name = "dynamicCheckBox";
            this.dynamicCheckBox.Size = new System.Drawing.Size(67, 17);
            this.dynamicCheckBox.TabIndex = 8;
            this.dynamicCheckBox.Text = "Dynamic";
            this.dynamicCheckBox.UseVisualStyleBackColor = true;
            this.dynamicCheckBox.CheckedChanged += new System.EventHandler(this.dynamicCheckBox_CheckedChanged);
            // 
            // frictionTrackBar
            // 
            this.frictionTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.frictionTrackBar.AutoSize = false;
            this.frictionTrackBar.Location = new System.Drawing.Point(78, 3);
            this.frictionTrackBar.Maximum = 100;
            this.frictionTrackBar.Name = "frictionTrackBar";
            this.frictionTrackBar.Size = new System.Drawing.Size(183, 35);
            this.frictionTrackBar.TabIndex = 2;
            this.frictionTrackBar.TickFrequency = 50;
            this.frictionTrackBar.Value = 50;
            this.frictionTrackBar.Scroll += new System.EventHandler(this.frictionTrackBar_Scroll);
            // 
            // meshPropertiesGroupBox
            // 
            this.meshPropertiesGroupBox.AutoSize = true;
            this.meshPropertiesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.meshPropertiesGroupBox.Controls.Add(this.meshPropertiesTable);
            this.meshPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meshPropertiesGroupBox.Location = new System.Drawing.Point(3, 57);
            this.meshPropertiesGroupBox.Name = "meshPropertiesGroupBox";
            this.meshPropertiesGroupBox.Size = new System.Drawing.Size(270, 46);
            this.meshPropertiesGroupBox.TabIndex = 17;
            this.meshPropertiesGroupBox.TabStop = false;
            this.meshPropertiesGroupBox.Text = "Mesh Properties";
            // 
            // meshPropertiesTable
            // 
            this.meshPropertiesTable.AutoSize = true;
            this.meshPropertiesTable.ColumnCount = 1;
            this.meshPropertiesTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.meshPropertiesTable.Controls.Add(this.colliderTypePanel, 0, 0);
            this.meshPropertiesTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meshPropertiesTable.Location = new System.Drawing.Point(3, 16);
            this.meshPropertiesTable.Margin = new System.Windows.Forms.Padding(0);
            this.meshPropertiesTable.Name = "meshPropertiesTable";
            this.meshPropertiesTable.RowCount = 2;
            this.meshPropertiesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.meshPropertiesTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.meshPropertiesTable.Size = new System.Drawing.Size(264, 27);
            this.meshPropertiesTable.TabIndex = 2;
            // 
            // colliderTypePanel
            // 
            this.colliderTypePanel.AutoSize = true;
            this.colliderTypePanel.Controls.Add(this.colliderTypeLabel);
            this.colliderTypePanel.Controls.Add(this.colliderTypeCombobox);
            this.colliderTypePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.colliderTypePanel.Location = new System.Drawing.Point(0, 0);
            this.colliderTypePanel.Margin = new System.Windows.Forms.Padding(0);
            this.colliderTypePanel.Name = "colliderTypePanel";
            this.colliderTypePanel.Size = new System.Drawing.Size(264, 27);
            this.colliderTypePanel.TabIndex = 0;
            // 
            // colliderTypeLabel
            // 
            this.colliderTypeLabel.AutoSize = true;
            this.colliderTypeLabel.Location = new System.Drawing.Point(3, 6);
            this.colliderTypeLabel.Name = "colliderTypeLabel";
            this.colliderTypeLabel.Size = new System.Drawing.Size(71, 13);
            this.colliderTypeLabel.TabIndex = 1;
            this.colliderTypeLabel.Text = "Collider Type:";
            // 
            // colliderTypeCombobox
            // 
            this.colliderTypeCombobox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.colliderTypeCombobox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.colliderTypeCombobox.Items.AddRange(new object[] {
            "Box",
            "Sphere",
            "Mesh"});
            this.colliderTypeCombobox.Location = new System.Drawing.Point(80, 3);
            this.colliderTypeCombobox.Name = "colliderTypeCombobox";
            this.colliderTypeCombobox.Size = new System.Drawing.Size(181, 21);
            this.colliderTypeCombobox.TabIndex = 0;
            this.colliderTypeCombobox.SelectedIndexChanged += new System.EventHandler(this.colliderTypeCombobox_SelectedIndexChanged);
            // 
            // propertySetOptionsBox
            // 
            this.propertySetOptionsBox.AutoSize = true;
            this.propertySetOptionsBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertySetOptionsBox.Controls.Add(this.propertyOptionsLayoutPanel);
            this.propertySetOptionsBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertySetOptionsBox.Location = new System.Drawing.Point(3, 3);
            this.propertySetOptionsBox.Name = "propertySetOptionsBox";
            this.propertySetOptionsBox.Size = new System.Drawing.Size(270, 48);
            this.propertySetOptionsBox.TabIndex = 16;
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
            this.propertyOptionsLayoutPanel.Controls.Add(this.changeNameButton, 1, 0);
            this.propertyOptionsLayoutPanel.Controls.Add(this.removeButton, 0, 0);
            this.propertyOptionsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.propertyOptionsLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.propertyOptionsLayoutPanel.Name = "propertyOptionsLayoutPanel";
            this.propertyOptionsLayoutPanel.RowCount = 1;
            this.propertyOptionsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertyOptionsLayoutPanel.Size = new System.Drawing.Size(264, 29);
            this.propertyOptionsLayoutPanel.TabIndex = 19;
            // 
            // changeNameButton
            // 
            this.changeNameButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.changeNameButton.Location = new System.Drawing.Point(135, 3);
            this.changeNameButton.Name = "changeNameButton";
            this.changeNameButton.Size = new System.Drawing.Size(126, 23);
            this.changeNameButton.TabIndex = 3;
            this.changeNameButton.Text = "Change Name";
            this.changeNameButton.UseVisualStyleBackColor = true;
            this.changeNameButton.Click += new System.EventHandler(this.changeNameButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.removeButton.Location = new System.Drawing.Point(3, 3);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(126, 23);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // propertiesLayoutPanel
            // 
            this.propertiesLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertiesLayoutPanel.AutoSize = true;
            this.propertiesLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertiesLayoutPanel.ColumnCount = 1;
            this.propertiesLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesLayoutPanel.Controls.Add(this.propertySetOptionsBox, 0, 0);
            this.propertiesLayoutPanel.Controls.Add(this.physicalPropertiesGroupBox, 0, 2);
            this.propertiesLayoutPanel.Controls.Add(this.meshPropertiesGroupBox, 0, 1);
            this.propertiesLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.propertiesLayoutPanel.Name = "propertiesLayoutPanel";
            this.propertiesLayoutPanel.RowCount = 3;
            this.propertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertiesLayoutPanel.Size = new System.Drawing.Size(276, 355);
            this.propertiesLayoutPanel.TabIndex = 18;
            // 
            // propertiesScrollablePanel
            // 
            this.propertiesScrollablePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertiesScrollablePanel.AutoScroll = true;
            this.propertiesScrollablePanel.Controls.Add(this.propertiesLayoutPanel);
            this.propertiesScrollablePanel.Location = new System.Drawing.Point(307, 0);
            this.propertiesScrollablePanel.Name = "propertiesScrollablePanel";
            this.propertiesScrollablePanel.Size = new System.Drawing.Size(293, 400);
            this.propertiesScrollablePanel.TabIndex = 19;
            // 
            // inventorTreeView
            // 
            this.inventorTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventorTreeView.Location = new System.Drawing.Point(3, 3);
            this.inventorTreeView.Name = "inventorTreeView";
            this.inventorTreeView.Size = new System.Drawing.Size(298, 365);
            this.inventorTreeView.TabIndex = 12;
            // 
            // selectionLayoutPanel
            // 
            this.selectionLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.selectionLayoutPanel.ColumnCount = 1;
            this.selectionLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.selectionLayoutPanel.Controls.Add(this.inventorTreeView, 0, 0);
            this.selectionLayoutPanel.Controls.Add(this.inventorActionsPanel, 0, 1);
            this.selectionLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.selectionLayoutPanel.Name = "selectionLayoutPanel";
            this.selectionLayoutPanel.RowCount = 2;
            this.selectionLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.selectionLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.selectionLayoutPanel.Size = new System.Drawing.Size(304, 400);
            this.selectionLayoutPanel.TabIndex = 20;
            // 
            // ComponentPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.selectionLayoutPanel);
            this.Controls.Add(this.propertiesScrollablePanel);
            this.Name = "ComponentPropertiesForm";
            this.Size = new System.Drawing.Size(600, 400);
            this.inventorActionsPanel.ResumeLayout(false);
            this.physicalPropertiesGroupBox.ResumeLayout(false);
            this.physicalPropertiesGroupBox.PerformLayout();
            this.physicalLayoutPanel.ResumeLayout(false);
            this.physicalLayoutPanel.PerformLayout();
            this.frictionLabelsLayoutPanel.ResumeLayout(false);
            this.dynamicGroupBox.ResumeLayout(false);
            this.dynamicGroupBox.PerformLayout();
            this.dynamicLayoutPanel.ResumeLayout(false);
            this.dynamicLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).EndInit();
            this.meshPropertiesGroupBox.ResumeLayout(false);
            this.meshPropertiesGroupBox.PerformLayout();
            this.meshPropertiesTable.ResumeLayout(false);
            this.meshPropertiesTable.PerformLayout();
            this.colliderTypePanel.ResumeLayout(false);
            this.colliderTypePanel.PerformLayout();
            this.propertySetOptionsBox.ResumeLayout(false);
            this.propertySetOptionsBox.PerformLayout();
            this.propertyOptionsLayoutPanel.ResumeLayout(false);
            this.propertiesLayoutPanel.ResumeLayout(false);
            this.propertiesLayoutPanel.PerformLayout();
            this.propertiesScrollablePanel.ResumeLayout(false);
            this.propertiesScrollablePanel.PerformLayout();
            this.selectionLayoutPanel.ResumeLayout(false);
            this.selectionLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button inventorSelectButton;
        private System.Windows.Forms.Button addSelectionButton;
        public InventorTreeView inventorTreeView;
        private System.Windows.Forms.TableLayoutPanel inventorActionsPanel;
        private System.Windows.Forms.GroupBox physicalPropertiesGroupBox;
        private System.Windows.Forms.CheckBox dynamicCheckBox;
        private System.Windows.Forms.GroupBox dynamicGroupBox;
        private System.Windows.Forms.NumericUpDown massNumericUpDown;
        private System.Windows.Forms.Label massLabel;
        private System.Windows.Forms.Label rubberLabel;
        private System.Windows.Forms.Label carpetLabel;
        private System.Windows.Forms.Label iceLabel;
        private System.Windows.Forms.TrackBar frictionTrackBar;
        private System.Windows.Forms.Label frictionLabel;
        private System.Windows.Forms.GroupBox meshPropertiesGroupBox;
        private System.Windows.Forms.Label colliderTypeLabel;
        private System.Windows.Forms.ComboBox colliderTypeCombobox;
        private System.Windows.Forms.GroupBox propertySetOptionsBox;
        private System.Windows.Forms.Button changeNameButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.TableLayoutPanel propertiesLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel meshPropertiesTable;
        private System.Windows.Forms.Panel colliderTypePanel;
        private System.Windows.Forms.TableLayoutPanel frictionLabelsLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel propertyOptionsLayoutPanel;
        private System.Windows.Forms.Panel propertiesScrollablePanel;
        private System.Windows.Forms.TableLayoutPanel selectionLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel physicalLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel dynamicLayoutPanel;
        private GamepiecePropertiesForm gamepieceProperties;
    }
}
