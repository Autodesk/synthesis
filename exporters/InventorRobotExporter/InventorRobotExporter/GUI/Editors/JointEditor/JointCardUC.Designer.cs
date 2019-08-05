using InventorRobotExporter.Managers;
using InventorRobotExporter.Properties;

namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    sealed partial class JointCardUC
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JointCardUC));
            this.DriverLayout = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.weightInput = new System.Windows.Forms.NumericUpDown();
            this.driverTypeComboBox = new System.Windows.Forms.ComboBox();
            this.weightLabel = new System.Windows.Forms.Label();
            this.jointDriverLabel = new System.Windows.Forms.Label();
            this.advancedButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.jointName = new System.Windows.Forms.Label();
            this.jointTypeComboBox = new System.Windows.Forms.ComboBox();
            this.wheelTypeLabel = new System.Windows.Forms.Label();
            this.sideLabel = new System.Windows.Forms.Label();
            this.jointTypeLabel = new System.Windows.Forms.Label();
            this.dtSideComboBox = new System.Windows.Forms.ComboBox();
            this.wheelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.DriverLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightInput)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // DriverLayout
            // 
            this.DriverLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DriverLayout.BackColor = System.Drawing.SystemColors.Control;
            this.DriverLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.DriverLayout.ColumnCount = 2;
            this.DriverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DriverLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DriverLayout.Controls.Add(this.pictureBox1, 0, 0);
            this.DriverLayout.Controls.Add(this.tableLayoutPanel1, 1, 0);
            this.DriverLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.DriverLayout.Location = new System.Drawing.Point(0, 0);
            this.DriverLayout.Margin = new System.Windows.Forms.Padding(0);
            this.DriverLayout.Name = "DriverLayout";
            this.DriverLayout.RowCount = 1;
            this.DriverLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.DriverLayout.Size = new System.Drawing.Size(300, 136);
            this.DriverLayout.TabIndex = 2;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = null;
            this.pictureBox1.Location = new System.Drawing.Point(4, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(125, 128);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // weightInput
            // 
            this.weightInput.DecimalPlaces = 1;
            this.weightInput.Dock = System.Windows.Forms.DockStyle.Top;
            this.weightInput.Location = new System.Drawing.Point(116, 97);
            this.weightInput.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.weightInput.Name = "weightInput";
            this.weightInput.Size = new System.Drawing.Size(280, 22);
            this.weightInput.TabIndex = 9;
            this.weightInput.Value = new decimal(new int[] {
            225,
            0,
            0,
            65536});
            // 
            // driverTypeComboBox
            // 
            this.driverTypeComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.driverTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.driverTypeComboBox.FormattingEnabled = true;
            this.driverTypeComboBox.Location = new System.Drawing.Point(116, 128);
            this.driverTypeComboBox.Name = "driverTypeComboBox";
            this.driverTypeComboBox.Size = new System.Drawing.Size(280, 25);
            this.driverTypeComboBox.TabIndex = 8;
            // 
            // weightLabel
            // 
            this.weightLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.weightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weightLabel.Location = new System.Drawing.Point(4, 94);
            this.weightLabel.Name = "weightLabel";
            this.weightLabel.Padding = new System.Windows.Forms.Padding(0, 1, 3, 1);
            this.weightLabel.Size = new System.Drawing.Size(105, 30);
            this.weightLabel.TabIndex = 7;
            this.weightLabel.Text = "Weight (lbs):";
            this.weightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // jointDriverLabel
            // 
            this.jointDriverLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jointDriverLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.jointDriverLabel.Location = new System.Drawing.Point(4, 125);
            this.jointDriverLabel.Name = "jointDriverLabel";
            this.jointDriverLabel.Padding = new System.Windows.Forms.Padding(0, 1, 3, 1);
            this.jointDriverLabel.Size = new System.Drawing.Size(105, 30);
            this.jointDriverLabel.TabIndex = 6;
            this.jointDriverLabel.Text = "Joint Driver:";
            this.jointDriverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // advancedButton
            // 
            this.advancedButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.advancedButton.Location = new System.Drawing.Point(43, 5);
            this.advancedButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(116, 27);
            this.advancedButton.TabIndex = 7;
            this.advancedButton.Text = "Advanced...";
            this.advancedButton.UseVisualStyleBackColor = true;
            this.advancedButton.Click += new System.EventHandler(this.AdvancedButton_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 126F));
            this.tableLayoutPanel1.Controls.Add(this.jointName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.advancedButton, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(133, 1);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(166, 131);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // jointName
            // 
            this.jointName.AutoEllipsis = true;
            this.jointName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jointName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.jointName.Location = new System.Drawing.Point(3, 3);
            this.jointName.Margin = new System.Windows.Forms.Padding(3);
            this.jointName.Name = "jointName";
            this.jointName.Size = new System.Drawing.Size(34, 28);
            this.jointName.TabIndex = 0;
            this.jointName.Text = "am-3047_4inDuraOmniV2:1";
            this.jointName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // jointTypeComboBox
            // 
            this.jointTypeComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.jointTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointTypeComboBox.FormattingEnabled = true;
            this.jointTypeComboBox.Location = new System.Drawing.Point(115, 4);
            this.jointTypeComboBox.Name = "jointTypeComboBox";
            this.jointTypeComboBox.Size = new System.Drawing.Size(41, 24);
            this.jointTypeComboBox.TabIndex = 3;
            this.jointTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.JointTypeComboBox_SelectedIndexChanged);
            // 
            // wheelTypeLabel
            // 
            this.wheelTypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.wheelTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.wheelTypeLabel.Location = new System.Drawing.Point(4, 63);
            this.wheelTypeLabel.Name = "wheelTypeLabel";
            this.wheelTypeLabel.Padding = new System.Windows.Forms.Padding(0, 1, 3, 1);
            this.wheelTypeLabel.Size = new System.Drawing.Size(104, 30);
            this.wheelTypeLabel.TabIndex = 2;
            this.wheelTypeLabel.Text = "Wheel Type:";
            this.wheelTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // sideLabel
            // 
            this.sideLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sideLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sideLabel.Location = new System.Drawing.Point(4, 32);
            this.sideLabel.Name = "sideLabel";
            this.sideLabel.Padding = new System.Windows.Forms.Padding(0, 1, 3, 1);
            this.sideLabel.Size = new System.Drawing.Size(104, 30);
            this.sideLabel.TabIndex = 1;
            this.sideLabel.Text = "Side:";
            this.sideLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // jointTypeLabel
            // 
            this.jointTypeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.jointTypeLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.jointTypeLabel.Location = new System.Drawing.Point(4, 1);
            this.jointTypeLabel.Name = "jointTypeLabel";
            this.jointTypeLabel.Padding = new System.Windows.Forms.Padding(0, 1, 3, 1);
            this.jointTypeLabel.Size = new System.Drawing.Size(104, 30);
            this.jointTypeLabel.TabIndex = 0;
            this.jointTypeLabel.Text = "Joint Type:";
            this.jointTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // dtSideComboBox
            // 
            this.dtSideComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.dtSideComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dtSideComboBox.FormattingEnabled = true;
            this.dtSideComboBox.Items.AddRange(new object[] {
            "Right",
            "Left",
            "H-Drive Center"});
            this.dtSideComboBox.Location = new System.Drawing.Point(115, 35);
            this.dtSideComboBox.Name = "dtSideComboBox";
            this.dtSideComboBox.Size = new System.Drawing.Size(41, 24);
            this.dtSideComboBox.TabIndex = 4;
            // 
            // wheelTypeComboBox
            // 
            this.wheelTypeComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.wheelTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wheelTypeComboBox.FormattingEnabled = true;
            this.wheelTypeComboBox.Items.AddRange(new object[] {
            "Normal",
            "Omni",
            "Mecanum"});
            this.wheelTypeComboBox.Location = new System.Drawing.Point(115, 66);
            this.wheelTypeComboBox.Name = "wheelTypeComboBox";
            this.wheelTypeComboBox.Size = new System.Drawing.Size(41, 24);
            this.wheelTypeComboBox.TabIndex = 5;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.tableLayoutPanel2, 2);
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.wheelTypeComboBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.dtSideComboBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.jointTypeLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.sideLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.wheelTypeLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.jointTypeComboBox, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 37);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 3;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(160, 94);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // JointCardUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.DriverLayout);
            this.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.MinimumSize = new System.Drawing.Size(300, 0);
            this.Name = "JointCardUC";
            this.Size = new System.Drawing.Size(300, 136);
            this.DriverLayout.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightInput)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private readonly RobotDataManager robotDataManager;
        private System.Windows.Forms.TableLayoutPanel DriverLayout;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.NumericUpDown weightInput;
        private System.Windows.Forms.ComboBox driverTypeComboBox;
        private System.Windows.Forms.Label weightLabel;
        private System.Windows.Forms.Label jointDriverLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label jointName;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.ComboBox wheelTypeComboBox;
        private System.Windows.Forms.ComboBox dtSideComboBox;
        private System.Windows.Forms.Label jointTypeLabel;
        private System.Windows.Forms.Label sideLabel;
        private System.Windows.Forms.Label wheelTypeLabel;
        private System.Windows.Forms.ComboBox jointTypeComboBox;
        private System.Windows.Forms.Button advancedButton;
    }
}
