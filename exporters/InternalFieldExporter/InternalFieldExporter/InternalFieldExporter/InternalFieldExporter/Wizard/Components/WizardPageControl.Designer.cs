namespace InternalFieldExporter.Wizard
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
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.WizardNavigator = new InternalFieldExporter.Wizard.WizardNavigator();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 1;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Controls.Add(this.WizardNavigator, 0, 1);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 2;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.Size = new System.Drawing.Size(460, 687);
            this.MainLayout.TabIndex = 1;
            // 
            // WizardNavigator
            // 
            this.WizardNavigator.AutoSize = true;
            this.WizardNavigator.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.WizardNavigator.BackColor = System.Drawing.Color.Transparent;
            this.WizardNavigator.Dock = System.Windows.Forms.DockStyle.Top;
            this.WizardNavigator.Location = new System.Drawing.Point(3, 661);
            this.WizardNavigator.Name = "WizardNavigator";
            this.WizardNavigator.Size = new System.Drawing.Size(454, 23);
            this.WizardNavigator.TabIndex = 0;
            // 
            // WizardPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.MainLayout);
            this.Name = "WizardPageControl";
            this.Size = new System.Drawing.Size(460, 687);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        public WizardNavigator WizardNavigator;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
    }
}
