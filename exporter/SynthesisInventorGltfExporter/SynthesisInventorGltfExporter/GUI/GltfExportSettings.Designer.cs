namespace SynthesisInventorGltfExporter.GUI
{
    partial class GltfExportSettings
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GltfExportSettings));
            this.MainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.includeSynth = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkMaterials = new System.Windows.Forms.CheckBox();
            this.checkFace = new System.Windows.Forms.CheckBox();
            this.checkHidden = new System.Windows.Forms.CheckBox();
            this.ChildLabel = new System.Windows.Forms.Label();
            this.comboFileType = new System.Windows.Forms.ComboBox();
            this.numericTolerance = new System.Windows.Forms.NumericUpDown();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.MainLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize) (this.numericTolerance)).BeginInit();
            this.SuspendLayout();
            // 
            // MainLayout
            // 
            this.MainLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.MainLayout.ColumnCount = 2;
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 183F));
            this.MainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 151F));
            this.MainLayout.Controls.Add(this.includeSynth, 0, 4);
            this.MainLayout.Controls.Add(this.label1, 0, 5);
            this.MainLayout.Controls.Add(this.checkMaterials, 0, 0);
            this.MainLayout.Controls.Add(this.checkFace, 0, 1);
            this.MainLayout.Controls.Add(this.checkHidden, 0, 2);
            this.MainLayout.Controls.Add(this.ChildLabel, 0, 3);
            this.MainLayout.Controls.Add(this.comboFileType, 1, 5);
            this.MainLayout.Controls.Add(this.numericTolerance, 1, 3);
            this.MainLayout.Location = new System.Drawing.Point(5, 7);
            this.MainLayout.Margin = new System.Windows.Forms.Padding(4);
            this.MainLayout.Name = "MainLayout";
            this.MainLayout.RowCount = 6;
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 49F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.MainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MainLayout.Size = new System.Drawing.Size(334, 198);
            this.MainLayout.TabIndex = 14;
            // 
            // includeSynth
            // 
            this.includeSynth.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.MainLayout.SetColumnSpan(this.includeSynth, 2);
            this.includeSynth.Dock = System.Windows.Forms.DockStyle.Left;
            this.includeSynth.Location = new System.Drawing.Point(4, 137);
            this.includeSynth.Margin = new System.Windows.Forms.Padding(4);
            this.includeSynth.Name = "includeSynth";
            this.includeSynth.Size = new System.Drawing.Size(311, 23);
            this.includeSynth.TabIndex = 20;
            this.includeSynth.Text = "Include Synthesis Data: ";
            this.includeSynth.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(4, 168);
            this.label1.Margin = new System.Windows.Forms.Padding(4);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 26);
            this.label1.TabIndex = 19;
            this.label1.Text = "glTF File Type:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkMaterials
            // 
            this.checkMaterials.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.MainLayout.SetColumnSpan(this.checkMaterials, 2);
            this.checkMaterials.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkMaterials.Location = new System.Drawing.Point(4, 4);
            this.checkMaterials.Margin = new System.Windows.Forms.Padding(4);
            this.checkMaterials.Name = "checkMaterials";
            this.checkMaterials.Size = new System.Drawing.Size(311, 18);
            this.checkMaterials.TabIndex = 16;
            this.checkMaterials.Text = "Export Appearances:";
            this.checkMaterials.UseVisualStyleBackColor = true;
            this.checkMaterials.CheckedChanged += new System.EventHandler(this.checkMaterials_CheckedChanged);
            // 
            // checkFace
            // 
            this.checkFace.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.MainLayout.SetColumnSpan(this.checkFace, 2);
            this.checkFace.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkFace.Location = new System.Drawing.Point(4, 30);
            this.checkFace.Margin = new System.Windows.Forms.Padding(4);
            this.checkFace.Name = "checkFace";
            this.checkFace.Size = new System.Drawing.Size(311, 20);
            this.checkFace.TabIndex = 15;
            this.checkFace.Text = "Export Face Appearances:";
            this.checkFace.UseVisualStyleBackColor = true;
            // 
            // checkHidden
            // 
            this.checkHidden.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.MainLayout.SetColumnSpan(this.checkHidden, 2);
            this.checkHidden.Dock = System.Windows.Forms.DockStyle.Left;
            this.checkHidden.Location = new System.Drawing.Point(4, 58);
            this.checkHidden.Margin = new System.Windows.Forms.Padding(4);
            this.checkHidden.Name = "checkHidden";
            this.checkHidden.Size = new System.Drawing.Size(311, 22);
            this.checkHidden.TabIndex = 14;
            this.checkHidden.Text = "Export Hidden Components:";
            this.checkHidden.UseVisualStyleBackColor = true;
            // 
            // ChildLabel
            // 
            this.ChildLabel.AutoSize = true;
            this.ChildLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.ChildLabel.ForeColor = System.Drawing.Color.Black;
            this.ChildLabel.Location = new System.Drawing.Point(4, 88);
            this.ChildLabel.Margin = new System.Windows.Forms.Padding(4);
            this.ChildLabel.Name = "ChildLabel";
            this.ChildLabel.Size = new System.Drawing.Size(157, 41);
            this.ChildLabel.TabIndex = 6;
            this.ChildLabel.Text = "Mesh Tolerance (cm):\r\n(lower -> higher quality)\r\n";
            this.ChildLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboFileType
            // 
            this.comboFileType.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.comboFileType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboFileType.FormattingEnabled = true;
            this.comboFileType.Items.AddRange(new object[] {"Binary (.glb)", "JSON (.gltf)"});
            this.comboFileType.Location = new System.Drawing.Point(186, 168);
            this.comboFileType.Name = "comboFileType";
            this.comboFileType.Size = new System.Drawing.Size(145, 24);
            this.comboFileType.TabIndex = 18;
            // 
            // numericTolerance
            // 
            this.numericTolerance.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.numericTolerance.DecimalPlaces = 2;
            this.numericTolerance.Increment = new decimal(new int[] {1, 0, 0, 65536});
            this.numericTolerance.Location = new System.Drawing.Point(222, 97);
            this.numericTolerance.Margin = new System.Windows.Forms.Padding(3, 3, 20, 3);
            this.numericTolerance.Maximum = new decimal(new int[] {1000, 0, 0, 0});
            this.numericTolerance.Minimum = new decimal(new int[] {1, 0, 0, 131072});
            this.numericTolerance.Name = "numericTolerance";
            this.numericTolerance.Size = new System.Drawing.Size(92, 22);
            this.numericTolerance.TabIndex = 17;
            this.numericTolerance.Value = new decimal(new int[] {1, 0, 0, 0});
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(218, 238);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(121, 27);
            this.cancelButton.TabIndex = 17;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(91, 239);
            this.okButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(121, 27);
            this.okButton.TabIndex = 16;
            this.okButton.Text = "Export";
            this.okButton.UseVisualStyleBackColor = false;
            // 
            // GltfExportSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(347, 277);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.MainLayout);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon) (resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GltfExportSettings";
            this.Text = "glTF Export Settings";
            this.MainLayout.ResumeLayout(false);
            this.MainLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize) (this.numericTolerance)).EndInit();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox checkFace;
        private System.Windows.Forms.CheckBox checkHidden;
        private System.Windows.Forms.CheckBox checkMaterials;
        private System.Windows.Forms.Label ChildLabel;
        private System.Windows.Forms.ComboBox comboFileType;
        private System.Windows.Forms.CheckBox includeSynth;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel MainLayout;
        private System.Windows.Forms.NumericUpDown numericTolerance;
        private System.Windows.Forms.Button okButton;

        #endregion
    }
}