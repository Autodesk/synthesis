using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace InventorRobotExporter.GUI.Editors
{
    partial class ExportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            ComponentResourceManager resources = new ComponentResourceManager(typeof(ExportForm));
            this.MainLayout = new TableLayoutPanel();
            this.RobotNameLabel = new Label();
            this.RobotNameTextBox = new TextBox();
            this.FieldSelectComboBox = new ComboBox();
            this.ColorBox = new CheckBox();
            this.FieldLabel = new Label();
            this.OpenSynthesisBox = new CheckBox();
            this.WindowControlLayout = new TableLayoutPanel();
            this.ButtonCancel = new Button();
            this.ButtonOk = new Button();
            this.MainLayout.SuspendLayout();
            this.WindowControlLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 3;
            this.MainLayout.ColumnStyles.Add(new ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new ColumnStyle());
            this.MainLayout.Controls.Add(this.RobotNameLabel, 0, 0);
            this.MainLayout.Controls.Add(this.RobotNameTextBox, 1, 0);
            this.MainLayout.Controls.Add(this.FieldSelectComboBox, 2, 2);
            this.MainLayout.Controls.Add(this.ColorBox, 0, 1);
            this.MainLayout.Controls.Add(this.FieldLabel, 1, 2);
            this.MainLayout.Controls.Add(this.OpenSynthesisBox, 0, 2);
            this.MainLayout.Controls.Add(this.WindowControlLayout, 0, 3);
            this.MainLayout.Location = new Point(4, 4);
            this.MainLayout.Margin = new Padding(4, 4, 4, 4);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 4;
            this.MainLayout.RowStyles.Add(new RowStyle());
            this.MainLayout.RowStyles.Add(new RowStyle());
            this.MainLayout.RowStyles.Add(new RowStyle());
            this.MainLayout.RowStyles.Add(new RowStyle());
            this.MainLayout.Size = new Size(570, 123);
            this.MainLayout.TabIndex = 7;
            // 
            // RobotNameLabel
            // 
            this.RobotNameLabel.AutoSize = true;
            this.RobotNameLabel.Dock = DockStyle.Left;
            this.RobotNameLabel.Location = new Point(4, 4);
            this.RobotNameLabel.Margin = new Padding(4, 4, 4, 4);
            this.RobotNameLabel.Name = "RobotNameLabel";
            this.RobotNameLabel.Size = new Size(87, 22);
            this.RobotNameLabel.TabIndex = 2;
            this.RobotNameLabel.Text = "Robot Name";
            this.RobotNameLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // RobotNameTextBox
            // 
            this.MainLayout.SetColumnSpan(this.RobotNameTextBox, 2);
            this.RobotNameTextBox.Dock = DockStyle.Top;
            this.RobotNameTextBox.Location = new Point(152, 4);
            this.RobotNameTextBox.Margin = new Padding(4, 4, 4, 4);
            this.RobotNameTextBox.Name = "RobotNameTextBox";
            this.RobotNameTextBox.Size = new Size(414, 22);
            this.RobotNameTextBox.TabIndex = 1;
            this.RobotNameTextBox.WordWrap = false;
            this.RobotNameTextBox.TextChanged += new EventHandler(this.RobotNameTextBox_TextChanged);
            // 
            // FieldSelectComboBox
            // 
            this.FieldSelectComboBox.Dock = DockStyle.Top;
            this.FieldSelectComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.FieldSelectComboBox.Enabled = false;
            this.FieldSelectComboBox.FormattingEnabled = true;
            this.FieldSelectComboBox.Location = new Point(198, 63);
            this.FieldSelectComboBox.Margin = new Padding(4, 4, 4, 4);
            this.FieldSelectComboBox.Name = "FieldSelectComboBox";
            this.FieldSelectComboBox.Size = new Size(368, 24);
            this.FieldSelectComboBox.TabIndex = 8;
            this.FieldSelectComboBox.SelectedIndexChanged += new EventHandler(this.FieldSelectComboBox_SelectedIndexChanged);
            // 
            // ColorBox
            // 
            this.ColorBox.AutoSize = true;
            this.ColorBox.Location = new Point(4, 34);
            this.ColorBox.Margin = new Padding(4, 4, 4, 4);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new Size(140, 21);
            this.ColorBox.TabIndex = 7;
            this.ColorBox.Text = "Export with colors";
            this.ColorBox.UseVisualStyleBackColor = true;
            // 
            // FieldLabel
            // 
            this.FieldLabel.AutoSize = true;
            this.FieldLabel.Dock = DockStyle.Left;
            this.FieldLabel.Enabled = false;
            this.FieldLabel.Location = new Point(152, 63);
            this.FieldLabel.Margin = new Padding(4, 4, 4, 4);
            this.FieldLabel.Name = "FieldLabel";
            this.FieldLabel.Size = new Size(38, 24);
            this.FieldLabel.TabIndex = 6;
            this.FieldLabel.Text = "Field";
            this.FieldLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // OpenSynthesisBox
            // 
            this.OpenSynthesisBox.AutoSize = true;
            this.OpenSynthesisBox.Dock = DockStyle.Left;
            this.OpenSynthesisBox.Location = new Point(4, 66);
            this.OpenSynthesisBox.Margin = new Padding(4, 7, 4, 4);
            this.OpenSynthesisBox.Name = "OpenSynthesisBox";
            this.OpenSynthesisBox.Size = new Size(130, 21);
            this.OpenSynthesisBox.TabIndex = 4;
            this.OpenSynthesisBox.Text = "Open Synthesis";
            this.OpenSynthesisBox.UseVisualStyleBackColor = true;
            this.OpenSynthesisBox.CheckedChanged += new EventHandler(this.OpenSynthesisBox_CheckedChanged);
            // 
            // WindowControlLayout
            // 
            this.WindowControlLayout.AutoSize = true;
            this.WindowControlLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.WindowControlLayout.ColumnCount = 3;
            this.MainLayout.SetColumnSpan(this.WindowControlLayout, 3);
            this.WindowControlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.WindowControlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 4F));
            this.WindowControlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.WindowControlLayout.Controls.Add(this.ButtonCancel, 0, 0);
            this.WindowControlLayout.Controls.Add(this.ButtonOk, 2, 0);
            this.WindowControlLayout.Dock = DockStyle.Top;
            this.WindowControlLayout.Location = new Point(3, 93);
            this.WindowControlLayout.Margin = new Padding(3, 2, 3, 2);
            this.WindowControlLayout.Name = "WindowControlLayout";
            this.WindowControlLayout.RowCount = 1;
            this.WindowControlLayout.RowStyles.Add(new RowStyle());
            this.WindowControlLayout.Size = new Size(564, 28);
            this.WindowControlLayout.TabIndex = 3;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = DialogResult.Cancel;
            this.ButtonCancel.Dock = DockStyle.Top;
            this.ButtonCancel.Location = new Point(0, 0);
            this.ButtonCancel.Margin = new Padding(0);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new Size(280, 28);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Dock = DockStyle.Top;
            this.ButtonOk.Enabled = false;
            this.ButtonOk.Location = new Point(284, 0);
            this.ButtonOk.Margin = new Padding(0);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new Size(280, 28);
            this.ButtonOk.TabIndex = 5;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new EventHandler(this.ButtonOK_Click);
            // 
            // ExportRobotForm
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.BackColor = Color.White;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new Size(576, 146);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Icon = ((Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Export Robot to Synthesis";
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.WindowControlLayout.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TableLayoutPanel MainLayout;
        private CheckBox OpenSynthesisBox;
        private Label FieldLabel;
        private CheckBox ColorBox;
        private ComboBox FieldSelectComboBox;
        private Button ButtonCancel;
        private Button ButtonOk;
        private TableLayoutPanel WindowControlLayout;
        private Label RobotNameLabel;
        private TextBox RobotNameTextBox;
    }
}