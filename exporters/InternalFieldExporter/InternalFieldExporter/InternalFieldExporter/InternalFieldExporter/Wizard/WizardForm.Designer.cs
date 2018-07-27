namespace InternalFieldExporter.Wizard
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
            this.WizardPages = new InternalFieldExporter.Wizard.WizardPageControl();
            this.ExportField_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // WizardPages
            // 
            this.WizardPages.BackColor = System.Drawing.Color.Transparent;
            this.WizardPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.WizardPages.Location = new System.Drawing.Point(0, 0);
            this.WizardPages.Margin = new System.Windows.Forms.Padding(5);
            this.WizardPages.Name = "WizardPages";
            this.WizardPages.Size = new System.Drawing.Size(643, 875);
            this.WizardPages.TabIndex = 0;
            // 
            // ExportField_Button
            // 
            this.ExportField_Button.Location = new System.Drawing.Point(436, 808);
            this.ExportField_Button.Name = "ExportField_Button";
            this.ExportField_Button.Size = new System.Drawing.Size(195, 32);
            this.ExportField_Button.TabIndex = 1;
            this.ExportField_Button.Text = "Export Field";
            this.ExportField_Button.UseVisualStyleBackColor = true;
            this.ExportField_Button.Click += new System.EventHandler(this.FieldExport_Button_clicked);
            // 
            // WizardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(643, 875);
            this.Controls.Add(this.ExportField_Button);
            this.Controls.Add(this.WizardPages);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(661, 12297);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(661, 912);
            this.Name = "WizardForm";
            this.Text = "Exporter Setup";
            this.ResumeLayout(false);

        }

        #endregion

        private WizardPageControl WizardPages;
        private System.Windows.Forms.Button ExportField_Button;
    }
}