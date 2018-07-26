namespace BxDRobotExporter.Wizard
{
    partial class WheelSlotPanel
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
            this.MainGroupBox = new System.Windows.Forms.GroupBox();
            this.EmptyLabel = new System.Windows.Forms.Label();
            this.MainGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainGroupBox
            // 
            this.MainGroupBox.Controls.Add(this.EmptyLabel);
            this.MainGroupBox.Location = new System.Drawing.Point(0, 0);
            this.MainGroupBox.Name = "MainGroupBox";
            this.MainGroupBox.Size = new System.Drawing.Size(220, 100);
            this.MainGroupBox.TabIndex = 0;
            this.MainGroupBox.TabStop = false;
            this.MainGroupBox.Text = "Undefined Node";
            // 
            // EmptyLabel
            // 
            this.EmptyLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.EmptyLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmptyLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.EmptyLabel.Location = new System.Drawing.Point(3, 16);
            this.EmptyLabel.Name = "EmptyLabel";
            this.EmptyLabel.Size = new System.Drawing.Size(214, 81);
            this.EmptyLabel.TabIndex = 0;
            this.EmptyLabel.Text = "Empty Wheel Slot";
            this.EmptyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EmptyWheelSlotPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MainGroupBox);
            this.Name = "EmptyWheelSlotPanel";
            this.Size = new System.Drawing.Size(220, 100);
            this.MainGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox MainGroupBox;
        private System.Windows.Forms.Label EmptyLabel;
    }
}
