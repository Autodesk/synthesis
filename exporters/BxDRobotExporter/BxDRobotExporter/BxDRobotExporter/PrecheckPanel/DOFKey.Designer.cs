namespace BxDRobotExporter.PrecheckPanel
{
    partial class DOFKey
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DOFKey));
            this.hookHTML = new System.Windows.Forms.WebBrowser();
            this.green = new System.Windows.Forms.PictureBox();
            this.blue = new System.Windows.Forms.PictureBox();
            this.red = new System.Windows.Forms.PictureBox();
            this.blueDescription = new System.Windows.Forms.TextBox();
            this.greenDescription = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.green)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.blue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.red)).BeginInit();
            this.SuspendLayout();
            // 
            // hookHTML
            // 
            this.hookHTML.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hookHTML.Location = new System.Drawing.Point(0, 0);
            this.hookHTML.MinimumSize = new System.Drawing.Size(20, 20);
            this.hookHTML.Name = "hookHTML";
            this.hookHTML.Size = new System.Drawing.Size(220, 120);
            this.hookHTML.TabIndex = 0;
            // 
            // green
            // 
            this.green.Image = ((System.Drawing.Image)(resources.GetObject("green.Image")));
            this.green.InitialImage = ((System.Drawing.Image)(resources.GetObject("green.InitialImage")));
            this.green.Location = new System.Drawing.Point(10, 30);
            this.green.Name = "green";
            this.green.Size = new System.Drawing.Size(20, 20);
            this.green.TabIndex = 1;
            this.green.TabStop = false;
            // 
            // blue
            // 
            this.blue.Image = ((System.Drawing.Image)(resources.GetObject("blue.Image")));
            this.blue.Location = new System.Drawing.Point(10, 5);
            this.blue.Name = "blue";
            this.blue.Size = new System.Drawing.Size(20, 20);
            this.blue.TabIndex = 2;
            this.blue.TabStop = false;
            // 
            // red
            // 
            this.red.Image = ((System.Drawing.Image)(resources.GetObject("red.Image")));
            this.red.Location = new System.Drawing.Point(10, 55);
            this.red.Name = "red";
            this.red.Size = new System.Drawing.Size(20, 20);
            this.red.TabIndex = 3;
            this.red.TabStop = false;
            // 
            // blueDescription
            // 
            this.blueDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.blueDescription.Location = new System.Drawing.Point(37, 8);
            this.blueDescription.Name = "blueDescription";
            this.blueDescription.Size = new System.Drawing.Size(156, 13);
            this.blueDescription.TabIndex = 4;
            this.blueDescription.Text = "Blue - Fixed object / drivetrain";
            // 
            // greenDescription
            // 
            this.greenDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.greenDescription.Location = new System.Drawing.Point(37, 33);
            this.greenDescription.Name = "greenDescription";
            this.greenDescription.Size = new System.Drawing.Size(155, 13);
            this.greenDescription.TabIndex = 5;
            this.greenDescription.Text = "Green - Jointed object";
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(37, 58);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(155, 13);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "Red - Joint without driver";
            // 
            // DOFKey
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(204, 81);
            this.ControlBox = false;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.greenDescription);
            this.Controls.Add(this.blueDescription);
            this.Controls.Add(this.red);
            this.Controls.Add(this.blue);
            this.Controls.Add(this.green);
            this.Controls.Add(this.hookHTML);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(220, 120);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(220, 120);
            this.Name = "DOFKey";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Degrees of Freedom Key";
            ((System.ComponentModel.ISupportInitialize)(this.green)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.blue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.red)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser hookHTML;
        private System.Windows.Forms.PictureBox green;
        private System.Windows.Forms.PictureBox blue;
        private System.Windows.Forms.PictureBox red;
        private System.Windows.Forms.TextBox blueDescription;
        private System.Windows.Forms.TextBox greenDescription;
        private System.Windows.Forms.TextBox textBox1;
    }
}