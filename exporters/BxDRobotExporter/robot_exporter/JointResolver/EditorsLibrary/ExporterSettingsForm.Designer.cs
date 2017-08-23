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
            this.HighlightParentsCheckBox = new System.Windows.Forms.CheckBox();
            this.ChildHighlight = new System.Windows.Forms.Button();
            this.ParentHighlight = new System.Windows.Forms.Button();
            this.ParentLabel = new System.Windows.Forms.Label();
            this.ChildLabel = new System.Windows.Forms.Label();
            this.groupboxGeneral = new System.Windows.Forms.GroupBox();
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
            this.buttonCancel.Location = new System.Drawing.Point(16, 207);
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
            this.buttonOK.Location = new System.Drawing.Point(155, 207);
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
            this.groupboxColor.Controls.Add(this.HighlightParentsCheckBox);
            this.groupboxColor.Controls.Add(this.ChildHighlight);
            this.groupboxColor.Controls.Add(this.ParentHighlight);
            this.groupboxColor.Controls.Add(this.ParentLabel);
            this.groupboxColor.Controls.Add(this.ChildLabel);
            this.groupboxColor.ForeColor = System.Drawing.Color.Gray;
            this.groupboxColor.Location = new System.Drawing.Point(11, 98);
            this.groupboxColor.Margin = new System.Windows.Forms.Padding(2);
            this.groupboxColor.Name = "groupboxColor";
            this.groupboxColor.Padding = new System.Windows.Forms.Padding(2);
            this.groupboxColor.Size = new System.Drawing.Size(264, 105);
            this.groupboxColor.TabIndex = 12;
            this.groupboxColor.TabStop = false;
            this.groupboxColor.Text = "Inventor";
            // 
            // HighlightParentsCheckBox
            // 
            this.HighlightParentsCheckBox.AutoSize = true;
            this.HighlightParentsCheckBox.ForeColor = System.Drawing.Color.Black;
            this.HighlightParentsCheckBox.Location = new System.Drawing.Point(2, 83);
            this.HighlightParentsCheckBox.Name = "HighlightParentsCheckBox";
            this.HighlightParentsCheckBox.Size = new System.Drawing.Size(135, 17);
            this.HighlightParentsCheckBox.TabIndex = 10;
            this.HighlightParentsCheckBox.Text = "Highlight Parent Nodes";
            this.HighlightParentsCheckBox.UseVisualStyleBackColor = true;
            this.HighlightParentsCheckBox.CheckedChanged += new System.EventHandler(this.HighlightParentsCheckBox_CheckedChanged);
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
            // ParentHighlight
            // 
            this.ParentHighlight.BackColor = System.Drawing.Color.Black;
            this.ParentHighlight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ParentHighlight.Location = new System.Drawing.Point(126, 51);
            this.ParentHighlight.Margin = new System.Windows.Forms.Padding(2);
            this.ParentHighlight.Name = "ParentHighlight";
            this.ParentHighlight.Size = new System.Drawing.Size(76, 20);
            this.ParentHighlight.TabIndex = 8;
            this.ParentHighlight.UseVisualStyleBackColor = false;
            this.ParentHighlight.Click += new System.EventHandler(this.ParentHighlight_Click);
            // 
            // ParentLabel
            // 
            this.ParentLabel.AutoSize = true;
            this.ParentLabel.ForeColor = System.Drawing.Color.Black;
            this.ParentLabel.Location = new System.Drawing.Point(2, 54);
            this.ParentLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.ParentLabel.Name = "ParentLabel";
            this.ParentLabel.Size = new System.Drawing.Size(109, 13);
            this.ParentLabel.TabIndex = 7;
            this.ParentLabel.Text = "Parent Highlight Color";
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
            this.groupboxGeneral.Controls.Add(this.SaveLocationLabel);
            this.groupboxGeneral.Controls.Add(this.buttonBrowse);
            this.groupboxGeneral.Controls.Add(this.SaveLocationTextBox);
            this.groupboxGeneral.Location = new System.Drawing.Point(11, 13);
            this.groupboxGeneral.Name = "groupboxGeneral";
            this.groupboxGeneral.Size = new System.Drawing.Size(264, 76);
            this.groupboxGeneral.TabIndex = 13;
            this.groupboxGeneral.TabStop = false;
            this.groupboxGeneral.Text = "General";
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
            this.ClientSize = new System.Drawing.Size(286, 247);
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
        private System.Windows.Forms.CheckBox HighlightParentsCheckBox;
        private System.Windows.Forms.Button ChildHighlight;
        private System.Windows.Forms.Button ParentHighlight;
        private System.Windows.Forms.Label ParentLabel;
        private System.Windows.Forms.Label ChildLabel;
        private System.Windows.Forms.GroupBox groupboxGeneral;
        private System.Windows.Forms.Label SaveLocationLabel;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.TextBox SaveLocationTextBox;
    }
}