namespace EditorsLibrary
{
    partial class JointSensorEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JointSensorEditorForm));
            this.label1 = new System.Windows.Forms.Label();
            this.typeBox = new System.Windows.Forms.ComboBox();
            this.PortALbl = new System.Windows.Forms.Label();
            this.SaveButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.PortANumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.button1 = new System.Windows.Forms.Button();
            this.PortBLbl = new System.Windows.Forms.Label();
            this.PortBNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.ConversionLbl = new System.Windows.Forms.Label();
            this.ConversionNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortANumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortBNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ConversionNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 42);
            this.label1.TabIndex = 0;
            this.label1.Text = "Sensor Type:";
            // 
            // typeBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.typeBox, 2);
            this.typeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeBox.FormattingEnabled = true;
            this.typeBox.Items.AddRange(new object[] {
            "Encoder"});
            this.typeBox.Location = new System.Drawing.Point(103, 2);
            this.typeBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(194, 24);
            this.typeBox.TabIndex = 0;
            this.typeBox.SelectedIndexChanged += new System.EventHandler(this.typeBox_SelectedIndexChanged);
            // 
            // port1Lbl
            // 
            this.PortALbl.AutoSize = true;
            this.PortALbl.Location = new System.Drawing.Point(3, 42);
            this.PortALbl.Name = "portALbl";
            this.PortALbl.Size = new System.Drawing.Size(50, 17);
            this.PortALbl.TabIndex = 2;
            this.PortALbl.Text = "Port A:";
            // 
            // SaveButton
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.SaveButton, 2);
            this.SaveButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.SaveButton.Location = new System.Drawing.Point(203, 128);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(194, 34);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.Text = "Save Sensor";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // CancelButton
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.CancelButton, 2);
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.CancelButton.Location = new System.Drawing.Point(3, 128);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(194, 34);
            this.CancelButton.TabIndex = 12;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.PortALbl, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.CancelButton, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.SaveButton, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.typeBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.button1, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.PortBLbl, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.PortBNumericUpDown, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.ConversionLbl, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.ConversionNumericUpDown, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.PortANumericUpDown, 1, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(400, 165);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // Port1NumericUpDown
            // 
            this.PortANumericUpDown.Location = new System.Drawing.Point(103, 45);
            this.PortANumericUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.PortANumericUpDown.Name = "PortANumericUpDown";
            this.PortANumericUpDown.Size = new System.Drawing.Size(94, 22);
            this.PortANumericUpDown.TabIndex = 14;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(303, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(0, 0);
            this.button1.TabIndex = 13;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // PortBLbl
            // 
            this.PortBLbl.AutoSize = true;
            this.PortBLbl.Location = new System.Drawing.Point(203, 42);
            this.PortBLbl.Name = "PortBLbl";
            this.PortBLbl.Size = new System.Drawing.Size(50, 17);
            this.PortBLbl.TabIndex = 15;
            this.PortBLbl.Text = "Port B:";
            // 
            // Port2NumericUpDown
            // 
            this.PortBNumericUpDown.Location = new System.Drawing.Point(303, 45);
            this.PortBNumericUpDown.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.PortBNumericUpDown.Name = "PortBNumericUpDown";
            this.PortBNumericUpDown.Size = new System.Drawing.Size(94, 22);
            this.PortBNumericUpDown.TabIndex = 16;
            // 
            // ConversionLbl
            // 
            this.ConversionLbl.AutoSize = true;
            this.ConversionLbl.Location = new System.Drawing.Point(3, 84);
            this.ConversionLbl.Name = "ConversionLbl";
            this.ConversionLbl.Size = new System.Drawing.Size(46, 17);
            this.ConversionLbl.TabIndex = 17;
            this.ConversionLbl.Text = "label2";
            // 
            // ConversionNumericUpDown
            // 
            this.ConversionNumericUpDown.DecimalPlaces = 3;
            this.ConversionNumericUpDown.Location = new System.Drawing.Point(103, 87);
            this.ConversionNumericUpDown.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.ConversionNumericUpDown.Minimum = new decimal(new int[] {
            100000,
            0,
            0,
            -2147483648});
            this.ConversionNumericUpDown.Name = "ConversionNumericUpDown";
            this.ConversionNumericUpDown.Size = new System.Drawing.Size(94, 22);
            this.ConversionNumericUpDown.TabIndex = 18;
            // 
            // EditSensorForm
            // 
            this.AcceptButton = this.SaveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 165);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JointSensorEditorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sensor";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PortANumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PortBNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ConversionNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox typeBox;
        private System.Windows.Forms.Label PortALbl;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown PortANumericUpDown;
        private System.Windows.Forms.Label PortBLbl;
        private System.Windows.Forms.NumericUpDown PortBNumericUpDown;
        private System.Windows.Forms.Label ConversionLbl;
        private System.Windows.Forms.NumericUpDown ConversionNumericUpDown;
    }
}