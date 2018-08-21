namespace FieldExporter.Components
{
    partial class GamepiecePropertiesForm
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
            this.gamepieceGroupbox = new System.Windows.Forms.GroupBox();
            this.gamepieceLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.spawnpointLabel = new System.Windows.Forms.Label();
            this.selectSpawnpointButton = new System.Windows.Forms.Button();
            this.gamepieceCheckBox = new System.Windows.Forms.CheckBox();
            this.holdingLimitCheckBox = new System.Windows.Forms.CheckBox();
            this.holdingLimitUpDown = new System.Windows.Forms.NumericUpDown();
            this.gamepieceGroupbox.SuspendLayout();
            this.gamepieceLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.holdingLimitUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // gamepieceGroupbox
            // 
            this.gamepieceGroupbox.AutoSize = true;
            this.gamepieceGroupbox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gamepieceGroupbox.Controls.Add(this.gamepieceLayoutPanel);
            this.gamepieceGroupbox.Controls.Add(this.gamepieceCheckBox);
            this.gamepieceGroupbox.Dock = System.Windows.Forms.DockStyle.Top;
            this.gamepieceGroupbox.Location = new System.Drawing.Point(0, 0);
            this.gamepieceGroupbox.Name = "gamepieceGroupbox";
            this.gamepieceGroupbox.Size = new System.Drawing.Size(250, 76);
            this.gamepieceGroupbox.TabIndex = 0;
            this.gamepieceGroupbox.TabStop = false;
            // 
            // gamepieceLayoutPanel
            // 
            this.gamepieceLayoutPanel.AutoSize = true;
            this.gamepieceLayoutPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.gamepieceLayoutPanel.ColumnCount = 2;
            this.gamepieceLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.gamepieceLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.gamepieceLayoutPanel.Controls.Add(this.spawnpointLabel, 0, 0);
            this.gamepieceLayoutPanel.Controls.Add(this.selectSpawnpointButton, 1, 0);
            this.gamepieceLayoutPanel.Controls.Add(this.holdingLimitCheckBox, 0, 1);
            this.gamepieceLayoutPanel.Controls.Add(this.holdingLimitUpDown, 1, 1);
            this.gamepieceLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.gamepieceLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.gamepieceLayoutPanel.Name = "gamepieceLayoutPanel";
            this.gamepieceLayoutPanel.RowCount = 2;
            this.gamepieceLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gamepieceLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.gamepieceLayoutPanel.Size = new System.Drawing.Size(244, 57);
            this.gamepieceLayoutPanel.TabIndex = 1;
            // 
            // spawnpointLabel
            // 
            this.spawnpointLabel.AutoSize = true;
            this.spawnpointLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.spawnpointLabel.Location = new System.Drawing.Point(3, 3);
            this.spawnpointLabel.Margin = new System.Windows.Forms.Padding(3);
            this.spawnpointLabel.Name = "spawnpointLabel";
            this.spawnpointLabel.Size = new System.Drawing.Size(132, 23);
            this.spawnpointLabel.TabIndex = 1;
            this.spawnpointLabel.Text = "Spawnpoint: [0.0, 0.0, 0.0]";
            this.spawnpointLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // selectSpawnpointButton
            // 
            this.selectSpawnpointButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.selectSpawnpointButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.selectSpawnpointButton.Location = new System.Drawing.Point(179, 3);
            this.selectSpawnpointButton.Name = "selectSpawnpointButton";
            this.selectSpawnpointButton.Size = new System.Drawing.Size(62, 23);
            this.selectSpawnpointButton.TabIndex = 0;
            this.selectSpawnpointButton.Text = "Select";
            this.selectSpawnpointButton.UseVisualStyleBackColor = true;
            this.selectSpawnpointButton.Click += new System.EventHandler(this.selectSpawnpointButton_Click);
            // 
            // gamepieceCheckBox
            // 
            this.gamepieceCheckBox.AutoSize = true;
            this.gamepieceCheckBox.Checked = true;
            this.gamepieceCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.gamepieceCheckBox.Location = new System.Drawing.Point(6, 0);
            this.gamepieceCheckBox.Name = "gamepieceCheckBox";
            this.gamepieceCheckBox.Size = new System.Drawing.Size(80, 17);
            this.gamepieceCheckBox.TabIndex = 0;
            this.gamepieceCheckBox.Text = "Gamepiece";
            this.gamepieceCheckBox.UseVisualStyleBackColor = true;
            this.gamepieceCheckBox.CheckedChanged += new System.EventHandler(this.gamepieceCheckBox_CheckedChanged);
            // 
            // holdingLimitCheckBox
            // 
            this.holdingLimitCheckBox.AutoSize = true;
            this.holdingLimitCheckBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.holdingLimitCheckBox.Location = new System.Drawing.Point(3, 32);
            this.holdingLimitCheckBox.Name = "holdingLimitCheckBox";
            this.holdingLimitCheckBox.Size = new System.Drawing.Size(108, 22);
            this.holdingLimitCheckBox.TabIndex = 2;
            this.holdingLimitCheckBox.Text = "Limit number held";
            this.holdingLimitCheckBox.UseVisualStyleBackColor = true;
            this.holdingLimitCheckBox.CheckedChanged += new System.EventHandler(this.holdingLimitCheckBox_CheckedChanged);
            // 
            // holdingLimitUpDown
            // 
            this.holdingLimitUpDown.Enabled = false;
            this.holdingLimitUpDown.Location = new System.Drawing.Point(180, 33);
            this.holdingLimitUpDown.Margin = new System.Windows.Forms.Padding(4);
            this.holdingLimitUpDown.Name = "holdingLimitUpDown";
            this.holdingLimitUpDown.Size = new System.Drawing.Size(60, 20);
            this.holdingLimitUpDown.TabIndex = 3;
            // 
            // GamepiecePropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.gamepieceGroupbox);
            this.MinimumSize = new System.Drawing.Size(250, 0);
            this.Name = "GamepiecePropertiesForm";
            this.Size = new System.Drawing.Size(250, 76);
            this.gamepieceGroupbox.ResumeLayout(false);
            this.gamepieceGroupbox.PerformLayout();
            this.gamepieceLayoutPanel.ResumeLayout(false);
            this.gamepieceLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.holdingLimitUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gamepieceGroupbox;
        private System.Windows.Forms.CheckBox gamepieceCheckBox;
        private System.Windows.Forms.TableLayoutPanel gamepieceLayoutPanel;
        private System.Windows.Forms.Label spawnpointLabel;
        private System.Windows.Forms.Button selectSpawnpointButton;
        private System.Windows.Forms.CheckBox holdingLimitCheckBox;
        private System.Windows.Forms.NumericUpDown holdingLimitUpDown;
    }
}
