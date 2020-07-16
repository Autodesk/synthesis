using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SynthesisInventorGltfExporter.GUI.Editors
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportForm));
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.RobotNameLabel = new System.Windows.Forms.Label();
            this.RobotNameTextBox = new System.Windows.Forms.TextBox();
            this.ColorBox = new System.Windows.Forms.CheckBox();
            this.OpenSynthesisBox = new System.Windows.Forms.CheckBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.Controls.Add(this.RobotNameLabel, 0, 0);
            this.MainLayout.Controls.Add(this.RobotNameTextBox, 1, 0);
            this.MainLayout.Controls.Add(this.ColorBox, 0, 1);
            this.MainLayout.Controls.Add(this.OpenSynthesisBox, 1, 1);
            this.MainLayout.Location = new System.Drawing.Point(4, 4);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(4);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 2;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.Size = new System.Drawing.Size(570, 59);
            this.MainLayout.TabIndex = 7;
            // 
            // RobotNameLabel
            // 
            this.RobotNameLabel.AutoSize = true;
            this.RobotNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.RobotNameLabel.Location = new System.Drawing.Point(4, 4);
            this.RobotNameLabel.Margin = new System.Windows.Forms.Padding(4);
            this.RobotNameLabel.Name = "RobotNameLabel";
            this.RobotNameLabel.Size = new System.Drawing.Size(87, 22);
            this.RobotNameLabel.TabIndex = 2;
            this.RobotNameLabel.Text = "Robot Name";
            this.RobotNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RobotNameTextBox
            // 
            this.RobotNameTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotNameTextBox.Location = new System.Drawing.Point(152, 4);
            this.RobotNameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.RobotNameTextBox.Name = "RobotNameTextBox";
            this.RobotNameTextBox.Size = new System.Drawing.Size(414, 22);
            this.RobotNameTextBox.TabIndex = 1;
            this.RobotNameTextBox.WordWrap = false;
            this.RobotNameTextBox.TextChanged += new System.EventHandler(this.RobotNameTextBox_TextChanged);
            // 
            // ColorBox
            // 
            this.ColorBox.AutoSize = true;
            this.ColorBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ColorBox.Location = new System.Drawing.Point(4, 34);
            this.ColorBox.Margin = new System.Windows.Forms.Padding(4);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new System.Drawing.Size(140, 21);
            this.ColorBox.TabIndex = 7;
            this.ColorBox.Text = "Export with colors";
            this.ColorBox.UseVisualStyleBackColor = true;
            // 
            // OpenSynthesisBox
            // 
            this.OpenSynthesisBox.AutoSize = true;
            this.OpenSynthesisBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OpenSynthesisBox.Location = new System.Drawing.Point(152, 34);
            this.OpenSynthesisBox.Margin = new System.Windows.Forms.Padding(4);
            this.OpenSynthesisBox.Name = "OpenSynthesisBox";
            this.OpenSynthesisBox.Size = new System.Drawing.Size(414, 21);
            this.OpenSynthesisBox.TabIndex = 4;
            this.OpenSynthesisBox.Text = "Open Synthesis";
            this.OpenSynthesisBox.UseVisualStyleBackColor = true;
            this.OpenSynthesisBox.CheckedChanged += new System.EventHandler(this.OpenSynthesisBox_CheckedChanged);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(447, 41);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(121, 27);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(320, 41);
            this.okButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(121, 27);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = false;
            this.okButton.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ExportForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(580, 80);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export Robot to Synthesis";
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private TableLayoutPanel MainLayout;
        private CheckBox OpenSynthesisBox;
        private CheckBox ColorBox;
        private Label RobotNameLabel;
        private TextBox RobotNameTextBox;
        private Button cancelButton;
        private Button okButton;
    }
}