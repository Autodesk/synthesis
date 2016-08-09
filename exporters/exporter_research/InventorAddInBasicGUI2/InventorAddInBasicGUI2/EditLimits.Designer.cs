using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    partial class EditLimits
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
            this.lblUpper = new System.Windows.Forms.Label();
            this.lblLower = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.txtUpper = new System.Windows.Forms.TextBox();
            this.txtLower = new System.Windows.Forms.TextBox();
            this.tabDOF = new System.Windows.Forms.TabControl();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblUpper
            // 
            this.lblUpper.AutoSize = true;
            this.lblUpper.Location = new System.Drawing.Point(190, 35);
            this.lblUpper.Name = "lblUpper";
            this.lblUpper.Size = new System.Drawing.Size(47, 17);
            this.lblUpper.TabIndex = 4;
            this.lblUpper.Text = "Upper";
            // 
            // lblLower
            // 
            this.lblLower.AutoSize = true;
            this.lblLower.Location = new System.Drawing.Point(60, 35);
            this.lblLower.Name = "lblLower";
            this.lblLower.Size = new System.Drawing.Size(46, 17);
            this.lblLower.TabIndex = 3;
            this.lblLower.Text = "Lower";
            // 
            // txtUpper
            // 
            this.txtUpper.Location = new System.Drawing.Point(150, 60);
            this.txtUpper.Name = "txtUpper";
            this.txtUpper.Size = new System.Drawing.Size(120, 22);
            this.txtUpper.TabIndex = 1;
            this.txtUpper.Text = "1";
            this.txtUpper.TextChanged += new System.EventHandler(this.UpperLimitChanged);
            // 
            // txtLower
            // 
            this.txtLower.Location = new System.Drawing.Point(20, 60);
            this.txtLower.Name = "txtLower";
            this.txtLower.Size = new System.Drawing.Size(120, 22);
            this.txtLower.TabIndex = 0;
            this.txtLower.Text = "1";
            this.txtLower.TextChanged += new System.EventHandler(this.LowerLimitChanged);
            // 
            // tabDOF
            // 
            this.tabDOF.Location = new System.Drawing.Point(13, 5);
            this.tabDOF.Name = "tabDOF";
            this.tabDOF.SelectedIndex = 0;
            this.tabDOF.Size = new System.Drawing.Size(265, 87);
            this.tabDOF.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(123, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(47, 17);
            this.lblTitle.TabIndex = 5;
            this.lblTitle.Text = "Upper";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(20, 98);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(252, 26);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "OK";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.SaveButtonPressed);
            // 
            // EditLimits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 136);
            this.Controls.Add(this.lblUpper);
            this.Controls.Add(this.lblLower);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtUpper);
            this.Controls.Add(this.txtLower);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabDOF);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Location = new System.Drawing.Point(4, 25);
            this.Name = "EditLimits";
            this.Text = "Edit Limits";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.TabControl tabDOF;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtUpper;
        private System.Windows.Forms.TextBox txtLower;
        private System.Windows.Forms.Label lblUpper, lblLower, lblTitle;
        
    }
}