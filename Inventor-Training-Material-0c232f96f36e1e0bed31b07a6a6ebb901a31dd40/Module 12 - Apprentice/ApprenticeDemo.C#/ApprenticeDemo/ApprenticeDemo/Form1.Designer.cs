namespace ApprenticeDemo
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
            this.OpenButton = new System.Windows.Forms.Button();
            this.ViewerButton = new System.Windows.Forms.Button();
            this.btnSetPro = new System.Windows.Forms.Button();
            this.PreviewPic = new System.Windows.Forms.PictureBox();
            this.TextBoxAuthor = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbFilename = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPic)).BeginInit();
            this.SuspendLayout();
            // 
            // OpenButton
            // 
            this.OpenButton.Location = new System.Drawing.Point(34, 422);
            this.OpenButton.Name = "OpenButton";
            this.OpenButton.Size = new System.Drawing.Size(421, 41);
            this.OpenButton.TabIndex = 0;
            this.OpenButton.Text = "Open Inventor File";
            this.OpenButton.UseVisualStyleBackColor = true;
            this.OpenButton.Click += new System.EventHandler(this.OpenButton_Click);
            // 
            // ViewerButton
            // 
            this.ViewerButton.Location = new System.Drawing.Point(34, 469);
            this.ViewerButton.Name = "ViewerButton";
            this.ViewerButton.Size = new System.Drawing.Size(421, 43);
            this.ViewerButton.TabIndex = 0;
            this.ViewerButton.Text = "Viewer with View control";
            this.ViewerButton.UseVisualStyleBackColor = true;
            this.ViewerButton.Click += new System.EventHandler(this.ViewerButton_Click);
            // 
            // btnSetPro
            // 
            this.btnSetPro.Location = new System.Drawing.Point(34, 518);
            this.btnSetPro.Name = "btnSetPro";
            this.btnSetPro.Size = new System.Drawing.Size(305, 37);
            this.btnSetPro.TabIndex = 0;
            this.btnSetPro.Text = "Set Property [Author]";
            this.btnSetPro.UseVisualStyleBackColor = true;
            this.btnSetPro.Click += new System.EventHandler(this.btnSetPro_Click);
            // 
            // PreviewPic
            // 
            this.PreviewPic.Location = new System.Drawing.Point(25, 38);
            this.PreviewPic.Name = "PreviewPic";
            this.PreviewPic.Size = new System.Drawing.Size(430, 362);
            this.PreviewPic.TabIndex = 1;
            this.PreviewPic.TabStop = false;
            // 
            // TextBoxAuthor
            // 
            this.TextBoxAuthor.Location = new System.Drawing.Point(345, 526);
            this.TextBoxAuthor.Name = "TextBoxAuthor";
            this.TextBoxAuthor.Size = new System.Drawing.Size(110, 25);
            this.TextBoxAuthor.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "view in picturebox";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(177, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(0, 15);
            this.label2.TabIndex = 3;
            // 
            // lbFilename
            // 
            this.lbFilename.AutoSize = true;
            this.lbFilename.Location = new System.Drawing.Point(223, 10);
            this.lbFilename.Name = "lbFilename";
            this.lbFilename.Size = new System.Drawing.Size(55, 15);
            this.lbFilename.TabIndex = 4;
            this.lbFilename.Text = "label3";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(468, 567);
            this.Controls.Add(this.lbFilename);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextBoxAuthor);
            this.Controls.Add(this.PreviewPic);
            this.Controls.Add(this.btnSetPro);
            this.Controls.Add(this.ViewerButton);
            this.Controls.Add(this.OpenButton);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPic)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OpenButton;
        private System.Windows.Forms.Button ViewerButton;
        private System.Windows.Forms.Button btnSetPro;
        private System.Windows.Forms.PictureBox PreviewPic;
        private System.Windows.Forms.TextBox TextBoxAuthor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbFilename;
    }
}

