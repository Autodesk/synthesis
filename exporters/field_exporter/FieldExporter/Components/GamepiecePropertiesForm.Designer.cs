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
            this.gamepieceCheckBox = new System.Windows.Forms.CheckBox();
            this.gamepieceGroupbox.SuspendLayout();
            this.SuspendLayout();
            // 
            // gamepieceGroupbox
            // 
            this.gamepieceGroupbox.Controls.Add(this.gamepieceCheckBox);
            this.gamepieceGroupbox.Dock = System.Windows.Forms.DockStyle.Top;
            this.gamepieceGroupbox.Location = new System.Drawing.Point(0, 0);
            this.gamepieceGroupbox.Name = "gamepieceGroupbox";
            this.gamepieceGroupbox.Size = new System.Drawing.Size(250, 100);
            this.gamepieceGroupbox.TabIndex = 0;
            this.gamepieceGroupbox.TabStop = false;
            // 
            // gamepieceCheckBox
            // 
            this.gamepieceCheckBox.AutoSize = true;
            this.gamepieceCheckBox.Location = new System.Drawing.Point(6, 0);
            this.gamepieceCheckBox.Name = "gamepieceCheckBox";
            this.gamepieceCheckBox.Size = new System.Drawing.Size(80, 17);
            this.gamepieceCheckBox.TabIndex = 0;
            this.gamepieceCheckBox.Text = "Gamepiece";
            this.gamepieceCheckBox.UseVisualStyleBackColor = true;
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
            this.Size = new System.Drawing.Size(250, 100);
            this.gamepieceGroupbox.ResumeLayout(false);
            this.gamepieceGroupbox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gamepieceGroupbox;
        private System.Windows.Forms.CheckBox gamepieceCheckBox;
    }
}
