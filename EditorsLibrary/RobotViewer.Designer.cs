using System.Windows.Forms;

namespace EditorsLibrary
{
    partial class RobotViewer
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
            this.glControl1 = new OpenTK.GLControl();
            this.saveButton = new System.Windows.Forms.Button();
            this.openExisting = new System.Windows.Forms.Button();
            this.loadInventor = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.Black;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Location = new System.Drawing.Point(0, 0);
            this.glControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(768, 500);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = true;
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            // 
            // saveButton
            // 
            this.saveButton.Location = new System.Drawing.Point(680, 456);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(75, 29);
            this.saveButton.TabIndex = 1;
            this.saveButton.Text = "Save";
            this.saveButton.UseVisualStyleBackColor = true;
            // 
            // openExisting
            // 
            this.openExisting.Location = new System.Drawing.Point(158, 456);
            this.openExisting.Name = "openExisting";
            this.openExisting.Size = new System.Drawing.Size(112, 29);
            this.openExisting.TabIndex = 2;
            this.openExisting.Text = "Open Existing";
            this.openExisting.UseVisualStyleBackColor = true;
            // 
            // loadInventor
            // 
            this.loadInventor.Location = new System.Drawing.Point(14, 456);
            this.loadInventor.Name = "loadInventor";
            this.loadInventor.Size = new System.Drawing.Size(138, 29);
            this.loadInventor.TabIndex = 3;
            this.loadInventor.Text = "Load from Inventor";
            this.loadInventor.UseVisualStyleBackColor = true;
            // 
            // RobotViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.loadInventor);
            this.Controls.Add(this.openExisting);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.glControl1);
            this.Name = "RobotViewer";
            this.Size = new System.Drawing.Size(768, 500);
            this.Load += new System.EventHandler(this.glControl1_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl1_Paint);
            this.ResumeLayout(false);

        }

        #endregion

        private OpenTK.GLControl glControl1;
        public System.Windows.Forms.Button saveButton;
        public System.Windows.Forms.Button openExisting;
        public System.Windows.Forms.Button loadInventor;
    }
}
