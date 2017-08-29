namespace BxDRobotExporter.Wizard
{
    partial class StartPage
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
            this.WelcomeMessageLabel = new System.Windows.Forms.Label();
            this.TutorialsLinkLabel = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // WelcomeMessageLabel
            // 
            this.WelcomeMessageLabel.Location = new System.Drawing.Point(15, 570);
            this.WelcomeMessageLabel.Name = "WelcomeMessageLabel";
            this.WelcomeMessageLabel.Size = new System.Drawing.Size(427, 26);
            this.WelcomeMessageLabel.TabIndex = 0;
            this.WelcomeMessageLabel.Text = "Welcome to the Robot Exporter! This wizard will guide you through the process of " +
    "setting joint properties and defining wheels.  \r\n";
            // 
            // TutorialsLinkLabel
            // 
            this.TutorialsLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TutorialsLinkLabel.AutoSize = true;
            this.TutorialsLinkLabel.BackColor = System.Drawing.Color.Transparent;
            this.TutorialsLinkLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.TutorialsLinkLabel.Location = new System.Drawing.Point(109, 606);
            this.TutorialsLinkLabel.Name = "TutorialsLinkLabel";
            this.TutorialsLinkLabel.Size = new System.Drawing.Size(157, 13);
            this.TutorialsLinkLabel.TabIndex = 1;
            this.TutorialsLinkLabel.TabStop = true;
            this.TutorialsLinkLabel.Text = "bxd.autodesk.com/tutorials.html";
            this.TutorialsLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TutorialsLinkLabel_LinkClicked);
            // 
            // LowerLimitLabel
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 606);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "For tutorials, visit:";
            // 
            // StartPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TutorialsLinkLabel);
            this.Controls.Add(this.WelcomeMessageLabel);
            this.Name = "StartPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label WelcomeMessageLabel;
        private System.Windows.Forms.LinkLabel TutorialsLinkLabel;
        private System.Windows.Forms.Label label1;
    }
}
