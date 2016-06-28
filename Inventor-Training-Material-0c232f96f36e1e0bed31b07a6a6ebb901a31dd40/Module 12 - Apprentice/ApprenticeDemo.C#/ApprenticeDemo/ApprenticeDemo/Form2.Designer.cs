namespace ApprenticeDemo
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.axInventorViewControl1 = new AxInventorViewControlLib.AxInventorViewControl();
            ((System.ComponentModel.ISupportInitialize)(this.axInventorViewControl1)).BeginInit();
            this.SuspendLayout();
            // 
            // axInventorViewControl1
            // 
            this.axInventorViewControl1.Enabled = true;
            this.axInventorViewControl1.Location = new System.Drawing.Point(10, 15);
            this.axInventorViewControl1.Name = "axInventorViewControl1";
            this.axInventorViewControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axInventorViewControl1.OcxState")));
            this.axInventorViewControl1.Size = new System.Drawing.Size(455, 440);
            this.axInventorViewControl1.TabIndex = 0;
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 503);
            this.Controls.Add(this.axInventorViewControl1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axInventorViewControl1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AxInventorViewControlLib.AxInventorViewControl axInventorViewControl1;
    }
}