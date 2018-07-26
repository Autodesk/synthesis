﻿namespace JointResolver.ControlGUI
{
    partial class ExportRobotForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportRobotForm));
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.RobotNameLabel = new System.Windows.Forms.Label();
            this.RobotNameTextBox = new System.Windows.Forms.TextBox();
            this.FieldSelectComboBox = new System.Windows.Forms.ComboBox();
            this.ColorBox = new System.Windows.Forms.CheckBox();
            this.FieldLabel = new System.Windows.Forms.Label();
            this.OpenSynthesisBox = new System.Windows.Forms.CheckBox();
            this.WindowControlLayout = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.MainLayout.SuspendLayout();
            this.WindowControlLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 3;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.Controls.Add(this.RobotNameLabel, 0, 0);
            this.MainLayout.Controls.Add(this.RobotNameTextBox, 1, 0);
            this.MainLayout.Controls.Add(this.FieldSelectComboBox, 2, 2);
            this.MainLayout.Controls.Add(this.ColorBox, 0, 1);
            this.MainLayout.Controls.Add(this.FieldLabel, 1, 2);
            this.MainLayout.Controls.Add(this.OpenSynthesisBox, 0, 2);
            this.MainLayout.Controls.Add(this.WindowControlLayout, 0, 3);
            this.MainLayout.Location = new System.Drawing.Point(4, 4);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 4;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.Size = new System.Drawing.Size(570, 123);
            this.MainLayout.TabIndex = 7;
            // 
            // RobotNameLabel
            // 
            this.RobotNameLabel.AutoSize = true;
            this.RobotNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.RobotNameLabel.Location = new System.Drawing.Point(4, 4);
            this.RobotNameLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotNameLabel.Name = "RobotNameLabel";
            this.RobotNameLabel.Size = new System.Drawing.Size(87, 22);
            this.RobotNameLabel.TabIndex = 2;
            this.RobotNameLabel.Text = "Robot Name";
            this.RobotNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RobotNameTextBox
            // 
            this.MainLayout.SetColumnSpan(this.RobotNameTextBox, 2);
            this.RobotNameTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotNameTextBox.Location = new System.Drawing.Point(152, 4);
            this.RobotNameTextBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.RobotNameTextBox.Name = "RobotNameTextBox";
            this.RobotNameTextBox.Size = new System.Drawing.Size(414, 22);
            this.RobotNameTextBox.TabIndex = 1;
            this.RobotNameTextBox.WordWrap = false;
            this.RobotNameTextBox.TextChanged += new System.EventHandler(this.RobotNameTextBox_TextChanged);
            // 
            // FieldSelectComboBox
            // 
            this.FieldSelectComboBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.FieldSelectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FieldSelectComboBox.Enabled = false;
            this.FieldSelectComboBox.FormattingEnabled = true;
            this.FieldSelectComboBox.Location = new System.Drawing.Point(198, 63);
            this.FieldSelectComboBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FieldSelectComboBox.Name = "FieldSelectComboBox";
            this.FieldSelectComboBox.Size = new System.Drawing.Size(368, 24);
            this.FieldSelectComboBox.TabIndex = 8;
            this.FieldSelectComboBox.SelectedIndexChanged += new System.EventHandler(this.FieldSelectComboBox_SelectedIndexChanged);
            // 
            // ColorBox
            // 
            this.ColorBox.AutoSize = true;
            this.ColorBox.Location = new System.Drawing.Point(4, 34);
            this.ColorBox.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new System.Drawing.Size(140, 21);
            this.ColorBox.TabIndex = 7;
            this.ColorBox.Text = "Export with colors";
            this.ColorBox.UseVisualStyleBackColor = true;
            // 
            // FieldLabel
            // 
            this.FieldLabel.AutoSize = true;
            this.FieldLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.FieldLabel.Enabled = false;
            this.FieldLabel.Location = new System.Drawing.Point(152, 63);
            this.FieldLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.FieldLabel.Name = "FieldLabel";
            this.FieldLabel.Size = new System.Drawing.Size(38, 24);
            this.FieldLabel.TabIndex = 6;
            this.FieldLabel.Text = "Field";
            this.FieldLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // OpenSynthesisBox
            // 
            this.OpenSynthesisBox.AutoSize = true;
            this.OpenSynthesisBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.OpenSynthesisBox.Location = new System.Drawing.Point(4, 66);
            this.OpenSynthesisBox.Margin = new System.Windows.Forms.Padding(4, 7, 4, 4);
            this.OpenSynthesisBox.Name = "OpenSynthesisBox";
            this.OpenSynthesisBox.Size = new System.Drawing.Size(130, 21);
            this.OpenSynthesisBox.TabIndex = 4;
            this.OpenSynthesisBox.Text = "Open Synthesis";
            this.OpenSynthesisBox.UseVisualStyleBackColor = true;
            this.OpenSynthesisBox.CheckedChanged += new System.EventHandler(this.OpenSynthesisBox_CheckedChanged);
            // 
            // WindowControlLayout
            // 
            this.WindowControlLayout.AutoSize = true;
            this.WindowControlLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WindowControlLayout.ColumnCount = 3;
            this.MainLayout.SetColumnSpan(this.WindowControlLayout, 3);
            this.WindowControlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WindowControlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 4F));
            this.WindowControlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WindowControlLayout.Controls.Add(this.ButtonCancel, 0, 0);
            this.WindowControlLayout.Controls.Add(this.ButtonOk, 2, 0);
            this.WindowControlLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.WindowControlLayout.Location = new System.Drawing.Point(3, 93);
            this.WindowControlLayout.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.WindowControlLayout.Name = "WindowControlLayout";
            this.WindowControlLayout.RowCount = 1;
            this.WindowControlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WindowControlLayout.Size = new System.Drawing.Size(564, 28);
            this.WindowControlLayout.TabIndex = 3;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonCancel.Location = new System.Drawing.Point(0, 0);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(280, 28);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonOk.Enabled = false;
            this.ButtonOk.Location = new System.Drawing.Point(284, 0);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(280, 28);
            this.ButtonOk.TabIndex = 5;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ExportRobotForm
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(576, 146);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportRobotForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export Robot";
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.WindowControlLayout.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.CheckBox OpenSynthesisBox;
        private System.Windows.Forms.Label FieldLabel;
        private System.Windows.Forms.CheckBox ColorBox;
        private System.Windows.Forms.ComboBox FieldSelectComboBox;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.TableLayoutPanel WindowControlLayout;
        private System.Windows.Forms.Label RobotNameLabel;
        private System.Windows.Forms.TextBox RobotNameTextBox;
    }
}