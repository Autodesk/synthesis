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
            this.panel1 = new System.Windows.Forms.Panel();
            this.dynamicCheckBox = new System.Windows.Forms.CheckBox();
            this.dynamicGroupBox = new System.Windows.Forms.GroupBox();
            this.massNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.massLabel = new System.Windows.Forms.Label();
            this.rubberLabel = new System.Windows.Forms.Label();
            this.carpetLabel = new System.Windows.Forms.Label();
            this.iceLabel = new System.Windows.Forms.Label();
            this.colliderTypeLabel = new System.Windows.Forms.Label();
            this.colliderTypeCombobox = new System.Windows.Forms.ComboBox();
            this.frictionTrackBar = new System.Windows.Forms.TrackBar();
            this.frictionLabel = new System.Windows.Forms.Label();
            this.physicsGroupOptionsBox = new System.Windows.Forms.GroupBox();
            this.changeNameButton = new System.Windows.Forms.Button();
            this.removeButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.inventorTreeView = new FieldExporter.Components.InventorTreeView(this.components);
            this.propertiesBox.SuspendLayout();
            this.panel1.SuspendLayout();
            this.dynamicGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).BeginInit();
            this.physicsGroupOptionsBox.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // inventorSelectButton
            // 
            this.inventorSelectButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventorSelectButton.Location = new System.Drawing.Point(3, 3);
            this.inventorSelectButton.Name = "inventorSelectButton";
            this.inventorSelectButton.Size = new System.Drawing.Size(144, 34);
            this.inventorSelectButton.TabIndex = 13;
            this.inventorSelectButton.Tag = "";
            this.inventorSelectButton.Text = "Select in Inventor";
            this.inventorSelectButton.UseVisualStyleBackColor = true;
            this.inventorSelectButton.Click += new System.EventHandler(this.inventorSelectButton_Click);
            // 
            // addSelectionButton
            // 
            this.addSelectionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.addSelectionButton.Enabled = false;
            this.addSelectionButton.Location = new System.Drawing.Point(153, 3);
            this.addSelectionButton.Name = "addSelectionButton";
            this.addSelectionButton.Size = new System.Drawing.Size(145, 34);
            this.addSelectionButton.TabIndex = 14;
            this.addSelectionButton.Text = "Add Selection";
            this.addSelectionButton.UseVisualStyleBackColor = true;
            this.addSelectionButton.Click += new System.EventHandler(this.addSelectionButton_Click);
            // 
            // propertiesBox
            // 
            this.propertiesBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.propertiesBox.Controls.Add(this.panel1);
            this.propertiesBox.Location = new System.Drawing.Point(304, 68);
            this.propertiesBox.Name = "propertiesBox";
            this.propertiesBox.Size = new System.Drawing.Size(293, 329);
            this.propertiesBox.TabIndex = 15;
            this.propertiesBox.TabStop = false;
            this.propertiesBox.Text = "Physical Properties";
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.dynamicCheckBox);
            this.panel1.Controls.Add(this.dynamicGroupBox);
            this.panel1.Controls.Add(this.rubberLabel);
            this.panel1.Controls.Add(this.carpetLabel);
            this.panel1.Controls.Add(this.iceLabel);
            this.panel1.Controls.Add(this.colliderTypeLabel);
            this.panel1.Controls.Add(this.colliderTypeCombobox);
            this.panel1.Controls.Add(this.frictionTrackBar);
            this.panel1.Controls.Add(this.frictionLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 18);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.panel1.Size = new System.Drawing.Size(287, 308);
            this.panel1.TabIndex = 8;
            // 
            // dynamicCheckBox
            // 
            this.dynamicCheckBox.AutoSize = true;
            this.dynamicCheckBox.Location = new System.Drawing.Point(12, 94);
            this.dynamicCheckBox.Name = "dynamicCheckBox";
            this.dynamicCheckBox.Size = new System.Drawing.Size(84, 21);
            this.dynamicCheckBox.TabIndex = 8;
            this.dynamicCheckBox.Text = "Dynamic";
            this.dynamicCheckBox.UseVisualStyleBackColor = true;
            this.dynamicCheckBox.CheckedChanged += new System.EventHandler(this.dynamicCheckBox_CheckedChanged);
            // 
            // dynamicGroupBox
            // 
            this.dynamicGroupBox.Controls.Add(this.massNumericUpDown);
            this.dynamicGroupBox.Controls.Add(this.massLabel);
            this.dynamicGroupBox.Enabled = false;
            this.dynamicGroupBox.Location = new System.Drawing.Point(6, 95);
            this.dynamicGroupBox.Name = "dynamicGroupBox";
            this.dynamicGroupBox.Size = new System.Drawing.Size(254, 49);
            this.dynamicGroupBox.TabIndex = 9;
            this.dynamicGroupBox.TabStop = false;
            // 
            // massNumericUpDown
            // 
            this.massNumericUpDown.Location = new System.Drawing.Point(89, 21);
            this.massNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.massNumericUpDown.Name = "massNumericUpDown";
            this.massNumericUpDown.Size = new System.Drawing.Size(159, 22);
            this.massNumericUpDown.TabIndex = 10;
            // 
            // massLabel
            // 
            this.massLabel.AutoSize = true;
            this.massLabel.Location = new System.Drawing.Point(6, 24);
            this.massLabel.Name = "massLabel";
            this.massLabel.Size = new System.Drawing.Size(77, 17);
            this.massLabel.TabIndex = 9;
            this.massLabel.Text = "Mass (lbs):";
            // 
            // rubberLabel
            // 
            this.rubberLabel.Location = new System.Drawing.Point(199, 72);
            this.rubberLabel.Name = "rubberLabel";
            this.rubberLabel.Size = new System.Drawing.Size(55, 17);
            this.rubberLabel.TabIndex = 6;
            this.rubberLabel.Text = "Rubber";
            // 
            // carpetLabel
            // 
            this.carpetLabel.AutoSize = true;
            this.carpetLabel.Location = new System.Drawing.Point(129, 72);
            this.carpetLabel.Name = "carpetLabel";
            this.carpetLabel.Size = new System.Drawing.Size(50, 17);
            this.carpetLabel.TabIndex = 7;
            this.carpetLabel.Text = "Carpet";
            // 
            // iceLabel
            // 
            this.iceLabel.AutoSize = true;
            this.iceLabel.Location = new System.Drawing.Point(81, 72);
            this.iceLabel.Name = "iceLabel";
            this.iceLabel.Size = new System.Drawing.Size(26, 17);
            this.iceLabel.TabIndex = 5;
            this.iceLabel.Text = "Ice";
            // 
            // colliderTypeLabel
            // 
            this.colliderTypeLabel.AutoSize = true;
            this.colliderTypeLabel.Location = new System.Drawing.Point(3, 6);
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
            this.colliderTypeCombobox.Location = new System.Drawing.Point(104, 3);
            this.colliderTypeCombobox.Name = "colliderTypeCombobox";
            this.colliderTypeCombobox.Size = new System.Drawing.Size(156, 24);
            this.colliderTypeCombobox.TabIndex = 0;
            // 
            // frictionTrackBar
            // 
            this.frictionTrackBar.LargeChange = 1;
            this.frictionTrackBar.Location = new System.Drawing.Point(67, 33);
            this.frictionTrackBar.Name = "frictionTrackBar";
            this.frictionTrackBar.Size = new System.Drawing.Size(193, 56);
            this.frictionTrackBar.TabIndex = 2;
            this.frictionTrackBar.Scroll += new System.EventHandler(this.frictionTrackBar_Scroll);
            // 
            // frictionLabel
            // 
            this.frictionLabel.AutoSize = true;
            this.frictionLabel.Location = new System.Drawing.Point(3, 33);
            this.frictionLabel.Name = "frictionLabel";
            this.frictionLabel.Size = new System.Drawing.Size(58, 34);
            this.frictionLabel.TabIndex = 3;
            this.frictionLabel.Text = "Friction:\r\n0/10";
            // 
            // physicsGroupOptionsBox
            // 
            this.physicsGroupOptionsBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.physicsGroupOptionsBox.Controls.Add(this.changeNameButton);
            this.physicsGroupOptionsBox.Controls.Add(this.removeButton);
            this.physicsGroupOptionsBox.Location = new System.Drawing.Point(304, 3);
            this.physicsGroupOptionsBox.Name = "physicsGroupOptionsBox";
            this.physicsGroupOptionsBox.Size = new System.Drawing.Size(293, 59);
            this.physicsGroupOptionsBox.TabIndex = 16;
            this.physicsGroupOptionsBox.TabStop = false;
            this.physicsGroupOptionsBox.Text = "PhysicsGroup Options";
            // 
            // changeNameButton
            // 
            this.changeNameButton.Location = new System.Drawing.Point(175, 21);
            this.changeNameButton.Name = "changeNameButton";
            this.changeNameButton.Size = new System.Drawing.Size(112, 32);
            this.changeNameButton.TabIndex = 3;
            this.changeNameButton.Text = "Change Name";
            this.changeNameButton.UseVisualStyleBackColor = true;
            this.changeNameButton.Click += new System.EventHandler(this.changeNameButton_Click);
            // 
            // removeButton
            // 
            this.removeButton.Location = new System.Drawing.Point(6, 21);
            this.removeButton.Name = "removeButton";
            this.removeButton.Size = new System.Drawing.Size(163, 32);
            this.removeButton.TabIndex = 2;
            this.removeButton.Text = "Remove PhysicsGroup";
            this.removeButton.UseVisualStyleBackColor = true;
            this.removeButton.Click += new System.EventHandler(this.removeButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.inventorSelectButton, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.addSelectionButton, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 360);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(301, 40);
            this.tableLayoutPanel1.TabIndex = 8;
            // 
            // inventorTreeView
            // 
            this.inventorTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inventorTreeView.Location = new System.Drawing.Point(3, 3);
            this.inventorTreeView.Name = "inventorTreeView";
            this.inventorTreeView.Size = new System.Drawing.Size(295, 354);
            this.inventorTreeView.TabIndex = 12;
            // 
            // ComponentPropertiesForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.physicsGroupOptionsBox);
            this.Controls.Add(this.propertiesBox);
            this.Controls.Add(this.inventorTreeView);
            this.Name = "ComponentPropertiesForm";
            this.Size = new System.Drawing.Size(600, 400);
            this.propertiesBox.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.dynamicGroupBox.ResumeLayout(false);
            this.dynamicGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.massNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frictionTrackBar)).EndInit();
            this.physicsGroupOptionsBox.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button inventorSelectButton;
        private System.Windows.Forms.Button addSelectionButton;
        private System.Windows.Forms.GroupBox propertiesBox;
        private System.Windows.Forms.GroupBox physicsGroupOptionsBox;
        private System.Windows.Forms.Button removeButton;
        private System.Windows.Forms.Button changeNameButton;
        private System.Windows.Forms.ComboBox colliderTypeCombobox;
        private System.Windows.Forms.Label colliderTypeLabel;
        private System.Windows.Forms.Label frictionLabel;
        private System.Windows.Forms.TrackBar frictionTrackBar;
        public InventorTreeView inventorTreeView;
        private System.Windows.Forms.Label rubberLabel;
        private System.Windows.Forms.Label iceLabel;
        private System.Windows.Forms.Label carpetLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox dynamicGroupBox;
        private System.Windows.Forms.CheckBox dynamicCheckBox;
        private System.Windows.Forms.Label massLabel;
        private System.Windows.Forms.NumericUpDown massNumericUpDown;
    }
}
