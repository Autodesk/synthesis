
namespace BxDFieldExporter {
    partial class ComponentPropertiesForm {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComponentPropertiesForm));
            this.physicalPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.frictionLabelsLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.rubberLabel = new System.Windows.Forms.Label();
            this.iceLabel = new System.Windows.Forms.Label();
            this.carpetLabel = new System.Windows.Forms.Label();
            this.dynamicCheckBox = new System.Windows.Forms.CheckBox();
            this.frictionTrackBar = new System.Windows.Forms.TrackBar();
            this.dynamicGroupBox = new System.Windows.Forms.GroupBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.massNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.massLabel = new System.Windows.Forms.Label();
            this.frictionLabel = new System.Windows.Forms.Label();
            this.meshPropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.meshPropertiesTable = new System.Windows.Forms.TableLayoutPanel();
            this.colliderTypePanel = new System.Windows.Forms.Panel();
            this.colliderTypeLabel = new System.Windows.Forms.Label();
            this.colliderTypeCombobox = new System.Windows.Forms.ComboBox();
            this.changeNameButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.propertiesLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.propertiesScrollablePanel = new System.Windows.Forms.Panel();
            this.physicalPropertiesGroupBox.SuspendLayout();
            this.frictionLabelsLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).BeginInit();
            this.dynamicGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).BeginInit();
            this.meshPropertiesGroupBox.SuspendLayout();
            this.meshPropertiesTable.SuspendLayout();
            this.colliderTypePanel.SuspendLayout();
            this.propertiesLayoutPanel.SuspendLayout();
            this.propertiesScrollablePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // physicalPropertiesGroupBox
            // 
            this.physicalPropertiesGroupBox.Controls.Add(this.btnSave);
            this.physicalPropertiesGroupBox.Controls.Add(this.frictionLabelsLayoutPanel);
            this.physicalPropertiesGroupBox.Controls.Add(this.dynamicCheckBox);
            this.physicalPropertiesGroupBox.Controls.Add(this.frictionTrackBar);
            this.physicalPropertiesGroupBox.Controls.Add(this.dynamicGroupBox);
            this.physicalPropertiesGroupBox.Controls.Add(this.frictionLabel);
            this.physicalPropertiesGroupBox.Location = new System.Drawing.Point(3, 55);
            this.physicalPropertiesGroupBox.Name = "physicalPropertiesGroupBox";
            this.physicalPropertiesGroupBox.Size = new System.Drawing.Size(290, 182);
            this.physicalPropertiesGroupBox.TabIndex = 15;
            this.physicalPropertiesGroupBox.TabStop = false;
            this.physicalPropertiesGroupBox.Text = "Physical Properties";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(3, 145);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(281, 31);
            this.btnSave.TabIndex = 11;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
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
            this.frictionLabelsLayoutPanel.Location = new System.Drawing.Point(74, 57);
            this.frictionLabelsLayoutPanel.Name = "frictionLabelsLayoutPanel";
            this.frictionLabelsLayoutPanel.RowCount = 1;
            this.frictionLabelsLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.frictionLabelsLayoutPanel.Size = new System.Drawing.Size(210, 20);
            this.frictionLabelsLayoutPanel.TabIndex = 10;
            // 
            // rubberLabel
            // 
            this.rubberLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rubberLabel.Location = new System.Drawing.Point(143, 0);
            this.rubberLabel.Name = "rubberLabel";
            this.rubberLabel.Size = new System.Drawing.Size(64, 20);
            this.rubberLabel.TabIndex = 6;
            this.rubberLabel.Text = "Rubber";
            this.rubberLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // iceLabel
            // 
            this.iceLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.iceLabel.Location = new System.Drawing.Point(3, 0);
            this.iceLabel.Name = "iceLabel";
            this.iceLabel.Size = new System.Drawing.Size(64, 20);
            this.iceLabel.TabIndex = 5;
            this.iceLabel.Text = "Ice";
            // 
            // carpetLabel
            // 
            this.carpetLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.carpetLabel.Location = new System.Drawing.Point(73, 0);
            this.carpetLabel.Name = "carpetLabel";
            this.carpetLabel.Size = new System.Drawing.Size(64, 20);
            this.carpetLabel.TabIndex = 7;
            this.carpetLabel.Text = "Carpet";
            this.carpetLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // dynamicCheckBox
            // 
            this.dynamicCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dynamicCheckBox.AutoSize = true;
            this.dynamicCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F);
            this.dynamicCheckBox.Location = new System.Drawing.Point(12, 82);
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
            this.frictionTrackBar.Location = new System.Drawing.Point(70, 21);
            this.frictionTrackBar.Maximum = 100;
            this.frictionTrackBar.Name = "frictionTrackBar";
            this.frictionTrackBar.Size = new System.Drawing.Size(214, 45);
            this.frictionTrackBar.TabIndex = 2;
            this.frictionTrackBar.TickFrequency = 50;
            this.frictionTrackBar.Value = 50;
            this.frictionTrackBar.Scroll += new System.EventHandler(this.frictionTrackBar_Scroll);
            // 
            // dynamicGroupBox
            // 
            this.dynamicGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dynamicGroupBox.Controls.Add(this.comboBox1);
            this.dynamicGroupBox.Controls.Add(this.massNumericUpDown);
            this.dynamicGroupBox.Controls.Add(this.massLabel);
            this.dynamicGroupBox.Enabled = false;
            this.dynamicGroupBox.Location = new System.Drawing.Point(6, 83);
            this.dynamicGroupBox.Name = "dynamicGroupBox";
            this.dynamicGroupBox.Size = new System.Drawing.Size(278, 63);
            this.dynamicGroupBox.TabIndex = 9;
            this.dynamicGroupBox.TabStop = false;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "kgs",
            "lbs"});
            this.comboBox1.Location = new System.Drawing.Point(219, 20);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(53, 21);
            this.comboBox1.TabIndex = 11;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // massNumericUpDown
            // 
            this.massNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.massNumericUpDown.DecimalPlaces = 2;
            this.massNumericUpDown.Location = new System.Drawing.Point(89, 21);
            this.massNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.massNumericUpDown.Name = "massNumericUpDown";
            this.massNumericUpDown.Size = new System.Drawing.Size(124, 20);
            this.massNumericUpDown.TabIndex = 10;
            this.massNumericUpDown.ValueChanged += new System.EventHandler(this.MassChanged);
            // 
            // massLabel
            // 
            this.massLabel.AutoSize = true;
            this.massLabel.Location = new System.Drawing.Point(6, 24);
            this.massLabel.Name = "massLabel";
            this.massLabel.Size = new System.Drawing.Size(57, 13);
            this.massLabel.TabIndex = 9;
            this.massLabel.Text = "Mass (lbs):";
            // 
            // frictionLabel
            // 
            this.frictionLabel.AutoSize = true;
            this.frictionLabel.Location = new System.Drawing.Point(6, 18);
            this.frictionLabel.Name = "frictionLabel";
            this.frictionLabel.Size = new System.Drawing.Size(44, 26);
            this.frictionLabel.TabIndex = 3;
            this.frictionLabel.Text = "Friction:\r\n50/100";
            // 
            // meshPropertiesGroupBox
            // 
            this.meshPropertiesGroupBox.AutoSize = true;
            this.meshPropertiesGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.meshPropertiesGroupBox.Controls.Add(this.meshPropertiesTable);
            this.meshPropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.meshPropertiesGroupBox.Location = new System.Drawing.Point(3, 3);
            this.meshPropertiesGroupBox.Name = "meshPropertiesGroupBox";
            this.meshPropertiesGroupBox.Size = new System.Drawing.Size(327, 46);
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
            this.meshPropertiesTable.Size = new System.Drawing.Size(321, 27);
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
            this.colliderTypePanel.Size = new System.Drawing.Size(321, 27);
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
            this.colliderTypeCombobox.Location = new System.Drawing.Point(104, 3);
            this.colliderTypeCombobox.Name = "colliderTypeCombobox";
            this.colliderTypeCombobox.Size = new System.Drawing.Size(183, 21);
            this.colliderTypeCombobox.TabIndex = 0;
            this.colliderTypeCombobox.SelectedIndexChanged += new System.EventHandler(this.colliderTypeCombobox_SelectedIndexChanged);
            // 
            // changeNameButton
            // 
            this.changeNameButton.Location = new System.Drawing.Point(145, 3);
            this.changeNameButton.Name = "changeNameButton";
            this.changeNameButton.Size = new System.Drawing.Size(75, 23);
            this.changeNameButton.TabIndex = 0;
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(3, 3);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(75, 23);
            this.removeButton.TabIndex = 1;
            // 
            // propertiesLayoutPanel
            // 
            this.propertiesLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertiesLayoutPanel.AutoSize = true;
            this.propertiesLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertiesLayoutPanel.ColumnCount = 1;
            this.propertiesLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesLayoutPanel.Controls.Add(this.physicalPropertiesGroupBox, 0, 2);
            this.propertiesLayoutPanel.Controls.Add(this.meshPropertiesGroupBox, 0, 1);
            this.propertiesLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.propertiesLayoutPanel.Name = "propertiesLayoutPanel";
            this.propertiesLayoutPanel.RowCount = 3;
            this.propertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertiesLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.propertiesLayoutPanel.Size = new System.Drawing.Size(333, 240);
            this.propertiesLayoutPanel.TabIndex = 18;
            // 
            // propertiesScrollablePanel
            // 
            this.propertiesScrollablePanel.AutoScroll = true;
            this.propertiesScrollablePanel.Controls.Add(this.propertiesLayoutPanel);
            this.propertiesScrollablePanel.Location = new System.Drawing.Point(0, 0);
            this.propertiesScrollablePanel.Name = "propertiesScrollablePanel";
            this.propertiesScrollablePanel.Size = new System.Drawing.Size(296, 402);
            this.propertiesScrollablePanel.TabIndex = 19;
            // 
            // ComponentPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(309, 356);
            this.Controls.Add(this.propertiesScrollablePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ComponentPropertiesForm";
            this.physicalPropertiesGroupBox.ResumeLayout(false);
            this.physicalPropertiesGroupBox.PerformLayout();
            this.frictionLabelsLayoutPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).EndInit();
            this.dynamicGroupBox.ResumeLayout(false);
            this.dynamicGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).EndInit();
            this.meshPropertiesGroupBox.ResumeLayout(false);
            this.meshPropertiesGroupBox.PerformLayout();
            this.meshPropertiesTable.ResumeLayout(false);
            this.meshPropertiesTable.PerformLayout();
            this.colliderTypePanel.ResumeLayout(false);
            this.colliderTypePanel.PerformLayout();
            this.propertiesLayoutPanel.ResumeLayout(false);
            this.propertiesLayoutPanel.PerformLayout();
            this.propertiesScrollablePanel.ResumeLayout(false);
            this.propertiesScrollablePanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

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
        private System.Windows.Forms.Button changeNameButton;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.TableLayoutPanel propertiesLayoutPanel;
        private System.Windows.Forms.TableLayoutPanel controls;
        private System.Windows.Forms.TableLayoutPanel meshPropertiesTable;
        private System.Windows.Forms.Panel colliderTypePanel;
        private System.Windows.Forms.TableLayoutPanel frictionLabelsLayoutPanel;
        //private System.Windows.Forms.TableLayoutPanel propertyOptionsLayoutPanel;
        private System.Windows.Forms.Panel propertiesScrollablePanel;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnSave;
    }
}
