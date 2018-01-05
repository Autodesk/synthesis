namespace EditorsLibrary
{
    partial class PluginSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PluginSettingsForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.groupboxColor = new System.Windows.Forms.GroupBox();
            this.ChildHighlight = new System.Windows.Forms.Button();
            this.ChildLabel = new System.Windows.Forms.Label();
            this.groupboxGeneral = new System.Windows.Forms.GroupBox();
            this.UseFancyColorsCheckBox = new System.Windows.Forms.CheckBox();
            this.SaveLocationLabel = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.SaveLocationTextBox = new System.Windows.Forms.TextBox();
            this.groupboxColor.SuspendLayout();
            this.groupboxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(16, 163);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(120, 29);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonOK.Location = new System.Drawing.Point(155, 163);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(120, 29);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // groupboxColor
            // 
            this.groupboxColor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupboxColor.Controls.Add(this.ChildHighlight);
            this.groupboxColor.Controls.Add(this.ChildLabel);
            this.groupboxColor.ForeColor = System.Drawing.Color.Gray;
            this.groupboxColor.Location = new System.Drawing.Point(11, 106);
            this.groupboxColor.Margin = new System.Windows.Forms.Padding(2);
            this.groupboxColor.Name = "groupboxColor";
            this.groupboxColor.Padding = new System.Windows.Forms.Padding(2);
            this.groupboxColor.Size = new System.Drawing.Size(264, 53);
            this.groupboxColor.TabIndex = 12;
            this.groupboxColor.TabStop = false;
            this.groupboxColor.Text = "Inventor";
            // 
            // ChildHighlight
            // 
            this.ChildHighlight.BackColor = System.Drawing.Color.Black;
            this.ChildHighlight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChildHighlight.Location = new System.Drawing.Point(126, 21);
            this.ChildHighlight.Margin = new System.Windows.Forms.Padding(2);
            this.ChildHighlight.Name = "ChildHighlight";
            this.ChildHighlight.Size = new System.Drawing.Size(76, 20);
            this.ChildHighlight.TabIndex = 9;
            this.ChildHighlight.UseVisualStyleBackColor = false;
            this.ChildHighlight.Click += new System.EventHandler(this.ChildHighlight_Click);
            // 
            // ChildLabel
            // 
            this.ChildLabel.AutoSize = true;
            this.ChildLabel.ForeColor = System.Drawing.Color.Black;
            this.ChildLabel.Location = new System.Drawing.Point(2, 24);
            this.ChildLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ChildLabel.Name = "ChildLabel";
            this.ChildLabel.Size = new System.Drawing.Size(101, 13);
            this.ChildLabel.TabIndex = 6;
            this.ChildLabel.Text = "Child Highlight Color";
            // 
            // groupboxGeneral
            // 
            this.groupboxGeneral.Controls.Add(this.UseFancyColorsCheckBox);
            this.groupboxGeneral.Controls.Add(this.SaveLocationLabel);
            this.groupboxGeneral.Controls.Add(this.buttonBrowse);
            this.groupboxGeneral.Controls.Add(this.SaveLocationTextBox);
            this.groupboxGeneral.Location = new System.Drawing.Point(11, 12);
            this.groupboxGeneral.Name = "groupboxGeneral";
            this.groupboxGeneral.Size = new System.Drawing.Size(264, 88);
            this.groupboxGeneral.TabIndex = 13;
            this.groupboxGeneral.TabStop = false;
            this.groupboxGeneral.Text = "General";
            // 
            // UseFancyColorsCheckBox
            // 
            this.UseFancyColorsCheckBox.AutoSize = true;
            this.UseFancyColorsCheckBox.Location = new System.Drawing.Point(6, 65);
            this.UseFancyColorsCheckBox.Name = "UseFancyColorsCheckBox";
            this.UseFancyColorsCheckBox.Size = new System.Drawing.Size(119, 17);
            this.UseFancyColorsCheckBox.TabIndex = 3;
            this.UseFancyColorsCheckBox.Text = "Fancy Colors (Slow)";
            this.UseFancyColorsCheckBox.UseVisualStyleBackColor = true;
            this.UseFancyColorsCheckBox.CheckedChanged += new System.EventHandler(this.UseFancyColorsCheckBox_CheckedChanged);
            // 
            // SaveLocationLabel
            // 
            this.SaveLocationLabel.AutoSize = true;
            this.SaveLocationLabel.Location = new System.Drawing.Point(7, 21);
            this.SaveLocationLabel.Name = "SaveLocationLabel";
            this.SaveLocationLabel.Size = new System.Drawing.Size(108, 13);
            this.SaveLocationLabel.TabIndex = 2;
            this.SaveLocationLabel.Text = "Robot Save Location";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(232, 40);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(26, 20);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.ButtonBrowse_Click);
            // 
            // SaveLocationTextBox
            // 
            this.SaveLocationTextBox.Location = new System.Drawing.Point(7, 40);
            this.SaveLocationTextBox.Name = "SaveLocationTextBox";
            this.SaveLocationTextBox.ReadOnly = true;
            this.SaveLocationTextBox.Size = new System.Drawing.Size(219, 20);
            this.SaveLocationTextBox.TabIndex = 0;
            // 
            // PluginSettingsForm
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(286, 203);
            this.Controls.Add(this.groupboxGeneral);
            this.Controls.Add(this.groupboxColor);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PluginSettingsForm";
            this.Text = "Exporter Settings";
            this.groupboxColor.ResumeLayout(false);
            this.groupboxColor.PerformLayout();
            this.groupboxGeneral.ResumeLayout(false);
            this.groupboxGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.GroupBox groupboxColor;
        private System.Windows.Forms.Button ChildHighlight;
        private System.Windows.Forms.Label ChildLabel;
        private System.Windows.Forms.GroupBox groupboxGeneral;
        private System.Windows.Forms.Label SaveLocationLabel;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox SaveLocationTextBox;
        private System.Windows.Forms.CheckBox UseFancyColorsCheckBox;
    }
}