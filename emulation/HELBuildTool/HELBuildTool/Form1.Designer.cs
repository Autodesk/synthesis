namespace WindowsFormsApp1
{
    partial class Form1
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBrowseJava = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.javaButton = new System.Windows.Forms.RadioButton();
            this.cppButton = new System.Windows.Forms.RadioButton();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtBrowse = new System.Windows.Forms.TextBox();
            this.lbBrowse = new System.Windows.Forms.Label();
            this.btnSetup = new System.Windows.Forms.Button();
            this.btnRunCode = new System.Windows.Forms.Button();
            this.txtNumber = new System.Windows.Forms.TextBox();
            this.lbNumber = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBrowseJava);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.javaButton);
            this.groupBox1.Controls.Add(this.cppButton);
            this.groupBox1.Controls.Add(this.btnBrowse);
            this.groupBox1.Controls.Add(this.txtBrowse);
            this.groupBox1.Controls.Add(this.lbBrowse);
            this.groupBox1.Controls.Add(this.btnSetup);
            this.groupBox1.Controls.Add(this.btnRunCode);
            this.groupBox1.Controls.Add(this.txtNumber);
            this.groupBox1.Controls.Add(this.lbNumber);
            this.groupBox1.Location = new System.Drawing.Point(34, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(202, 330);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Build Information";
            // 
            // txtBrowseJava
            // 
            this.txtBrowseJava.Location = new System.Drawing.Point(21, 194);
            this.txtBrowseJava.Name = "txtBrowseJava";
            this.txtBrowseJava.Size = new System.Drawing.Size(100, 20);
            this.txtBrowseJava.TabIndex = 11;
            this.txtBrowseJava.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 167);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Enter the main Java class path";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // javaButton
            // 
            this.javaButton.AutoSize = true;
            this.javaButton.Location = new System.Drawing.Point(21, 254);
            this.javaButton.Name = "javaButton";
            this.javaButton.Size = new System.Drawing.Size(48, 17);
            this.javaButton.TabIndex = 9;
            this.javaButton.TabStop = true;
            this.javaButton.Text = "Java";
            this.javaButton.UseVisualStyleBackColor = true;
            // 
            // cppButton
            // 
            this.cppButton.AutoSize = true;
            this.cppButton.Checked = true;
            this.cppButton.Location = new System.Drawing.Point(21, 231);
            this.cppButton.Name = "cppButton";
            this.cppButton.Size = new System.Drawing.Size(44, 17);
            this.cppButton.TabIndex = 8;
            this.cppButton.TabStop = true;
            this.cppButton.Text = "C++";
            this.cppButton.UseVisualStyleBackColor = true;
            this.cppButton.CheckedChanged += new System.EventHandler(this.cppButton_CheckedChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(128, 124);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtBrowse
            // 
            this.txtBrowse.Location = new System.Drawing.Point(21, 124);
            this.txtBrowse.Name = "txtBrowse";
            this.txtBrowse.Size = new System.Drawing.Size(100, 20);
            this.txtBrowse.TabIndex = 6;
            // 
            // lbBrowse
            // 
            this.lbBrowse.AutoSize = true;
            this.lbBrowse.Location = new System.Drawing.Point(21, 97);
            this.lbBrowse.Name = "lbBrowse";
            this.lbBrowse.Size = new System.Drawing.Size(157, 13);
            this.lbBrowse.TabIndex = 5;
            this.lbBrowse.Text = "Browse for robot code directory:";
            // 
            // btnSetup
            // 
            this.btnSetup.Location = new System.Drawing.Point(103, 291);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(75, 23);
            this.btnSetup.TabIndex = 3;
            this.btnSetup.Text = "Setup";
            this.btnSetup.UseVisualStyleBackColor = true;
            this.btnSetup.Click += new System.EventHandler(this.btnSetup_Click);
            // 
            // btnRunCode
            // 
            this.btnRunCode.Location = new System.Drawing.Point(21, 291);
            this.btnRunCode.Name = "btnRunCode";
            this.btnRunCode.Size = new System.Drawing.Size(75, 23);
            this.btnRunCode.TabIndex = 2;
            this.btnRunCode.Text = "Run Code";
            this.btnRunCode.UseVisualStyleBackColor = true;
            this.btnRunCode.Click += new System.EventHandler(this.btnRunCode_Click);
            // 
            // txtNumber
            // 
            this.txtNumber.Location = new System.Drawing.Point(21, 58);
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.Size = new System.Drawing.Size(100, 20);
            this.txtNumber.TabIndex = 1;
            // 
            // lbNumber
            // 
            this.lbNumber.AutoSize = true;
            this.lbNumber.Location = new System.Drawing.Point(18, 32);
            this.lbNumber.Name = "lbNumber";
            this.lbNumber.Size = new System.Drawing.Size(122, 13);
            this.lbNumber.TabIndex = 0;
            this.lbNumber.Text = "Enter your team number:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 354);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "HELBuildTool";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lbNumber;
        private System.Windows.Forms.TextBox txtNumber;
        private System.Windows.Forms.Button btnSetup;
        private System.Windows.Forms.Button btnRunCode;
        private System.Windows.Forms.Label lbBrowse;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox txtBrowse;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.RadioButton javaButton;
        private System.Windows.Forms.RadioButton cppButton;
        private System.Windows.Forms.TextBox txtBrowseJava;
        private System.Windows.Forms.Label label1;
    }
}

