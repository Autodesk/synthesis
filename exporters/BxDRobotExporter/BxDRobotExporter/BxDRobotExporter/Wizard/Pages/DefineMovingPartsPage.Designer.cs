namespace BxDRobotExporter.Wizard
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
            this.DefineMovingPartsLabel = new System.Windows.Forms.Label();
            this.Step3InfoLabel = new System.Windows.Forms.Label();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DefinePartsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // DefineMovingPartsLabel
            // 
            this.DefineMovingPartsLabel.AutoSize = true;
            this.DefineMovingPartsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DefineMovingPartsLabel.Location = new System.Drawing.Point(-4, 0);
            this.DefineMovingPartsLabel.Name = "DefineMovingPartsLabel";
            this.DefineMovingPartsLabel.Size = new System.Drawing.Size(283, 20);
            this.DefineMovingPartsLabel.TabIndex = 0;
            this.DefineMovingPartsLabel.Text = "Step 3: Define Other Moving Parts";
            // 
            // Step3InfoLabel
            // 
            this.Step3InfoLabel.AutoSize = true;
            this.Step3InfoLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.Step3InfoLabel.Location = new System.Drawing.Point(3, 3);
            this.Step3InfoLabel.Margin = new System.Windows.Forms.Padding(3);
            this.Step3InfoLabel.Name = "Step3InfoLabel";
            this.Step3InfoLabel.Size = new System.Drawing.Size(454, 26);
            this.Step3InfoLabel.TabIndex = 1;
            this.Step3InfoLabel.Text = "If you have any other moving parts on your robot that you believe would be useful" +
    " to have in the simulation, set them up here by checking any of the nodes on the" +
    " list.\r\n";
            // 
            // MainLayout
            // 
            this.MainLayout.ColumnCount = 1;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Controls.Add(this.Step3InfoLabel, 0, 0);
            this.MainLayout.Controls.Add(this.DefinePartsLayout, 0, 1);
            this.MainLayout.Location = new System.Drawing.Point(0, 23);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 2;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.MainLayout.Size = new System.Drawing.Size(460, 630);
            this.MainLayout.TabIndex = 3;
            // 
            // DefinePartsLayout
            // 
            this.DefinePartsLayout.AutoScroll = true;
            this.DefinePartsLayout.ColumnCount = 1;
            this.DefinePartsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.DefinePartsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefinePartsLayout.Location = new System.Drawing.Point(3, 35);
            this.DefinePartsLayout.Name = "DefinePartsLayout";
            this.DefinePartsLayout.RowCount = 1;
            this.DefinePartsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.DefinePartsLayout.Size = new System.Drawing.Size(454, 592);
            this.DefinePartsLayout.TabIndex = 2;
            // 
            // DefineMovingPartsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainLayout);
            this.Controls.Add(this.DefineMovingPartsLabel);
            this.Name = "DefineMovingPartsPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineMovingPartsLabel;
        private System.Windows.Forms.Label Step3InfoLabel;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.TableLayoutPanel DefinePartsLayout;
    }
}
