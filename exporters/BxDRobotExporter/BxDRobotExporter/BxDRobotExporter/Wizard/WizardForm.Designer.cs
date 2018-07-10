namespace BxDRobotExporter.Wizard
{
    partial class WizardForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WizardForm));
            this.WizardPages = new BxDRobotExporter.Wizard.WizardPageControl();
            this.SuspendLayout();
            // 
            // WizardPages
            // 
            this.WizardPages.BackColor = System.Drawing.Color.Transparent;
            this.WizardPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WizardPages.Location = new System.Drawing.Point(0, 0);
            this.WizardPages.Name = "WizardPages";
            this.WizardPages.Size = new System.Drawing.Size(484, 711);
            this.WizardPages.TabIndex = 0;
            // 
            // WizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(484, 711);
            this.Controls.Add(this.WizardPages);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(500, 10000);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 750);
            this.Name = "WizardForm";
            this.Text = "Exporter Wizard";
            this.ResumeLayout(false);

        }

        #endregion

        private WizardPageControl WizardPages;
    }
}