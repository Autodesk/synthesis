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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.axInventorViewControl1 = new AxInventorViewControlLib.AxInventorViewControl();
            this.btnOpen = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.axInventorViewControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // axInventorViewControl1
            // 
            this.axInventorViewControl1.Enabled = true;
            this.axInventorViewControl1.Location = new System.Drawing.Point(24, 12);
            this.axInventorViewControl1.Name = "axInventorViewControl1";
            this.axInventorViewControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axInventorViewControl1.OcxState")));
            this.axInventorViewControl1.Size = new System.Drawing.Size(465, 375);
            this.axInventorViewControl1.TabIndex = 0;
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(164, 420);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(181, 44);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open File";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 476);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.axInventorViewControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.axInventorViewControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxInventorViewControlLib.AxInventorViewControl axInventorViewControl1;
        private System.Windows.Forms.Button btnOpen;
    }
}

