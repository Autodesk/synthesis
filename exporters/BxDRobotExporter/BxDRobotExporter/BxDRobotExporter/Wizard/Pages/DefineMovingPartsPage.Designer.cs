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
            this.DefinePartsPanelLayout = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
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
            // DefinePartsPanelLayout
            // 
            this.DefinePartsPanelLayout.AutoScroll = true;
            this.DefinePartsPanelLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefinePartsPanelLayout.Location = new System.Drawing.Point(3, 35);
            this.DefinePartsPanelLayout.Name = "DefinePartsPanelLayout";
            this.DefinePartsPanelLayout.Size = new System.Drawing.Size(454, 592);
            this.DefinePartsPanelLayout.TabIndex = 2;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.Step3InfoLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.DefinePartsPanelLayout, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 23);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(460, 630);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // DefineMovingPartsPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.DefineMovingPartsLabel);
            this.Name = "DefineMovingPartsPage";
            this.Size = new System.Drawing.Size(460, 653);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DefineMovingPartsLabel;
        private System.Windows.Forms.Label Step3InfoLabel;
        private System.Windows.Forms.FlowLayoutPanel DefinePartsPanelLayout;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
