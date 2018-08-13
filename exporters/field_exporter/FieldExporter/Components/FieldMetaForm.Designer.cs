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
            this.selectCoordinatesButton = new System.Windows.Forms.Button();
            this.mainLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainLayoutPanel
            // 
            this.mainLayoutPanel.ColumnCount = 2;
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.mainLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayoutPanel.Controls.Add(this.spawnPointLabel, 0, 0);
            this.mainLayoutPanel.Controls.Add(this.selectCoordinatesButton, 1, 0);
            this.mainLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainLayoutPanel.Name = "mainLayoutPanel";
            this.mainLayoutPanel.RowCount = 2;
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.mainLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
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
            this.spawnPointLabel.Size = new System.Drawing.Size(107, 23);
            this.spawnPointLabel.TabIndex = 0;
            this.spawnPointLabel.Text = "Robot Spawn Points:";
            this.spawnPointLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // selectCoordinatesButton
            // 
            this.selectCoordinatesButton.AutoSize = true;
            this.selectCoordinatesButton.Dock = System.Windows.Forms.DockStyle.Top;
            this.selectCoordinatesButton.Location = new System.Drawing.Point(116, 3);
            this.selectCoordinatesButton.Name = "selectCoordinatesButton";
            this.selectCoordinatesButton.Size = new System.Drawing.Size(332, 23);
            this.selectCoordinatesButton.TabIndex = 1;
            this.selectCoordinatesButton.Text = "Select Points";
            this.selectCoordinatesButton.UseVisualStyleBackColor = true;
            this.selectCoordinatesButton.Click += new System.EventHandler(this.selectCoordinatesButton_Click);
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
    }
}
