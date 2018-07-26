namespace EditorsLibrary
{
    partial class EditSensorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditSensorForm));
            this.label1 = new System.Windows.Forms.Label();
            this.typeBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.moduleTextBox = new System.Windows.Forms.TextBox();
            this.SaveButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.secondaryBox = new System.Windows.Forms.CheckBox();
            this.coefficentTextBox = new System.Windows.Forms.TextBox();
            this.lblEquationParsed = new System.Windows.Forms.Label();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Sensor Type:";
            // 
            // typeBox
            // 
            this.typeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeBox.FormattingEnabled = true;
            this.typeBox.Location = new System.Drawing.Point(116, 12);
            this.typeBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(268, 24);
            this.typeBox.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port #:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Module #:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(91, 82);
            this.portTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(47, 22);
            this.portTextBox.TabIndex = 2;
            // 
            // moduleTextBox
            // 
            this.moduleTextBox.Location = new System.Drawing.Point(91, 48);
            this.moduleTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.moduleTextBox.Name = "moduleTextBox";
            this.moduleTextBox.Size = new System.Drawing.Size(47, 22);
            this.moduleTextBox.TabIndex = 1;
            // 
            // SaveButton
            // 
            this.SaveButton.Location = new System.Drawing.Point(207, 153);
            this.SaveButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SaveButton.Name = "SaveButton";
            this.SaveButton.Size = new System.Drawing.Size(179, 34);
            this.SaveButton.TabIndex = 0;
            this.SaveButton.Text = "Save Sensor";
            this.SaveButton.UseVisualStyleBackColor = true;
            this.SaveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(195, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Polynomial Coefficients:";
            // 
            // secondaryBox
            // 
            this.secondaryBox.AutoSize = true;
            this.secondaryBox.Location = new System.Drawing.Point(36, 122);
            this.secondaryBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.secondaryBox.Name = "secondaryBox";
            this.secondaryBox.Size = new System.Drawing.Size(99, 21);
            this.secondaryBox.TabIndex = 4;
            this.secondaryBox.Text = "Use Linear";
            this.secondaryBox.UseVisualStyleBackColor = true;
            this.secondaryBox.Visible = false;
            // 
            // coefficentTextBox
            // 
            this.coefficentTextBox.Location = new System.Drawing.Point(199, 82);
            this.coefficentTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.coefficentTextBox.Name = "coefficentTextBox";
            this.coefficentTextBox.Size = new System.Drawing.Size(152, 22);
            this.coefficentTextBox.TabIndex = 3;
            this.coefficentTextBox.TextChanged += new System.EventHandler(this.coefficentTextBox_TextChanged);
            // 
            // lblEquationParsed
            // 
            this.lblEquationParsed.AutoSize = true;
            this.lblEquationParsed.Location = new System.Drawing.Point(195, 122);
            this.lblEquationParsed.Name = "lblEquationParsed";
            this.lblEquationParsed.Size = new System.Drawing.Size(88, 17);
            this.lblEquationParsed.TabIndex = 11;
            this.lblEquationParsed.Text = "Polynomial...";
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(15, 153);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(179, 34);
            this.CancelButton.TabIndex = 12;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // EditSensorForm
            // 
            this.AcceptButton = this.SaveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 207);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.lblEquationParsed);
            this.Controls.Add(this.coefficentTextBox);
            this.Controls.Add(this.secondaryBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.SaveButton);
            this.Controls.Add(this.moduleTextBox);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.typeBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditSensorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sensor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox typeBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.TextBox moduleTextBox;
        private System.Windows.Forms.Button SaveButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox secondaryBox;
        private System.Windows.Forms.TextBox coefficentTextBox;
        private System.Windows.Forms.Label lblEquationParsed;
        private System.Windows.Forms.Button CancelButton;
    }
}