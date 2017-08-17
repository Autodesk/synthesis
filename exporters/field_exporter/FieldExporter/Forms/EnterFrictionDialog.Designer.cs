namespace FieldExporter.Forms
{
    partial class EnterFrictionDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterFrictionDialog));
            this.frictionLabel = new System.Windows.Forms.Label();
            this.frictionNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.frictionNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // frictionLabel
            // 
            this.frictionLabel.AutoSize = true;
            this.frictionLabel.Location = new System.Drawing.Point(12, 14);
            this.frictionLabel.Name = "frictionLabel";
            this.frictionLabel.Size = new System.Drawing.Size(44, 13);
            this.frictionLabel.TabIndex = 0;
            this.frictionLabel.Text = "Friction:";
            // 
            // frictionNumericUpDown
            // 
            this.frictionNumericUpDown.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.frictionNumericUpDown.Location = new System.Drawing.Point(76, 12);
            this.frictionNumericUpDown.Name = "frictionNumericUpDown";
            this.frictionNumericUpDown.Size = new System.Drawing.Size(257, 20);
            this.frictionNumericUpDown.TabIndex = 1;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(135, 40);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(96, 32);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(237, 40);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(96, 32);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // EnterFrictionDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(345, 84);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.frictionNumericUpDown);
            this.Controls.Add(this.frictionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterFrictionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Friction";
            ((System.ComponentModel.ISupportInitialize)(this.frictionNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label frictionLabel;
        private System.Windows.Forms.NumericUpDown frictionNumericUpDown;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}