namespace StandaloneViewer
{
    partial class InventorViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventorViewerForm));
            robotViewer1 = new RobotViewer();
            this.SuspendLayout();
            //
            // robotViewer1
            //
            this.robotViewer1.Location = new System.Drawing.Point(0, 0);
            this.robotViewer1.Padding = new System.Windows.Forms.Padding(0);
            // 
            // StandaloneViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = LaunchParams.WindowSize;
            this.MaximumSize = LaunchParams.WindowSize;
            this.MinimumSize = LaunchParams.WindowSize;
            this.MaximizeBox = false;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Controls.Add(robotViewer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "StandaloneViewerForm";
            this.Text = "Robot Viewer: " + LaunchParams.Path.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries)[LaunchParams.Path.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries).Length - 1];
            this.ResumeLayout(false);

        }

        private RobotViewer robotViewer1;
        #endregion
    }
}