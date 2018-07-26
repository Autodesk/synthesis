namespace BxDRobotExporter.Wizard
{
    partial class ExportOrAdvancedForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.MainTextLbl = new System.Windows.Forms.Label();
            this.AdvancedExportButton = new System.Windows.Forms.Button();
            this.exportRobotButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.MainTextLbl, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.AdvancedExportButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.exportRobotButton, 1, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(446, 206);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // MainTextLbl
            // 
            this.MainTextLbl.AutoSize = true;
            this.tableLayoutPanel1.SetColumnSpan(this.MainTextLbl, 2);
            this.MainTextLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainTextLbl.Location = new System.Drawing.Point(12, 8);
            this.MainTextLbl.Margin = new System.Windows.Forms.Padding(12, 8, 8, 8);
            this.MainTextLbl.Name = "MainTextLbl";
            this.MainTextLbl.Size = new System.Drawing.Size(425, 60);
            this.MainTextLbl.TabIndex = 0;
            this.MainTextLbl.Text = "You now completed the initial setup for exporting your robot. Would you like to c" +
    "ontinue to configure advanced settings or finish and export your robot to Synthe" +
    "sis?\r\n";
            // 
            // AdvancedExportButton
            // 
            this.AdvancedExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AdvancedExportButton.Location = new System.Drawing.Point(3, 153);
            this.AdvancedExportButton.Name = "AdvancedExportButton";
            this.AdvancedExportButton.Size = new System.Drawing.Size(210, 50);
            this.AdvancedExportButton.TabIndex = 1;
            this.AdvancedExportButton.Text = "Configure Advanced  Settings";
            this.AdvancedExportButton.UseVisualStyleBackColor = true;
            this.AdvancedExportButton.Click += new System.EventHandler(this.AdvancedExportButton_Click);
            // 
            // exportRobotButton
            // 
            this.exportRobotButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportRobotButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.exportRobotButton.Location = new System.Drawing.Point(233, 153);
            this.exportRobotButton.Name = "exportRobotButton";
            this.exportRobotButton.Size = new System.Drawing.Size(210, 50);
            this.exportRobotButton.TabIndex = 2;
            this.exportRobotButton.Text = "Export Robot";
            this.exportRobotButton.UseVisualStyleBackColor = true;
            this.exportRobotButton.Click += new System.EventHandler(this.exportRobotButton_Click);
            // 
            // ExportOrAdvancedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 230);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ExportOrAdvancedForm";
            this.ShowIcon = false;
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button AdvancedExportButton;
        private System.Windows.Forms.Button exportRobotButton;
        private System.Windows.Forms.Label MainTextLbl;
    }
}