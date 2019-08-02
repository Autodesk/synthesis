namespace InventorRobotExporter.GUI.Editors.JointEditor
{
    partial class JointForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JointForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.DefinePartsLayout = new System.Windows.Forms.TableLayoutPanel();
            this.DefinePartsLayout.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(23, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(0, 0);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // DefinePartsLayout
            // 
            this.DefinePartsLayout.AutoScroll = true;
            this.DefinePartsLayout.AutoSize = true;
            this.DefinePartsLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.DefinePartsLayout.BackColor = System.Drawing.SystemColors.ControlLight;
            this.DefinePartsLayout.ColumnCount = 1;
            this.DefinePartsLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.DefinePartsLayout.Controls.Add(this.tableLayoutPanel1, 0, 0);
            this.DefinePartsLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DefinePartsLayout.Location = new System.Drawing.Point(0, 0);
            this.DefinePartsLayout.Margin = new System.Windows.Forms.Padding(0);
            this.DefinePartsLayout.Name = "DefinePartsLayout";
            this.DefinePartsLayout.Padding = new System.Windows.Forms.Padding(20, 16, 43, 20);
            this.DefinePartsLayout.RowCount = 1;
            this.DefinePartsLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.DefinePartsLayout.Size = new System.Drawing.Size(682, 803);
            this.DefinePartsLayout.TabIndex = 3;
            // 
            // JointForm
            // 
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(682, 803);
            this.Controls.Add(this.DefinePartsLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1000, 850);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(0, 850);
            this.Name = "JointForm";
            this.Text = "Joint Editor";
            this.TopMost = true;
            this.DefinePartsLayout.ResumeLayout(false);
            this.DefinePartsLayout.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel DefinePartsLayout;
    }
}