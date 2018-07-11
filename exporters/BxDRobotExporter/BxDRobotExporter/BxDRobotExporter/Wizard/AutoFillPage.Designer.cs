namespace BxDRobotExporter.Wizard
{
    partial class AutoFillPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoFillPage));
            this.label2 = new System.Windows.Forms.Label();
            this.WheelUpDown = new System.Windows.Forms.NumericUpDown();
            this.StartButton = new System.Windows.Forms.Button();
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.WheelUpDown)).BeginInit();
            this.MainLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 3);
            this.label2.Margin = new System.Windows.Forms.Padding(3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 20);
            this.label2.TabIndex = 1;
            this.label2.Text = "Number of Wheels";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // WheelUpDown
            // 
            this.WheelUpDown.Location = new System.Drawing.Point(104, 3);
            this.WheelUpDown.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.WheelUpDown.Name = "WheelUpDown";
            this.WheelUpDown.Size = new System.Drawing.Size(47, 20);
            this.WheelUpDown.TabIndex = 2;
            // 
            // StartButton
            // 
            this.MainLayout.SetColumnSpan(this.StartButton, 2);
            this.StartButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.StartButton.Location = new System.Drawing.Point(2, 28);
            this.StartButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(151, 23);
            this.StartButton.TabIndex = 3;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSize = true;
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MainLayout.Controls.Add(this.label2, 0, 0);
            this.MainLayout.Controls.Add(this.StartButton, 0, 1);
            this.MainLayout.Controls.Add(this.WheelUpDown, 1, 0);
            this.MainLayout.Dock = System.Windows.Forms.DockStyle.Top;
            this.MainLayout.Location = new System.Drawing.Point(0, 0);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 2;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MainLayout.Size = new System.Drawing.Size(155, 53);
            this.MainLayout.TabIndex = 4;
            // 
            // AutoFillPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(155, 56);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoFillPage";
            this.Text = "Auto Fill";
            ((System.ComponentModel.ISupportInitialize)(this.WheelUpDown)).EndInit();
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown WheelUpDown;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
    }
}