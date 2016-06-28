namespace AsmDocEssentials
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
            this.Label1 = new System.Windows.Forms.Label();
            this.Button2 = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            this.VectorControl2 = new AsmDocEssentials.VectorControl();
            this.VectorControl1 = new AsmDocEssentials.VectorControl();
            this.button4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button3
            // 
            this.Button3.Location = new System.Drawing.Point(11, 51);
            this.Button3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button3.Name = "Button3";
            this.Button3.Size = new System.Drawing.Size(165, 27);
            this.Button3.TabIndex = 11;
            this.Button3.Text = "Add Occurrence";
            this.Button3.UseVisualStyleBackColor = true;
            this.Button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 232);
            this.Label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(111, 15);
            this.Label1.TabIndex = 10;
            this.Label1.Text = "Angle (Deg.):";
            // 
            // Button2
            // 
            this.Button2.Location = new System.Drawing.Point(11, 261);
            this.Button2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(165, 36);
            this.Button2.TabIndex = 9;
            this.Button2.Text = "Transform Occurrence";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Button1
            // 
            this.Button1.Location = new System.Drawing.Point(11, 18);
            this.Button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(165, 25);
            this.Button1.TabIndex = 8;
            this.Button1.Text = "Create Assembly";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // VectorControl2
            // 
            this.VectorControl2.Location = new System.Drawing.Point(11, 170);
            this.VectorControl2.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.VectorControl2.Name = "VectorControl2";
            this.VectorControl2.Size = new System.Drawing.Size(165, 52);
            this.VectorControl2.TabIndex = 7;
            this.VectorControl2.VectorName = "Axis vector:";
            // 
            // VectorControl1
            // 
            this.VectorControl1.Location = new System.Drawing.Point(11, 115);
            this.VectorControl1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.VectorControl1.Name = "VectorControl1";
            this.VectorControl1.Size = new System.Drawing.Size(165, 52);
            this.VectorControl1.TabIndex = 6;
            this.VectorControl1.VectorName = "Translation vector:";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(11, 318);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(164, 38);
            this.button4.TabIndex = 12;
            this.button4.Text = "Lab Demo-Constraint";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(192, 368);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.Button3);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.VectorControl2);
            this.Controls.Add(this.VectorControl1);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "Assembly";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button Button3;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.Button Button2;
        internal System.Windows.Forms.Button Button1;
        internal VectorControl VectorControl2;
        internal VectorControl VectorControl1;
        private System.Windows.Forms.Button button4;
    }
}

