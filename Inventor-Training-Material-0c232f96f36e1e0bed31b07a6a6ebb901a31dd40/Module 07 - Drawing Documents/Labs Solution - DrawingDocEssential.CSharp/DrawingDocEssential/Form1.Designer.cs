namespace DrawingDocEssential
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
            this.Button3 = new System.Windows.Forms.Button();
            this.Border2 = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button3
            // 
            this.Button3.AutoSize = true;
            this.Button3.Location = new System.Drawing.Point(12, 94);
            this.Button3.Name = "Button3";
            this.Button3.Size = new System.Drawing.Size(161, 38);
            this.Button3.TabIndex = 9;
            this.Button3.Text = "Drawing Complete ";
            this.Button3.UseVisualStyleBackColor = true;
            this.Button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // Border2
            // 
            this.Border2.Location = new System.Drawing.Point(12, 50);
            this.Border2.Name = "Border2";
            this.Border2.Size = new System.Drawing.Size(161, 38);
            this.Border2.TabIndex = 8;
            this.Border2.Text = "Add BorderDefinition";
            this.Border2.UseVisualStyleBackColor = true;
            this.Border2.Click += new System.EventHandler(this.Border2_Click);
            // 
            // Button1
            // 
            this.Button1.AutoSize = true;
            this.Button1.Location = new System.Drawing.Point(12, 12);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(161, 32);
            this.Button1.TabIndex = 7;
            this.Button1.Text = "Add TitleBlock";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(184, 148);
            this.Controls.Add(this.Button3);
            this.Controls.Add(this.Border2);
            this.Controls.Add(this.Button1);
            this.Name = "Form1";
            this.Text = "Drawing Doc";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button Button3;
        internal System.Windows.Forms.Button Border2;
        internal System.Windows.Forms.Button Button1;
    }
}

