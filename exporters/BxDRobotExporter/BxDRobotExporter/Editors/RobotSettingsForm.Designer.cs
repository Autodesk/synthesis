namespace BxDRobotExporter.Editors
{
    partial class RobotSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RobotSettingsForm));
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.RobotNameLabel = new System.Windows.Forms.Label();
            this.RobotNameTextBox = new System.Windows.Forms.TextBox();
            this.ColorBox = new System.Windows.Forms.CheckBox();
            this.WindowControlLayout = new System.Windows.Forms.TableLayoutPanel();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.OpenSynthesisBox = new System.Windows.Forms.CheckBox();
            this.MainLayout.SuspendLayout();
            this.WindowControlLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.Controls.Add(this.RobotNameLabel, 0, 0);
            this.MainLayout.Controls.Add(this.RobotNameTextBox, 1, 0);
            this.MainLayout.Controls.Add(this.ColorBox, 0, 1);
            this.MainLayout.Controls.Add(this.WindowControlLayout, 0, 2);
            this.MainLayout.Controls.Add(this.OpenSynthesisBox, 1, 1);
            this.MainLayout.Location = new System.Drawing.Point(4, 4);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(4);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 3;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.Size = new System.Drawing.Size(576, 91);
            this.MainLayout.TabIndex = 7;
            // 
            // RobotNameLabel
            // 
            this.RobotNameLabel.AutoSize = true;
            this.RobotNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.RobotNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RobotNameLabel.Location = new System.Drawing.Point(4, 4);
            this.RobotNameLabel.Margin = new System.Windows.Forms.Padding(4);
            this.RobotNameLabel.Name = "RobotNameLabel";
            this.RobotNameLabel.Size = new System.Drawing.Size(91, 22);
            this.RobotNameLabel.TabIndex = 2;
            this.RobotNameLabel.Text = "Robot Name:";
            this.RobotNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RobotNameTextBox
            // 
            this.RobotNameTextBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.RobotNameTextBox.Location = new System.Drawing.Point(158, 4);
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
            this.ColorBox.Location = new System.Drawing.Point(4, 34);
            this.ColorBox.Margin = new System.Windows.Forms.Padding(4);
            this.ColorBox.Name = "ColorBox";
            this.ColorBox.Size = new System.Drawing.Size(146, 21);
            this.ColorBox.TabIndex = 7;
            this.ColorBox.Text = "Export With Colors";
            this.ColorBox.UseVisualStyleBackColor = true;
            // 
            // WindowControlLayout
            // 
            this.WindowControlLayout.AutoSize = true;
            this.WindowControlLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WindowControlLayout.ColumnCount = 3;
            this.MainLayout.SetColumnSpan(this.WindowControlLayout, 2);
            this.WindowControlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WindowControlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 4F));
            this.WindowControlLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.WindowControlLayout.Controls.Add(this.ButtonCancel, 0, 0);
            this.WindowControlLayout.Controls.Add(this.ButtonOk, 2, 0);
            this.WindowControlLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.WindowControlLayout.Location = new System.Drawing.Point(3, 61);
            this.WindowControlLayout.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.WindowControlLayout.Name = "WindowControlLayout";
            this.WindowControlLayout.RowCount = 1;
            this.WindowControlLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.WindowControlLayout.Size = new System.Drawing.Size(570, 28);
            this.WindowControlLayout.TabIndex = 3;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonCancel.Location = new System.Drawing.Point(0, 0);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(283, 28);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Dock = System.Windows.Forms.DockStyle.Top;
            this.ButtonOk.Enabled = false;
            this.ButtonOk.Location = new System.Drawing.Point(287, 0);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(0);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(283, 28);
            this.ButtonOk.TabIndex = 5;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // OpenSynthesisBox
            // 
            this.OpenSynthesisBox.AutoSize = true;
            this.OpenSynthesisBox.Location = new System.Drawing.Point(164, 33);
            this.OpenSynthesisBox.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.OpenSynthesisBox.Name = "OpenSynthesisBox";
            this.OpenSynthesisBox.Size = new System.Drawing.Size(130, 21);
            this.OpenSynthesisBox.TabIndex = 8;
            this.OpenSynthesisBox.Text = "Open Synthesis";
            this.OpenSynthesisBox.UseVisualStyleBackColor = true;
            this.OpenSynthesisBox.CheckedChanged += new System.EventHandler(this.OpenSynthesisBox_CheckedChanged);
            // 
            // RobotSettingsForm
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.White;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(576, 100);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RobotSettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export Robot to Synthesis";
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.WindowControlLayout.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.CheckBox ColorBox;
        private System.Windows.Forms.Button ButtonCancel;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.TableLayoutPanel WindowControlLayout;
        private System.Windows.Forms.Label RobotNameLabel;
        private System.Windows.Forms.TextBox RobotNameTextBox;
        private System.Windows.Forms.CheckBox OpenSynthesisBox;
    }
}