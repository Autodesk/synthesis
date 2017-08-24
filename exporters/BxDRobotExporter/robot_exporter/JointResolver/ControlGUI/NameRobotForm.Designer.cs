namespace JointResolver.ControlGUI
{
    partial class NameRobotForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NameRobotForm));
            this.SaveLocationLabel = new System.Windows.Forms.Label();
            this.RobotNameTextBox = new System.Windows.Forms.TextBox();
            this.NameRobotLabel = new System.Windows.Forms.Label();
            this.PathTextBox = new System.Windows.Forms.TextBox();
            this.ButtonBrowse = new System.Windows.Forms.Button();
            this.ExportLocationDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // SaveLocationLabel
            // 
            this.SaveLocationLabel.AutoSize = true;
            this.SaveLocationLabel.Location = new System.Drawing.Point(12, 63);
            this.SaveLocationLabel.Name = "SaveLocationLabel";
            this.SaveLocationLabel.Size = new System.Drawing.Size(82, 13);
            this.SaveLocationLabel.TabIndex = 0;
            this.SaveLocationLabel.Text = "Save Location: ";
            // 
            // RobotNameTextBox
            // 
            this.RobotNameTextBox.Location = new System.Drawing.Point(15, 32);
            this.RobotNameTextBox.Name = "RobotNameTextBox";
            this.RobotNameTextBox.Size = new System.Drawing.Size(407, 20);
            this.RobotNameTextBox.TabIndex = 1;
            this.RobotNameTextBox.WordWrap = false;
            // 
            // NameRobotLabel
            // 
            this.NameRobotLabel.AutoSize = true;
            this.NameRobotLabel.Location = new System.Drawing.Point(15, 13);
            this.NameRobotLabel.Name = "NameRobotLabel";
            this.NameRobotLabel.Size = new System.Drawing.Size(211, 13);
            this.NameRobotLabel.TabIndex = 2;
            this.NameRobotLabel.Text = "Please enter a name for the exported robot:";
            // 
            // PathTextBox
            // 
            this.PathTextBox.Location = new System.Drawing.Point(91, 60);
            this.PathTextBox.Name = "PathTextBox";
            this.PathTextBox.ReadOnly = true;
            this.PathTextBox.Size = new System.Drawing.Size(331, 20);
            this.PathTextBox.TabIndex = 3;
            // 
            // ButtonBrowse
            // 
            this.ButtonBrowse.Location = new System.Drawing.Point(12, 87);
            this.ButtonBrowse.Name = "ButtonBrowse";
            this.ButtonBrowse.Size = new System.Drawing.Size(133, 23);
            this.ButtonBrowse.TabIndex = 4;
            this.ButtonBrowse.Text = "Browse...";
            this.ButtonBrowse.UseVisualStyleBackColor = true;
            this.ButtonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // ExportLocationDialog
            // 
            this.ExportLocationDialog.Description = "Select Export Location";
            // 
            // ButtonOk
            // 
            this.ButtonOk.Location = new System.Drawing.Point(290, 87);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(132, 23);
            this.ButtonOk.TabIndex = 5;
            this.ButtonOk.Text = "OK";
            this.ButtonOk.UseVisualStyleBackColor = true;
            this.ButtonOk.Click += new System.EventHandler(this.ButtonOK_Click);
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.ButtonCancel.Location = new System.Drawing.Point(151, 87);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(133, 23);
            this.ButtonCancel.TabIndex = 6;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            this.ButtonCancel.Click += new System.EventHandler(this.ButtonCancel_Click);
            // 
            // NameRobotForm
            // 
            this.AcceptButton = this.ButtonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.CancelButton = this.ButtonCancel;
            this.ClientSize = new System.Drawing.Size(434, 121);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonBrowse);
            this.Controls.Add(this.PathTextBox);
            this.Controls.Add(this.NameRobotLabel);
            this.Controls.Add(this.RobotNameTextBox);
            this.Controls.Add(this.SaveLocationLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NameRobotForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Name Robot";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label SaveLocationLabel;
        private System.Windows.Forms.TextBox RobotNameTextBox;
        private System.Windows.Forms.Label NameRobotLabel;
        private System.Windows.Forms.TextBox PathTextBox;
        private System.Windows.Forms.Button ButtonBrowse;
        private System.Windows.Forms.FolderBrowserDialog ExportLocationDialog;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
    }
}