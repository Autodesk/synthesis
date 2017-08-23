namespace StandaloneViewer
{
    partial class ViewerSettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewerSettingsForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkboxDebug = new System.Windows.Forms.CheckBox();
            this.flavorText2 = new System.Windows.Forms.Label();
            this.flavorText1 = new System.Windows.Forms.Label();
            this.trackbarCameraSen = new System.Windows.Forms.TrackBar();
            this.labelSensitivity = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.buttonChooseTint = new System.Windows.Forms.Button();
            this.labelTintColor = new System.Windows.Forms.Label();
            this.checkboxActuate = new System.Windows.Forms.CheckBox();
            this.checkboxDrawAxes = new System.Windows.Forms.CheckBox();
            this.checkboxTint = new System.Windows.Forms.CheckBox();
            this.buttonChooseHighlight = new System.Windows.Forms.Button();
            this.labelHighlightColor = new System.Windows.Forms.Label();
            this.checkboxHighlight = new System.Windows.Forms.CheckBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.comboBoxUnits = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackbarCameraSen)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkboxDebug);
            this.groupBox1.Controls.Add(this.flavorText2);
            this.groupBox1.Controls.Add(this.flavorText1);
            this.groupBox1.Controls.Add(this.trackbarCameraSen);
            this.groupBox1.Controls.Add(this.labelSensitivity);
            this.groupBox1.ForeColor = System.Drawing.Color.Gray;
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(358, 207);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Camera";
            // 
            // checkboxDebug
            // 
            this.checkboxDebug.AutoSize = true;
            this.checkboxDebug.ForeColor = System.Drawing.Color.Black;
            this.checkboxDebug.Location = new System.Drawing.Point(6, 168);
            this.checkboxDebug.Name = "checkboxDebug";
            this.checkboxDebug.Size = new System.Drawing.Size(157, 21);
            this.checkboxDebug.TabIndex = 4;
            this.checkboxDebug.Text = "Enable debug mode";
            this.checkboxDebug.UseVisualStyleBackColor = true;
            // 
            // flavorText2
            // 
            this.flavorText2.AutoSize = true;
            this.flavorText2.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flavorText2.ForeColor = System.Drawing.Color.Black;
            this.flavorText2.Location = new System.Drawing.Point(248, 71);
            this.flavorText2.Name = "flavorText2";
            this.flavorText2.Size = new System.Drawing.Size(104, 17);
            this.flavorText2.TabIndex = 3;
            this.flavorText2.Text = "MLG Noscoper";
            // 
            // flavorText1
            // 
            this.flavorText1.AutoSize = true;
            this.flavorText1.Font = new System.Drawing.Font("Courier New", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.flavorText1.ForeColor = System.Drawing.Color.Black;
            this.flavorText1.Location = new System.Drawing.Point(7, 71);
            this.flavorText1.Name = "flavorText1";
            this.flavorText1.Size = new System.Drawing.Size(48, 17);
            this.flavorText1.TabIndex = 2;
            this.flavorText1.Text = "Snail";
            // 
            // trackbarCameraSen
            // 
            this.trackbarCameraSen.LargeChange = 2;
            this.trackbarCameraSen.Location = new System.Drawing.Point(6, 91);
            this.trackbarCameraSen.Maximum = 11;
            this.trackbarCameraSen.Minimum = 1;
            this.trackbarCameraSen.Name = "trackbarCameraSen";
            this.trackbarCameraSen.Size = new System.Drawing.Size(346, 56);
            this.trackbarCameraSen.TabIndex = 1;
            this.trackbarCameraSen.Value = 6;
            // 
            // labelSensitivity
            // 
            this.labelSensitivity.AutoSize = true;
            this.labelSensitivity.ForeColor = System.Drawing.Color.Black;
            this.labelSensitivity.Location = new System.Drawing.Point(117, 34);
            this.labelSensitivity.Name = "labelSensitivity";
            this.labelSensitivity.Size = new System.Drawing.Size(124, 17);
            this.labelSensitivity.TabIndex = 0;
            this.labelSensitivity.Text = "Camera Sensitivity";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.comboBoxUnits);
            this.groupBox2.Controls.Add(this.buttonChooseTint);
            this.groupBox2.Controls.Add(this.labelTintColor);
            this.groupBox2.Controls.Add(this.checkboxActuate);
            this.groupBox2.Controls.Add(this.checkboxDrawAxes);
            this.groupBox2.Controls.Add(this.checkboxTint);
            this.groupBox2.Controls.Add(this.buttonChooseHighlight);
            this.groupBox2.Controls.Add(this.labelHighlightColor);
            this.groupBox2.Controls.Add(this.checkboxHighlight);
            this.groupBox2.ForeColor = System.Drawing.Color.Gray;
            this.groupBox2.Location = new System.Drawing.Point(13, 225);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(357, 252);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Model";
            // 
            // buttonChooseTint
            // 
            this.buttonChooseTint.BackColor = System.Drawing.Color.Black;
            this.buttonChooseTint.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChooseTint.Location = new System.Drawing.Point(145, 117);
            this.buttonChooseTint.Name = "buttonChooseTint";
            this.buttonChooseTint.Size = new System.Drawing.Size(102, 24);
            this.buttonChooseTint.TabIndex = 10;
            this.buttonChooseTint.UseVisualStyleBackColor = false;
            this.buttonChooseTint.Click += new System.EventHandler(this.buttonChooseTint_Click);
            // 
            // labelTintColor
            // 
            this.labelTintColor.AutoSize = true;
            this.labelTintColor.ForeColor = System.Drawing.Color.Black;
            this.labelTintColor.Location = new System.Drawing.Point(68, 121);
            this.labelTintColor.Name = "labelTintColor";
            this.labelTintColor.Size = new System.Drawing.Size(71, 17);
            this.labelTintColor.TabIndex = 9;
            this.labelTintColor.Text = "Tint color:";
            // 
            // checkboxActuate
            // 
            this.checkboxActuate.AutoSize = true;
            this.checkboxActuate.Checked = true;
            this.checkboxActuate.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxActuate.ForeColor = System.Drawing.Color.Black;
            this.checkboxActuate.Location = new System.Drawing.Point(9, 174);
            this.checkboxActuate.Name = "checkboxActuate";
            this.checkboxActuate.Size = new System.Drawing.Size(115, 21);
            this.checkboxActuate.TabIndex = 6;
            this.checkboxActuate.Text = "Actuate joints";
            this.checkboxActuate.UseVisualStyleBackColor = true;
            // 
            // checkboxDrawAxes
            // 
            this.checkboxDrawAxes.AutoSize = true;
            this.checkboxDrawAxes.ForeColor = System.Drawing.Color.Black;
            this.checkboxDrawAxes.Location = new System.Drawing.Point(40, 147);
            this.checkboxDrawAxes.Name = "checkboxDrawAxes";
            this.checkboxDrawAxes.Size = new System.Drawing.Size(234, 21);
            this.checkboxDrawAxes.TabIndex = 5;
            this.checkboxDrawAxes.Text = "Draw axes of motion on highlight";
            this.checkboxDrawAxes.UseVisualStyleBackColor = true;
            // 
            // checkboxTint
            // 
            this.checkboxTint.AutoSize = true;
            this.checkboxTint.Checked = true;
            this.checkboxTint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxTint.ForeColor = System.Drawing.Color.Black;
            this.checkboxTint.Location = new System.Drawing.Point(40, 90);
            this.checkboxTint.Name = "checkboxTint";
            this.checkboxTint.Size = new System.Drawing.Size(148, 21);
            this.checkboxTint.TabIndex = 4;
            this.checkboxTint.Text = "Tint on mouseover";
            this.checkboxTint.UseVisualStyleBackColor = true;
            this.checkboxTint.CheckedChanged += new System.EventHandler(this.checkboxTint_CheckedChanged);
            // 
            // buttonChooseHighlight
            // 
            this.buttonChooseHighlight.BackColor = System.Drawing.Color.Black;
            this.buttonChooseHighlight.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonChooseHighlight.Location = new System.Drawing.Point(145, 60);
            this.buttonChooseHighlight.Name = "buttonChooseHighlight";
            this.buttonChooseHighlight.Size = new System.Drawing.Size(102, 24);
            this.buttonChooseHighlight.TabIndex = 3;
            this.buttonChooseHighlight.UseVisualStyleBackColor = false;
            this.buttonChooseHighlight.Click += new System.EventHandler(this.buttonChooseHighlight_Click);
            // 
            // labelHighlightColor
            // 
            this.labelHighlightColor.AutoSize = true;
            this.labelHighlightColor.ForeColor = System.Drawing.Color.Black;
            this.labelHighlightColor.Location = new System.Drawing.Point(37, 64);
            this.labelHighlightColor.Name = "labelHighlightColor";
            this.labelHighlightColor.Size = new System.Drawing.Size(102, 17);
            this.labelHighlightColor.TabIndex = 2;
            this.labelHighlightColor.Text = "Highlight color:";
            // 
            // checkboxHighlight
            // 
            this.checkboxHighlight.AutoSize = true;
            this.checkboxHighlight.Checked = true;
            this.checkboxHighlight.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkboxHighlight.ForeColor = System.Drawing.Color.Black;
            this.checkboxHighlight.Location = new System.Drawing.Point(9, 33);
            this.checkboxHighlight.Name = "checkboxHighlight";
            this.checkboxHighlight.Size = new System.Drawing.Size(192, 21);
            this.checkboxHighlight.TabIndex = 0;
            this.checkboxHighlight.Text = "Enable model highlighting";
            this.checkboxHighlight.UseVisualStyleBackColor = true;
            this.checkboxHighlight.CheckedChanged += new System.EventHandler(this.checkboxHighlight_CheckedChanged);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(210, 507);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(160, 36);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(12, 507);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(160, 36);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // comboBoxUnits
            // 
            this.comboBoxUnits.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxUnits.FormattingEnabled = true;
            this.comboBoxUnits.Items.AddRange(new object[] {
            "Pounds (lb)",
            "Kilograms (kg)"});
            this.comboBoxUnits.Location = new System.Drawing.Point(104, 211);
            this.comboBoxUnits.Name = "comboBoxUnits";
            this.comboBoxUnits.Size = new System.Drawing.Size(154, 24);
            this.comboBoxUnits.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(6, 214);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 17);
            this.label1.TabIndex = 12;
            this.label1.Text = "Weight Units:";
            // 
            // ViewerSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 555);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ViewerSettingsForm";
            this.Text = "Viewer Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackbarCameraSen)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.TrackBar trackbarCameraSen;
        private System.Windows.Forms.Label labelSensitivity;
        private System.Windows.Forms.CheckBox checkboxDebug;
        private System.Windows.Forms.Label flavorText2;
        private System.Windows.Forms.Label flavorText1;
        private System.Windows.Forms.Label labelHighlightColor;
        private System.Windows.Forms.CheckBox checkboxHighlight;
        private System.Windows.Forms.Button buttonChooseHighlight;
        private System.Windows.Forms.CheckBox checkboxActuate;
        private System.Windows.Forms.CheckBox checkboxDrawAxes;
        private System.Windows.Forms.CheckBox checkboxTint;
        private System.Windows.Forms.Button buttonChooseTint;
        private System.Windows.Forms.Label labelTintColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxUnits;


    }
}