namespace BxDRobotExporter.Wizard
{
    partial class WizardPageControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.WizardNavigator = new BxDRobotExporter.Wizard.WizardNavigator();
            this.SuspendLayout();
            // 
            // WizardNavigator
            // 
            this.WizardNavigator.BackColor = System.Drawing.Color.Transparent;
            this.WizardNavigator.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.WizardNavigator.Location = new System.Drawing.Point(0, 659);
            this.WizardNavigator.Name = "WizardNavigator";
            this.WizardNavigator.Size = new System.Drawing.Size(460, 28);
            this.WizardNavigator.TabIndex = 0;
            // 
            // WizardPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.WizardNavigator);
            this.Name = "WizardPageControl";
            this.Size = new System.Drawing.Size(460, 687);
            this.ResumeLayout(false);

        }

        #endregion

        public WizardNavigator WizardNavigator;
    }
}
