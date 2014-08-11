namespace JointResolver.ControlGUI
{
    partial class AddSensorForm
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
            this.typeBox.Name = "typeBox";
            this.typeBox.Size = new System.Drawing.Size(128, 24);
            this.typeBox.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(130, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port #:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "Module #:";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(186, 57);
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(47, 22);
            this.portTextBox.TabIndex = 4;
            this.portTextBox.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // moduleTextBox
            // 
            this.moduleTextBox.Location = new System.Drawing.Point(77, 59);
            this.moduleTextBox.Name = "moduleTextBox";
            this.moduleTextBox.Size = new System.Drawing.Size(47, 22);
            this.moduleTextBox.TabIndex = 5;
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(77, 195);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(104, 34);
            this.saveButton.TabIndex = 6;
            this.saveButton.Text = "Save Sensor";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(57, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(157, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Polynomial Coefficients:";
            // 
            // secondaryBox
            // 
            this.secondaryBox.AutoSize = true;
            this.secondaryBox.Location = new System.Drawing.Point(77, 168);
            this.secondaryBox.Name = "secondaryBox";
            this.secondaryBox.Size = new System.Drawing.Size(99, 21);
            this.secondaryBox.TabIndex = 9;
            this.secondaryBox.Text = "Use Linear";
            this.secondaryBox.UseVisualStyleBackColor = true;
            this.secondaryBox.Visible = false;
            // 
            // coefficentTextBox
            // 
            this.coefficentTextBox.Location = new System.Drawing.Point(60, 114);
            this.coefficentTextBox.Name = "coefficentTextBox";
            this.coefficentTextBox.Size = new System.Drawing.Size(143, 22);
            this.coefficentTextBox.TabIndex = 10;
            this.coefficentTextBox.TextChanged += new System.EventHandler(this.coefficentTextBox_TextChanged);
            // 
            // lblEquationParsed
            // 
            this.lblEquationParsed.AutoSize = true;
            this.lblEquationParsed.Location = new System.Drawing.Point(57, 139);
            this.lblEquationParsed.Name = "lblEquationParsed";
            this.lblEquationParsed.Size = new System.Drawing.Size(88, 17);
            this.lblEquationParsed.TabIndex = 11;
            this.lblEquationParsed.Text = "Polynomial...";
            // 
            // AddSensorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 241);
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
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximumSize = new System.Drawing.Size(300, 286);
            this.MinimumSize = new System.Drawing.Size(300, 286);
            this.Name = "AddSensorForm";
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
    }
}