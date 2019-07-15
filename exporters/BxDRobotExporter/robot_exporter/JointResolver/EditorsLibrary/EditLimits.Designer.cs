using System;
namespace EditorsLibrary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditLimits));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOkay = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.Angular_Group_Box = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Angular_Start_textbox = new System.Windows.Forms.NumericUpDown();
            this.Angular_Current_textbox = new System.Windows.Forms.NumericUpDown();
            this.Angular_End_textbox = new System.Windows.Forms.NumericUpDown();
            this.Angular_End = new System.Windows.Forms.CheckBox();
            this.Angular_Current = new System.Windows.Forms.Label();
            this.Angular_Start = new System.Windows.Forms.CheckBox();
            this.LinearGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.Linear_Start_textbox = new System.Windows.Forms.NumericUpDown();
            this.Linear_Current_textbox = new System.Windows.Forms.NumericUpDown();
            this.Linear_End = new System.Windows.Forms.CheckBox();
            this.Linear_Current = new System.Windows.Forms.Label();
            this.Linear_Start = new System.Windows.Forms.CheckBox();
            this.Linear_End_textbox = new System.Windows.Forms.NumericUpDown();
            this.AnimateJointButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.Angular_Group_Box.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Angular_Start_textbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Angular_Current_textbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Angular_End_textbox)).BeginInit();
            this.LinearGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Linear_Start_textbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Linear_Current_textbox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Linear_End_textbox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(175, 199);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(91, 22);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOkay
            // 
            this.btnOkay.Location = new System.Drawing.Point(77, 200);
            this.btnOkay.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(93, 21);
            this.btnOkay.TabIndex = 0;
            this.btnOkay.Text = "OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            this.btnOkay.Click += new System.EventHandler(this.btnOkay_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel1.Controls.Add(this.Angular_Group_Box, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.LinearGroup, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(10, 11);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(280, 141);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // Angular_Group_Box
            // 
            this.Angular_Group_Box.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.Angular_Group_Box.Controls.Add(this.tableLayoutPanel2);
            this.Angular_Group_Box.Location = new System.Drawing.Point(2, 2);
            this.Angular_Group_Box.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_Group_Box.Name = "Angular_Group_Box";
            this.Angular_Group_Box.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_Group_Box.Size = new System.Drawing.Size(276, 66);
            this.Angular_Group_Box.TabIndex = 0;
            this.Angular_Group_Box.TabStop = false;
            this.Angular_Group_Box.Text = "Angular (Degrees)";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel2.Controls.Add(this.Angular_Start_textbox, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.Angular_Current_textbox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.Angular_End_textbox, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.Angular_End, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.Angular_Current, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.Angular_Start, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(5, 18);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(266, 43);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // Angular_Start_textbox
            // 
            this.Angular_Start_textbox.DecimalPlaces = 5;
            this.Angular_Start_textbox.Location = new System.Drawing.Point(2, 23);
            this.Angular_Start_textbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_Start_textbox.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.Angular_Start_textbox.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.Angular_Start_textbox.Name = "Angular_Start_textbox";
            this.Angular_Start_textbox.Size = new System.Drawing.Size(84, 20);
            this.Angular_Start_textbox.TabIndex = 0;
            // 
            // Angular_Current_textbox
            // 
            this.Angular_Current_textbox.DecimalPlaces = 5;
            this.Angular_Current_textbox.Location = new System.Drawing.Point(90, 23);
            this.Angular_Current_textbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_Current_textbox.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.Angular_Current_textbox.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.Angular_Current_textbox.Name = "Angular_Current_textbox";
            this.Angular_Current_textbox.Size = new System.Drawing.Size(84, 20);
            this.Angular_Current_textbox.TabIndex = 1;
            this.Angular_Current_textbox.ValueChanged += new System.EventHandler(this.Angular_Current_textbox_ValueChanged);
            // 
            // Angular_End_textbox
            // 
            this.Angular_End_textbox.DecimalPlaces = 5;
            this.Angular_End_textbox.Location = new System.Drawing.Point(178, 23);
            this.Angular_End_textbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_End_textbox.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.Angular_End_textbox.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.Angular_End_textbox.Name = "Angular_End_textbox";
            this.Angular_End_textbox.Size = new System.Drawing.Size(85, 20);
            this.Angular_End_textbox.TabIndex = 2;
            // 
            // Angular_End
            // 
            this.Angular_End.AutoSize = true;
            this.Angular_End.Location = new System.Drawing.Point(178, 2);
            this.Angular_End.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_End.Name = "Angular_End";
            this.Angular_End.Size = new System.Drawing.Size(45, 17);
            this.Angular_End.TabIndex = 5;
            this.Angular_End.Text = "End";
            this.Angular_End.UseVisualStyleBackColor = true;
            this.Angular_End.CheckedChanged += new System.EventHandler(this.Angular_End_CheckedChanged);
            // 
            // Angular_Current
            // 
            this.Angular_Current.AutoSize = true;
            this.Angular_Current.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Angular_Current.Location = new System.Drawing.Point(90, 3);
            this.Angular_Current.Margin = new System.Windows.Forms.Padding(2, 3, 2, 0);
            this.Angular_Current.Name = "Angular_Current";
            this.Angular_Current.Size = new System.Drawing.Size(84, 18);
            this.Angular_Current.TabIndex = 4;
            this.Angular_Current.Text = "       Current";
            // 
            // Angular_Start
            // 
            this.Angular_Start.AutoSize = true;
            this.Angular_Start.Location = new System.Drawing.Point(2, 2);
            this.Angular_Start.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Angular_Start.Name = "Angular_Start";
            this.Angular_Start.Size = new System.Drawing.Size(48, 17);
            this.Angular_Start.TabIndex = 3;
            this.Angular_Start.Text = "Start";
            this.Angular_Start.UseVisualStyleBackColor = true;
            this.Angular_Start.CheckedChanged += new System.EventHandler(this.Angular_Start_CheckedChanged);
            // 
            // LinearGroup
            // 
            this.LinearGroup.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.LinearGroup.Controls.Add(this.tableLayoutPanel3);
            this.LinearGroup.Location = new System.Drawing.Point(2, 72);
            this.LinearGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LinearGroup.Name = "LinearGroup";
            this.LinearGroup.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LinearGroup.Size = new System.Drawing.Size(276, 66);
            this.LinearGroup.TabIndex = 1;
            this.LinearGroup.TabStop = false;
            this.LinearGroup.Text = "Linear (In)";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.Controls.Add(this.Linear_Start_textbox, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.Linear_Current_textbox, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.Linear_End, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.Linear_Current, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.Linear_Start, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.Linear_End_textbox, 2, 1);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(5, 18);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(266, 43);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // Linear_Start_textbox
            // 
            this.Linear_Start_textbox.DecimalPlaces = 5;
            this.Linear_Start_textbox.Location = new System.Drawing.Point(2, 23);
            this.Linear_Start_textbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Linear_Start_textbox.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.Linear_Start_textbox.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.Linear_Start_textbox.Name = "Linear_Start_textbox";
            this.Linear_Start_textbox.Size = new System.Drawing.Size(84, 20);
            this.Linear_Start_textbox.TabIndex = 0;
            // 
            // Linear_Current_textbox
            // 
            this.Linear_Current_textbox.DecimalPlaces = 5;
            this.Linear_Current_textbox.Location = new System.Drawing.Point(90, 23);
            this.Linear_Current_textbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Linear_Current_textbox.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.Linear_Current_textbox.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.Linear_Current_textbox.Name = "Linear_Current_textbox";
            this.Linear_Current_textbox.Size = new System.Drawing.Size(84, 20);
            this.Linear_Current_textbox.TabIndex = 1;
            this.Linear_Current_textbox.ValueChanged += new System.EventHandler(this.Linear_Current_textbox_ValueChanged);
            // 
            // Linear_End
            // 
            this.Linear_End.AutoSize = true;
            this.Linear_End.Location = new System.Drawing.Point(178, 2);
            this.Linear_End.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Linear_End.Name = "Linear_End";
            this.Linear_End.Size = new System.Drawing.Size(45, 17);
            this.Linear_End.TabIndex = 5;
            this.Linear_End.Text = "End";
            this.Linear_End.UseVisualStyleBackColor = true;
            this.Linear_End.CheckedChanged += new System.EventHandler(this.Linear_End_CheckedChanged);
            // 
            // Linear_Current
            // 
            this.Linear_Current.AutoSize = true;
            this.Linear_Current.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Linear_Current.Location = new System.Drawing.Point(90, 3);
            this.Linear_Current.Margin = new System.Windows.Forms.Padding(2, 3, 2, 0);
            this.Linear_Current.Name = "Linear_Current";
            this.Linear_Current.Size = new System.Drawing.Size(84, 18);
            this.Linear_Current.TabIndex = 4;
            this.Linear_Current.Text = "       Current";
            // 
            // Linear_Start
            // 
            this.Linear_Start.AutoSize = true;
            this.Linear_Start.Location = new System.Drawing.Point(2, 2);
            this.Linear_Start.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Linear_Start.Name = "Linear_Start";
            this.Linear_Start.Size = new System.Drawing.Size(48, 17);
            this.Linear_Start.TabIndex = 3;
            this.Linear_Start.Text = "Start";
            this.Linear_Start.UseVisualStyleBackColor = true;
            this.Linear_Start.CheckedChanged += new System.EventHandler(this.Linear_Start_CheckedChanged);
            // 
            // Linear_End_textbox
            // 
            this.Linear_End_textbox.DecimalPlaces = 5;
            this.Linear_End_textbox.Location = new System.Drawing.Point(178, 23);
            this.Linear_End_textbox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Linear_End_textbox.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.Linear_End_textbox.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.Linear_End_textbox.Name = "Linear_End_textbox";
            this.Linear_End_textbox.Size = new System.Drawing.Size(85, 20);
            this.Linear_End_textbox.TabIndex = 2;
            // 
            // AnimateJointButton
            // 
            this.AnimateJointButton.Location = new System.Drawing.Point(8, 15);
            this.AnimateJointButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.AnimateJointButton.Name = "AnimateJointButton";
            this.AnimateJointButton.Size = new System.Drawing.Size(84, 19);
            this.AnimateJointButton.TabIndex = 1;
            this.AnimateJointButton.Text = "Animate Joint";
            this.AnimateJointButton.UseVisualStyleBackColor = true;
            this.AnimateJointButton.Click += new System.EventHandler(this.AnimateJointButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.groupBox1.Controls.Add(this.AnimateJointButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 157);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(278, 38);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Animate";
            // 
            // EditLimits
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(299, 230);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.btnOkay);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditLimits";
            this.Text = "Edit Limits";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.Angular_Group_Box.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Angular_Start_textbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Angular_Current_textbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Angular_End_textbox)).EndInit();
            this.LinearGroup.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Linear_Start_textbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Linear_Current_textbox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Linear_End_textbox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox Angular_Group_Box;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.NumericUpDown Angular_Start_textbox;
        private System.Windows.Forms.NumericUpDown Angular_Current_textbox;
        private System.Windows.Forms.NumericUpDown Angular_End_textbox;
        private System.Windows.Forms.CheckBox Angular_Start;
        private System.Windows.Forms.Label Angular_Current;
        private System.Windows.Forms.CheckBox Angular_End;
        private System.Windows.Forms.GroupBox LinearGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.NumericUpDown Linear_Start_textbox;
        private System.Windows.Forms.NumericUpDown Linear_Current_textbox;
        private System.Windows.Forms.NumericUpDown Linear_End_textbox;
        private System.Windows.Forms.CheckBox Linear_End;
        private System.Windows.Forms.Label Linear_Current;
        private System.Windows.Forms.CheckBox Linear_Start;
        private System.Windows.Forms.Button AnimateJointButton;
        private System.Windows.Forms.GroupBox groupBox1;

        private class LimitPane<T> : System.Windows.Forms.TabPage
        {
            private System.Windows.Forms.CheckBox chkHasLimits;
            private System.Windows.Forms.TextBox txtUpper;
            private System.Windows.Forms.TextBox txtLower;
            private System.Windows.Forms.Label lblUpper, lblLower;

            private T dof;

            private float cacheLower, cacheUpper;

            public LimitPane(T dof, string label)
            {
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
                    AngularDOF dof = (AngularDOF) this.dof;
                    chkHasLimits.Checked = dof.hasAngularLimits();
                    lblUpper.Visible = lblLower.Visible = txtUpper.Visible = txtLower.Visible = chkHasLimits.Checked;

                    txtLower.Text = Convert.ToString(cacheLower = dof.lowerLimit);
                    txtUpper.Text = Convert.ToString(cacheUpper = dof.upperLimit);
                    lblLower.Text = "Lower (rad)";
                    lblUpper.Text = "Upper (rad)";
                }
                else if (this.dof is LinearDOF)
                {
                    LinearDOF dof = (LinearDOF) this.dof;
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
                    AngularDOF dof = (AngularDOF) this.dof;
                    dof.lowerLimit = cacheLower;
                    dof.upperLimit = cacheUpper;
                }
                else if (this.dof is LinearDOF)
                {
                    LinearDOF dof = (LinearDOF) this.dof;
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
                        AngularDOF dof = (AngularDOF) this.dof;
                        dof.lowerLimit = chkHasLimits.Checked ? Convert.ToSingle(txtLower.Text) : float.NegativeInfinity;
                        dof.upperLimit = chkHasLimits.Checked ? Convert.ToSingle(txtUpper.Text) : float.PositiveInfinity;
                    }
                    else if (this.dof is LinearDOF)
                    {
                        LinearDOF dof = (LinearDOF) this.dof;
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
                }
            }
        }
    }
}