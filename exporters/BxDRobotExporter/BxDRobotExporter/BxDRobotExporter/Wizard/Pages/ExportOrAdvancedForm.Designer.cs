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
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.MainTextLbl = new System.Windows.Forms.Label();
            this.AdvancedExportButton = new System.Windows.Forms.Button();
            this.exportRobotButton = new System.Windows.Forms.Button();
            this.showAgainCheckBox = new System.Windows.Forms.CheckBox();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.MainLayout.Controls.Add(this.MainTextLbl, 0, 0);
            this.MainLayout.Controls.Add(this.AdvancedExportButton, 1, 0);
            this.MainLayout.Controls.Add(this.exportRobotButton, 1, 1);
            this.MainLayout.Controls.Add(this.showAgainCheckBox, 0, 2);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 3;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MainLayout.Size = new System.Drawing.Size(461, 185);
            this.MainLayout.TabIndex = 0;
            // 
            // MainTextLbl
            // 
            this.MainTextLbl.AutoSize = true;
            this.MainLayout.SetColumnSpan(this.MainTextLbl, 2);
            this.MainTextLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTextLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainTextLbl.Location = new System.Drawing.Point(0, 0);
            this.MainTextLbl.Margin = new System.Windows.Forms.Padding(0);
            this.MainTextLbl.Name = "MainTextLbl";
            this.MainTextLbl.Size = new System.Drawing.Size(461, 93);
            this.MainTextLbl.TabIndex = 0;
            this.MainTextLbl.Text = "You have now completed the initial setup for exporting your robot. Would you like" +
    " to configure advanced settings, or finish and export your robot to Synthesis?\r\n" +
    "";
            this.MainTextLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // AdvancedExportButton
            // 
            this.AdvancedExportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.AdvancedExportButton.Location = new System.Drawing.Point(3, 102);
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
            this.exportRobotButton.Location = new System.Drawing.Point(248, 102);
            this.exportRobotButton.Name = "exportRobotButton";
            this.exportRobotButton.Size = new System.Drawing.Size(210, 50);
            this.exportRobotButton.TabIndex = 2;
            this.exportRobotButton.Text = "Export Robot";
            this.exportRobotButton.UseVisualStyleBackColor = true;
            this.exportRobotButton.Click += new System.EventHandler(this.exportRobotButton_Click);
            // 
            // showAgainCheckBox
            // 
            this.showAgainCheckBox.AutoSize = true;
            this.showAgainCheckBox.Location = new System.Drawing.Point(3, 158);
            this.showAgainCheckBox.Name = "showAgainCheckBox";
            this.showAgainCheckBox.Size = new System.Drawing.Size(164, 21);
            this.showAgainCheckBox.TabIndex = 3;
            this.showAgainCheckBox.Text = "Don\'t show this again";
            this.showAgainCheckBox.UseVisualStyleBackColor = true;
            // 
            // ExportOrAdvancedForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(461, 185);
            this.Controls.Add(this.MainLayout);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportOrAdvancedForm";
            this.ShowIcon = false;
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.Button AdvancedExportButton;
        private System.Windows.Forms.Button exportRobotButton;
        private System.Windows.Forms.Label MainTextLbl;
        private System.Windows.Forms.CheckBox showAgainCheckBox;
    }
}