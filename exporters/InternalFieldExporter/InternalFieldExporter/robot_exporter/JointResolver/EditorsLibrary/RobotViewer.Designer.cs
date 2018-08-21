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

        #region Form Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControl1 = new OpenTK.GLControl();
            this.labelDebugPosition = new System.Windows.Forms.Label();
            this.labelDebugRotation = new System.Windows.Forms.Label();
            this.labelDebugMode = new System.Windows.Forms.Label();
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
            // 
            // labelDebugPosition
            // 
            this.labelDebugPosition.AutoSize = true;
            this.labelDebugPosition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelDebugPosition.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDebugPosition.Location = new System.Drawing.Point(4, 4);
            this.labelDebugPosition.Name = "labelDebugPosition";
            this.labelDebugPosition.Size = new System.Drawing.Size(218, 20);
            this.labelDebugPosition.TabIndex = 1;
            this.labelDebugPosition.Text = "Camera position: <0, 0, 0>";
            // 
            // labelDebugRotation
            // 
            this.labelDebugRotation.AutoSize = true;
            this.labelDebugRotation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelDebugRotation.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDebugRotation.Location = new System.Drawing.Point(4, 34);
            this.labelDebugRotation.Name = "labelDebugRotation";
            this.labelDebugRotation.Size = new System.Drawing.Size(218, 20);
            this.labelDebugRotation.TabIndex = 2;
            this.labelDebugRotation.Text = "Camera rotation: <0, 0, 0>";
            // 
            // labelDebugMode
            // 
            this.labelDebugMode.AutoSize = true;
            this.labelDebugMode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelDebugMode.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDebugMode.Location = new System.Drawing.Point(4, 64);
            this.labelDebugMode.Name = "labelDebugMode";
            this.labelDebugMode.Size = new System.Drawing.Size(146, 20);
            this.labelDebugMode.TabIndex = 3;
            this.labelDebugMode.Text = "Camera mode: NONE";
            this.labelDebugMode.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // RobotViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.labelDebugMode);
            this.Controls.Add(this.labelDebugRotation);
            this.Controls.Add(this.labelDebugPosition);
            this.Controls.Add(this.glControl1);
            this.Name = "RobotViewer";
            this.Size = new System.Drawing.Size(768, 500);
            this.Resize += new System.EventHandler(this.RobotViewer_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private OpenTK.GLControl glControl1;
        private Label labelDebugPosition;
        private Label labelDebugRotation;
        private Label labelDebugMode;
    }
}
