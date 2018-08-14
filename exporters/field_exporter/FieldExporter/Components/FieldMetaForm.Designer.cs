namespace FieldExporter.Components
{
    partial class FieldMetaForm
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
            this.mainLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.spawnPointLabel = new System.Windows.Forms.Label();
            this.spawnPointList = new System.Windows.Forms.ListBox();
            this.selectCoordinatesButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 2;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.Controls.Add(this.spawnPointLabel, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.spawnPointList, 0, 1);
            this.mainLayoutPanel.Controls.Add(this.selectCoordinatesButton, 0, 2);
            this.mainLayoutPanel.Controls.Add(this.panel1, 1, 0);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 3;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayoutPanel.Size = new System.Drawing.Size(451, 241);
            this.mainLayoutPanel.TabIndex = 0;
            // 
            // spawnPointLabel
            // 
            this.spawnPointLabel.AutoSize = true;
            this.spawnPointLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.spawnPointLabel.Location = new System.Drawing.Point(3, 3);
            this.spawnPointLabel.Margin = new System.Windows.Forms.Padding(3);
            this.spawnPointLabel.Name = "spawnPointLabel";
            this.spawnPointLabel.Size = new System.Drawing.Size(107, 13);
            this.spawnPointLabel.TabIndex = 0;
            this.spawnPointLabel.Text = "Robot Spawn Points:";
            this.spawnPointLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // spawnPointList
            // 
            this.spawnPointList.Dock = System.Windows.Forms.DockStyle.Left;
            this.spawnPointList.FormattingEnabled = true;
            this.spawnPointList.Location = new System.Drawing.Point(3, 22);
            this.spawnPointList.Name = "spawnPointList";
            this.spawnPointList.Size = new System.Drawing.Size(166, 187);
            this.spawnPointList.TabIndex = 2;
            // 
            // selectCoordinatesButton
            // 
            this.selectCoordinatesButton.AutoSize = true;
            this.selectCoordinatesButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.selectCoordinatesButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.selectCoordinatesButton.Location = new System.Drawing.Point(3, 215);
            this.selectCoordinatesButton.Name = "selectCoordinatesButton";
            this.selectCoordinatesButton.Size = new System.Drawing.Size(166, 23);
            this.selectCoordinatesButton.TabIndex = 1;
            this.selectCoordinatesButton.Text = "Select Points";
            this.selectCoordinatesButton.UseVisualStyleBackColor = true;
            this.selectCoordinatesButton.Click += new System.EventHandler(this.selectCoordinatesButton_Click);
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(175, 3);
            this.panel1.Name = "panel1";
            this.mainLayoutPanel.SetRowSpan(this.panel1, 3);
            this.panel1.Size = new System.Drawing.Size(273, 235);
            this.panel1.TabIndex = 3;
            // 
            // FieldMetaForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainLayoutPanel);
            this.Name = "FieldMetaForm";
            this.Size = new System.Drawing.Size(451, 241);
            this.mainLayoutPanel.ResumeLayout(false);
            this.mainLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainLayoutPanel;
        private System.Windows.Forms.Label spawnPointLabel;
        private System.Windows.Forms.Button selectCoordinatesButton;
        private System.Windows.Forms.ListBox spawnPointList;
        private System.Windows.Forms.Panel panel1;
    }
}
