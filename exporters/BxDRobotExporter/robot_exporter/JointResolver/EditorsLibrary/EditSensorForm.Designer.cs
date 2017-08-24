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
            this.saveButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.secondaryBox = new System.Windows.Forms.CheckBox();
            this.coefficentTextBox = new System.Windows.Forms.TextBox();
            this.lblEquationParsed = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Sensor Type:";
            // 
            // typeBox
            // 
            this.typeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.typeBox.FormattingEnabled = true;
            this.typeBox.Location = new System.Drawing.Point(87, 10);
            this.typeBox.Margin = new System.Windows.Forms.Padding(2);
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(202, 21);
            this.typeBox.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 67);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(39, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port #:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 39);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Module #:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(68, 67);
            this.portTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(36, 20);
            this.portTextBox.TabIndex = 2;
            // 
            // moduleTextBox
            // 
            this.moduleTextBox.Location = new System.Drawing.Point(68, 39);
            this.moduleTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.moduleTextBox.Name = "moduleTextBox";
            this.moduleTextBox.Size = new System.Drawing.Size(36, 20);
            this.moduleTextBox.TabIndex = 1;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(155, 124);
            this.saveButton.Margin = new System.Windows.Forms.Padding(2);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(134, 28);
            this.saveButton.TabIndex = 0;
            this.saveButton.Text = "Save Sensor";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(146, 42);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(118, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Polynomial Coefficients:";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // secondaryBox
            // 
            this.secondaryBox.AutoSize = true;
            this.secondaryBox.Location = new System.Drawing.Point(27, 99);
            this.secondaryBox.Margin = new System.Windows.Forms.Padding(2);
            this.secondaryBox.Name = "secondaryBox";
            this.secondaryBox.Size = new System.Drawing.Size(77, 17);
            this.secondaryBox.TabIndex = 4;
            this.secondaryBox.Text = "Use Linear";
            this.secondaryBox.UseVisualStyleBackColor = true;
            this.secondaryBox.Visible = false;
            this.secondaryBox.CheckedChanged += new System.EventHandler(this.secondaryBox_CheckedChanged);
            // 
            // coefficentTextBox
            // 
            this.coefficentTextBox.Location = new System.Drawing.Point(149, 67);
            this.coefficentTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.coefficentTextBox.Name = "coefficentTextBox";
            this.coefficentTextBox.Size = new System.Drawing.Size(115, 20);
            this.coefficentTextBox.TabIndex = 3;
            this.coefficentTextBox.TextChanged += new System.EventHandler(this.coefficentTextBox_TextChanged);
            // 
            // lblEquationParsed
            // 
            this.lblEquationParsed.AutoSize = true;
            this.lblEquationParsed.Location = new System.Drawing.Point(146, 99);
            this.lblEquationParsed.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblEquationParsed.Name = "lblEquationParsed";
            this.lblEquationParsed.Size = new System.Drawing.Size(66, 13);
            this.lblEquationParsed.TabIndex = 11;
            this.lblEquationParsed.Text = "Polynomial...";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(11, 124);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(134, 28);
            this.button1.TabIndex = 12;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // EditSensorForm
            // 
            this.AcceptButton = this.saveButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 168);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblEquationParsed);
            this.Controls.Add(this.coefficentTextBox);
            this.Controls.Add(this.secondaryBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.moduleTextBox);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.typeBox);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "EditSensorForm";
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
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
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox secondaryBox;
        private System.Windows.Forms.TextBox coefficentTextBox;
        private System.Windows.Forms.Label lblEquationParsed;
        private System.Windows.Forms.Button button1;
    }
}