namespace JointResolver.ControlGUI
{
    partial class SaveRobotForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaveRobotForm));
            this.RobotNameLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.FieldLabel = new System.Windows.Forms.Label();
            this.OpenSynthesisBox = new System.Windows.Forms.CheckBox();
            this.ColorBox = new System.Windows.Forms.CheckBox();
            this.RobotNameTextBox = new System.Windows.Forms.TextBox();
            this.FieldSelectComboBox = new System.Windows.Forms.ComboBox();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.windowControlPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            this.windowControlPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // RobotNameLabel
            // 
            this.RobotNameLabel.AutoSize = true;
            this.RobotNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.RobotNameLabel.Location = new System.Drawing.Point(18, 15);
            this.RobotNameLabel.Name = "RobotNameLabel";
            this.RobotNameLabel.Size = new System.Drawing.Size(67, 26);
            this.RobotNameLabel.TabIndex = 2;
            this.RobotNameLabel.Text = "Robot Name";
            this.RobotNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Controls.Add(this.windowControlPanel, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.RobotNameLabel, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.FieldLabel, 3, 5);
            this.tableLayoutPanel1.Controls.Add(this.OpenSynthesisBox, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.ColorBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.RobotNameTextBox, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.FieldSelectComboBox, 4, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 9;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(397, 180);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // FieldLabel
            // 
            this.FieldLabel.AutoSize = true;
            this.FieldLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.FieldLabel.Enabled = false;
            this.FieldLabel.Location = new System.Drawing.Point(145, 92);
            this.FieldLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.FieldLabel.Name = "FieldLabel";
            this.FieldLabel.Size = new System.Drawing.Size(29, 25);
            this.FieldLabel.TabIndex = 6;
            this.FieldLabel.Text = "Field";
            this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OpenSynthesisBox
            // 
            this.OpenSynthesisBox.AutoSize = true;
            this.OpenSynthesisBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.OpenSynthesisBox.Location = new System.Drawing.Point(17, 94);
            this.OpenSynthesisBox.Margin = new System.Windows.Forms.Padding(2);
            this.OpenSynthesisBox.Name = "OpenSynthesisBox";
            this.OpenSynthesisBox.Size = new System.Drawing.Size(100, 21);
            this.OpenSynthesisBox.TabIndex = 4;
            this.OpenSynthesisBox.Text = "Open Synthesis";
            this.OpenSynthesisBox.UseVisualStyleBackColor = true;
            this.OpenSynthesisBox.CheckedChanged += new System.EventHandler(this.OpenSynthesisBox_CheckedChanged);
            // 
            // ColorBox
            // 
            this.ColorBox.AutoSize = true;
            this.ColorBox.Location = new System.Drawing.Point(17, 58);
            this.ColorBox.Margin = new System.Windows.Forms.Padding(2);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new System.Drawing.Size(109, 17);
            this.ColorBox.TabIndex = 7;
            this.ColorBox.Text = "Export with colors";
            this.ColorBox.UseVisualStyleBackColor = true;
            // 
            // RobotNameTextBox
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.RobotNameTextBox, 2);
            this.RobotNameTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotNameTextBox.Location = new System.Drawing.Point(146, 18);
            this.RobotNameTextBox.Name = "RobotNameTextBox";
            this.RobotNameTextBox.Size = new System.Drawing.Size(233, 20);
            this.RobotNameTextBox.TabIndex = 1;
            this.RobotNameTextBox.WordWrap = false;
            // 
            // FieldSelectComboBox
            // 
            this.FieldSelectComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FieldSelectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FieldSelectComboBox.Enabled = false;
            this.FieldSelectComboBox.FormattingEnabled = true;
            this.FieldSelectComboBox.Location = new System.Drawing.Point(178, 94);
            this.FieldSelectComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.FieldSelectComboBox.Name = "FieldSelectComboBox";
            this.FieldSelectComboBox.Size = new System.Drawing.Size(202, 21);
            this.FieldSelectComboBox.TabIndex = 8;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonCancel.Location = new System.Drawing.Point(3, 3);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(175, 23);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonOk.Location = new System.Drawing.Point(184, 3);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(176, 23);
            this.ButtonOk.TabIndex = 5;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // windowControlPanel
            // 
            this.windowControlPanel.AutoSize = true;
            this.windowControlPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.windowControlPanel.ColumnCount = 2;
            this.tableLayoutPanel1.SetColumnSpan(this.windowControlPanel, 4);
            this.windowControlPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.windowControlPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.windowControlPanel.Controls.Add(this.ButtonOk, 1, 0);
            this.windowControlPanel.Controls.Add(this.ButtonCancel, 0, 0);
            this.windowControlPanel.Location = new System.Drawing.Point(17, 134);
            this.windowControlPanel.Margin = new System.Windows.Forms.Padding(2);
            this.windowControlPanel.Name = "windowControlPanel";
            this.windowControlPanel.RowCount = 1;
            this.windowControlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.windowControlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.windowControlPanel.Size = new System.Drawing.Size(363, 29);
            this.windowControlPanel.TabIndex = 3;
            // 
            // SaveRobotForm
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(397, 180);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveRobotForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export Robot";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.windowControlPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label RobotNameLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox OpenSynthesisBox;
        private System.Windows.Forms.Label FieldLabel;
        private System.Windows.Forms.CheckBox ColorBox;
        private System.Windows.Forms.TextBox RobotNameTextBox;
        private System.Windows.Forms.ComboBox FieldSelectComboBox;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.TableLayoutPanel windowControlPanel;
    }
}