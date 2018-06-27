namespace EditorsLibrary
{
    partial class SetMassForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetMassForm));
            this.MassBox = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.MassBox)).BeginInit();
            this.SuspendLayout();
            // 
            // MassBox
            // 
            this.MassBox.Location = new System.Drawing.Point(67, 49);
            this.MassBox.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.MassBox.Name = "MassBox";
            this.MassBox.Size = new System.Drawing.Size(120, 20);
            this.MassBox.TabIndex = 0;
            this.MassBox.ValueChanged += new System.EventHandler(this.MassBox_ValueChanged);
            // 
            // SetMassForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(261, 126);
            this.Controls.Add(this.MassBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SetMassForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Set Mass";
            ((System.ComponentModel.ISupportInitialize)(this.MassBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NumericUpDown MassBox;
    }
}