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

            lblUpper = new System.Windows.Forms.Label();
            lblLower = new System.Windows.Forms.Label();
            this.txtUpper = new System.Windows.Forms.TextBox();
            this.txtLower = new System.Windows.Forms.TextBox();
            this.tabDOF = new System.Windows.Forms.TabControl();
            this.btnSave = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.SuspendLayout();
            this.Controls.Add(lblUpper);
            this.Controls.Add(lblLower);
            this.Controls.Add(this.txtUpper);
            this.Controls.Add(this.txtLower);
            this.Location = new System.Drawing.Point(4, 25);
       //     this.Name = "tabDOF_" + dof.GetHashCode();
            this.Size = new System.Drawing.Size(259, 77);
            this.TabIndex = 0;
       //     this.Text = label;
       //     this.UseVisualStyleBackColor = true;

            lblUpper.AutoSize = true;
            lblUpper.Location = new System.Drawing.Point(190, 15);
            lblUpper.Name = "lblUpper";
            lblUpper.Size = new System.Drawing.Size(50, 17);
            lblUpper.TabIndex = 4;
            lblUpper.Text = "Upper";

            lblLower.AutoSize = true;
            lblLower.Location = new System.Drawing.Point(60, 15);
            lblLower.Name = "lblLower";
            lblLower.Size = new System.Drawing.Size(50, 17);
            lblLower.TabIndex = 3;
            lblLower.Text = "Lower";

            this.txtUpper.Location = new System.Drawing.Point(150, 35);
            this.txtUpper.Name = "txtUpper";
            this.txtUpper.Size = new System.Drawing.Size(120, 22);
            this.txtUpper.TabIndex = 1;
            this.txtUpper.Text = "1";
            this.txtUpper.TextChanged += new System.EventHandler(UpperLimitChanged);

            this.txtLower.Location = new System.Drawing.Point(20, 35);
            this.txtLower.Name = "txtLower";
            this.txtLower.Size = new System.Drawing.Size(120, 22);
            this.txtLower.TabIndex = 0;
            this.txtLower.Text = "1";
            this.txtLower.TextChanged += new System.EventHandler(LowerLimitChanged);

            this.ResumeLayout(false);
            this.PerformLayout();
            // 
            // tabDOF
            // 
            this.tabDOF.Location = new System.Drawing.Point(13, 5);
            this.tabDOF.Name = "tabDOF";
            this.tabDOF.SelectedIndex = 0;
            this.tabDOF.Size = new System.Drawing.Size(267, 60);
            this.tabDOF.TabIndex = 0;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(21, 68);
            this.btnSave.Name = "btnOkay";
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
            this.ClientSize = new System.Drawing.Size(292, 102);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.tabDOF);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "EditLimits";
            this.Text = "EditLimits";
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.TabControl tabDOF;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtUpper;
        private System.Windows.Forms.TextBox txtLower;
        private System.Windows.Forms.Label lblUpper, lblLower;
        private class LimitPane<T> : System.Windows.Forms.TabPage
        {
           
            public LimitPane(T dof, string label)
            {/*
                this.dof = dof;
                if (!(dof is AngularDOF) && !(dof is LinearDOF))
                {
                    throw new System.InvalidOperationException("Bad DOF Type: " + dof.GetType().Name);
                }

                lblUpper = new System.Windows.Forms.Label();
                lblLower = new System.Windows.Forms.Label();
                this.chkHasLimits = new System.Windows.Forms.CheckBox();
                this.txtUpper = new System.Windows.Forms.TextBox();
                this.txtLower = new System.Windows.Forms.TextBox();

                this.SuspendLayout();
                this.Controls.Add(lblUpper);
                this.Controls.Add(lblLower);
                this.Controls.Add(this.chkHasLimits);
                this.Controls.Add(this.txtUpper);
                this.Controls.Add(this.txtLower);
                this.Location = new System.Drawing.Point(4, 25);
                this.Name = "tabDOF_" + dof.GetHashCode();
                this.Size = new System.Drawing.Size(259, 77);
                this.TabIndex = 0;
                this.Text = label;
                this.UseVisualStyleBackColor = true;

                lblUpper.AutoSize = true;
                lblUpper.Location = new System.Drawing.Point(128, 28);
                lblUpper.Name = "lblUpper";
                lblUpper.Size = new System.Drawing.Size(47, 17);
                lblUpper.TabIndex = 4;
                lblUpper.Text = "Upper";

                lblLower.AutoSize = true;
                lblLower.Location = new System.Drawing.Point(3, 28);
                lblLower.Name = "lblLower";
                lblLower.Size = new System.Drawing.Size(46, 17);
                lblLower.TabIndex = 3;
                lblLower.Text = "Lower";

                this.chkHasLimits.AutoSize = true;
                this.chkHasLimits.Location = new System.Drawing.Point(4, 4);
                this.chkHasLimits.Name = "chkHasLimits";
                this.chkHasLimits.Size = new System.Drawing.Size(95, 21);
                this.chkHasLimits.TabIndex = 2;
                this.chkHasLimits.Text = "Has Limits";
                this.chkHasLimits.UseVisualStyleBackColor = true;
                this.chkHasLimits.CheckStateChanged += changedProps;

                this.txtUpper.Location = new System.Drawing.Point(131, 48);
                this.txtUpper.Name = "txtUpper";
                this.txtUpper.Size = new System.Drawing.Size(125, 22);
                this.txtUpper.TabIndex = 1;
                this.txtUpper.TextChanged += changedProps;

                this.txtLower.Location = new System.Drawing.Point(4, 48);
                this.txtLower.Name = "txtLower";
                this.txtLower.Size = new System.Drawing.Size(121, 22);
                this.txtLower.TabIndex = 0;
                this.txtLower.TextChanged += changedProps;
                this.ResumeLayout(false);
                this.PerformLayout();

                loadProps();
            }

            void loadProps()
            {
                if (this.dof is AngularDOF)
                {
                    AngularDOF dof = (AngularDOF)this.dof;
                    chkHasLimits.Checked = dof.hasAngularLimits();
                    lblUpper.Visible = lblLower.Visible = txtUpper.Visible = txtLower.Visible = chkHasLimits.Checked;

                    txtLower.Text = Convert.ToString(cacheLower = dof.lowerLimit);
                    txtUpper.Text = Convert.ToString(cacheUpper = dof.upperLimit);
                    lblLower.Text = "Lower (rad)";
                    lblUpper.Text = "Upper (rad)";
                }
                else if (this.dof is LinearDOF)
                {
                    LinearDOF dof = (LinearDOF)this.dof;
                    chkHasLimits.Checked = dof.hasUpperLinearLimit() || dof.hasLowerLinearLimit();
                    lblUpper.Visible = lblLower.Visible = txtUpper.Visible = txtLower.Visible = chkHasLimits.Checked;

                    txtLower.Text = Convert.ToString(cacheLower = dof.lowerLimit);
                    txtUpper.Text = Convert.ToString(cacheUpper = dof.upperLimit);
                    lblLower.Text = "Lower (cm)";
                    lblUpper.Text = "Upper (cm)";
                }
            }

            public void resetProps()
            {
                if (this.dof is AngularDOF)
                {
                    AngularDOF dof = (AngularDOF)this.dof;
                    dof.lowerLimit = cacheLower;
                    dof.upperLimit = cacheUpper;
                }
                else if (this.dof is LinearDOF)
                {
                    LinearDOF dof = (LinearDOF)this.dof;
                    dof.lowerLimit = cacheLower;
                    dof.upperLimit = cacheUpper;
                }
            }
            void changedProps(object sender, System.EventArgs e)
            {
                changedProps(false);
            }

            public bool changedProps(bool report)
            {
                lblLower.Visible = txtLower.Visible = chkHasLimits.Checked;
                lblUpper.Visible = txtUpper.Visible = chkHasLimits.Checked;

                try
                {
                    if (this.dof is AngularDOF)
                    {
                        AngularDOF dof = (AngularDOF)this.dof;
                        dof.lowerLimit = chkHasLimits.Checked ? Convert.ToSingle(txtLower.Text) : float.NegativeInfinity;
                        dof.upperLimit = chkHasLimits.Checked ? Convert.ToSingle(txtUpper.Text) : float.PositiveInfinity;
                    }
                    else if (this.dof is LinearDOF)
                    {
                        LinearDOF dof = (LinearDOF)this.dof;
                        dof.lowerLimit = chkHasLimits.Checked ? Convert.ToSingle(txtLower.Text) : float.NegativeInfinity;
                        dof.upperLimit = chkHasLimits.Checked ? Convert.ToSingle(txtUpper.Text) : float.PositiveInfinity;
                    }
                    return true;
                }
                catch (FormatException)
                {
                    if (report)
                    {
                        System.Windows.Forms.MessageBox.Show("Invalid number format");
                    }
                    return false;
                }*/
            }
        }
    }
}