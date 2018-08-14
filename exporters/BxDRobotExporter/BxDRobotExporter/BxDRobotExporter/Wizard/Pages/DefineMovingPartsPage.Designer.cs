namespace SynthesisRobotExporter.Wizard
{
    partial class DefineMovingPartsPage
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
            this.Step3InfoLabel = new System.Windows.Forms.Label();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DefinePartsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // Step3InfoLabel
            // 
            this.Step3InfoLabel.AutoSize = true;
            this.Step3InfoLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.Step3InfoLabel.Location = new System.Drawing.Point(4, 4);
            this.Step3InfoLabel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Step3InfoLabel.Name = "Step3InfoLabel";
            this.Step3InfoLabel.Size = new System.Drawing.Size(605, 17);
            this.Step3InfoLabel.TabIndex = 1;
            this.Step3InfoLabel.Text = "Define additional moving parts, any joints left undefined will not be bound to a " +
    "control port.";
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 1;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Controls.Add(this.Step3InfoLabel, 0, 0);
            this.MainLayout.Controls.Add(this.DefinePartsLayout, 0, 1);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 2;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Size = new System.Drawing.Size(613, 804);
            this.MainLayout.TabIndex = 3;
            // 
            // DefinePartsLayout
            // 
            this.DefinePartsLayout.AutoScroll = true;
            this.DefinePartsLayout.ColumnCount = 2;
            this.DefinePartsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.DefinePartsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 27F));
            this.DefinePartsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefinePartsLayout.Location = new System.Drawing.Point(4, 29);
            this.DefinePartsLayout.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DefinePartsLayout.Name = "DefinePartsLayout";
            this.DefinePartsLayout.RowCount = 1;
            this.DefinePartsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.DefinePartsLayout.Size = new System.Drawing.Size(605, 771);
            this.DefinePartsLayout.TabIndex = 2;
            // 
            // DefineMovingPartsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainLayout);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "DefineMovingPartsPage";
            this.Size = new System.Drawing.Size(613, 804);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label Step3InfoLabel;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.TableLayoutPanel DefinePartsLayout;
    }
}
