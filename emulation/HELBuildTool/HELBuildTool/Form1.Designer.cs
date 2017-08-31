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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label2 = new System.Windows.Forms.Label();
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
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Select Programming Language";
            // 
            // txtBrowseJava
            // 
            this.txtBrowseJava.Location = new System.Drawing.Point(12, 139);
            this.txtBrowseJava.Name = "txtBrowseJava";
            this.txtBrowseJava.Size = new System.Drawing.Size(460, 20);
            this.txtBrowseJava.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 123);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Java Main Class Path";
            // 
            // javaButton
            // 
            this.javaButton.AutoSize = true;
            this.javaButton.Location = new System.Drawing.Point(64, 64);
            this.javaButton.Name = "javaButton";
            this.javaButton.Size = new System.Drawing.Size(48, 17);
            this.javaButton.TabIndex = 21;
            this.javaButton.TabStop = true;
            this.javaButton.Text = "Java";
            this.javaButton.UseVisualStyleBackColor = true;
            // 
            // cppButton
            // 
            this.cppButton.AutoSize = true;
            this.cppButton.Checked = true;
            this.cppButton.Location = new System.Drawing.Point(12, 64);
            this.cppButton.Name = "cppButton";
            this.cppButton.Size = new System.Drawing.Size(44, 17);
            this.cppButton.TabIndex = 20;
            this.cppButton.TabStop = true;
            this.cppButton.Text = "C++";
            this.cppButton.UseVisualStyleBackColor = true;
            this.cppButton.CheckedChanged += new System.EventHandler(this.cppButton_CheckedChanged);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(397, 99);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 19;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtBrowse
            // 
            this.txtBrowse.Location = new System.Drawing.Point(12, 100);
            this.txtBrowse.Name = "txtBrowse";
            this.txtBrowse.Size = new System.Drawing.Size(379, 20);
            this.txtBrowse.TabIndex = 18;
            // 
            // lbBrowse
            // 
            this.lbBrowse.AutoSize = true;
            this.lbBrowse.Location = new System.Drawing.Point(9, 84);
            this.lbBrowse.Margin = new System.Windows.Forms.Padding(0);
            this.lbBrowse.Name = "lbBrowse";
            this.lbBrowse.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lbBrowse.Size = new System.Drawing.Size(109, 13);
            this.lbBrowse.TabIndex = 17;
            this.lbBrowse.Text = "Robot Code Directory";
            this.lbBrowse.Click += new System.EventHandler(this.lbBrowse_Click);
            // 
            // btnSetup
            // 
            this.btnSetup.Location = new System.Drawing.Point(12, 165);
            this.btnSetup.Name = "btnSetup";
            this.btnSetup.Size = new System.Drawing.Size(225, 23);
            this.btnSetup.TabIndex = 16;
            this.btnSetup.Text = "Remove Compiled Binaries";
            this.btnSetup.UseVisualStyleBackColor = true;
            this.btnSetup.Click += new System.EventHandler(this.btnClean_Click);
            // 
            // btnRunCode
            // 
            this.btnRunCode.Location = new System.Drawing.Point(247, 165);
            this.btnRunCode.Name = "btnRunCode";
            this.btnRunCode.Size = new System.Drawing.Size(225, 23);
            this.btnRunCode.TabIndex = 15;
            this.btnRunCode.Text = "Build and Run Code";
            this.btnRunCode.UseVisualStyleBackColor = true;
            this.btnRunCode.Click += new System.EventHandler(this.btnRunCode_Click);
            // 
            // txtNumber
            // 
            this.txtNumber.Location = new System.Drawing.Point(12, 25);
            this.txtNumber.Name = "txtNumber";
            this.txtNumber.Size = new System.Drawing.Size(99, 20);
            this.txtNumber.TabIndex = 14;
            // 
            // lbNumber
            // 
            this.lbNumber.AutoSize = true;
            this.lbNumber.Location = new System.Drawing.Point(9, 9);
            this.lbNumber.Margin = new System.Windows.Forms.Padding(0);
            this.lbNumber.Name = "lbNumber";
            this.lbNumber.Size = new System.Drawing.Size(102, 13);
            this.lbNumber.TabIndex = 13;
            this.lbNumber.Text = "Enter Team Number";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 197);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtBrowseJava);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.javaButton);
            this.Controls.Add(this.cppButton);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.txtBrowse);
            this.Controls.Add(this.lbBrowse);
            this.Controls.Add(this.btnSetup);
            this.Controls.Add(this.btnRunCode);
            this.Controls.Add(this.txtNumber);
            this.Controls.Add(this.lbNumber);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Run User Code";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBrowseJava;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton javaButton;
        private System.Windows.Forms.RadioButton cppButton;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtBrowse;
        private System.Windows.Forms.Label lbBrowse;
        private System.Windows.Forms.Button btnSetup;
        private System.Windows.Forms.Button btnRunCode;
        private System.Windows.Forms.TextBox txtNumber;
        private System.Windows.Forms.Label lbNumber;
    }
}

