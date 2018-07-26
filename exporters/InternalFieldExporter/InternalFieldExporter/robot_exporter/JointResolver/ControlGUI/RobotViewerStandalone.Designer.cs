namespace JointResolver.ControlGUI
{
    partial class RobotViewerStandalone
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
            this.robotViewer1 = new EditorsLibrary.RobotViewer();
            this.glControl1 = new OpenTK.GLControl();
            this.SuspendLayout();
            // 
            // robotViewer1
            // 
            this.robotViewer1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.robotViewer1.Cursor = System.Windows.Forms.Cursors.Cross;
            this.robotViewer1.Location = new System.Drawing.Point(0, 0);
            this.robotViewer1.Margin = new System.Windows.Forms.Padding(0);
            this.robotViewer1.Name = "robotViewer1";
            this.robotViewer1.Size = new System.Drawing.Size(605, 431);
            this.robotViewer1.TabIndex = 0;
            // 
            // RobotViewerStandalone
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(606, 428);
            this.Controls.Add(this.robotViewer1);
            this.Name = "RobotViewerStandalone";
            this.Text = "RobotViewerStandalone";
            this.ResumeLayout(false);
            //
            // glControl1
            //
        }

        #endregion

        private EditorsLibrary.RobotViewer robotViewer1;
        private OpenTK.GLControl glControl1;
    }
}