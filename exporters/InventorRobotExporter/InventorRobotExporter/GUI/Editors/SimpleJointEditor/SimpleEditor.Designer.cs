namespace InventorRobotExporter.GUI.Editors.SimpleJointEditor
{
    partial class SimpleEditor
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
            this.jointNavigator = new System.Windows.Forms.ComboBox();
            this.jointTypeBox = new System.Windows.Forms.GroupBox();
            this.jointTypeInput = new System.Windows.Forms.ComboBox();
            this.weightBox = new System.Windows.Forms.GroupBox();
            this.weightAmountInput = new System.Windows.Forms.NumericUpDown();
            this.jointDriverBox = new System.Windows.Forms.GroupBox();
            this.jointDriverInput = new System.Windows.Forms.ComboBox();
            this.advancedButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.nameBox = new System.Windows.Forms.GroupBox();
            this.nameInput = new System.Windows.Forms.TextBox();
            this.wheelTypeBox = new System.Windows.Forms.GroupBox();
            this.wheelTypeInput = new System.Windows.Forms.ComboBox();
            this.driveSideBox = new System.Windows.Forms.GroupBox();
            this.driveSideInput = new System.Windows.Forms.ComboBox();
            this.limitsBox = new System.Windows.Forms.GroupBox();
            this.animateMovementButton = new System.Windows.Forms.Button();
            this.limitEndInput = new System.Windows.Forms.NumericUpDown();
            this.limitEndCheckbox = new System.Windows.Forms.CheckBox();
            this.limitStartInput = new System.Windows.Forms.NumericUpDown();
            this.limitStartCheckbox = new System.Windows.Forms.CheckBox();
            this.jointPreviewImage = new System.Windows.Forms.PictureBox();
            this.backButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.jointTypeBox.SuspendLayout();
            this.weightBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.weightAmountInput)).BeginInit();
            this.jointDriverBox.SuspendLayout();
            this.panel1.SuspendLayout();
            this.nameBox.SuspendLayout();
            this.wheelTypeBox.SuspendLayout();
            this.driveSideBox.SuspendLayout();
            this.limitsBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.limitEndInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.limitStartInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.jointPreviewImage)).BeginInit();
            this.SuspendLayout();
            // 
            // jointNavigator
            // 
            this.jointNavigator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointNavigator.FormattingEnabled = true;
            this.jointNavigator.Location = new System.Drawing.Point(111, 10);
            this.jointNavigator.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointNavigator.Name = "jointNavigator";
            this.jointNavigator.Size = new System.Drawing.Size(259, 24);
            this.jointNavigator.TabIndex = 0;
            this.jointNavigator.SelectedIndexChanged += new System.EventHandler(this.JointNavigator_SelectedIndexChanged);
            // 
            // jointTypeBox
            // 
            this.jointTypeBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.jointTypeBox.Controls.Add(this.jointTypeInput);
            this.jointTypeBox.Location = new System.Drawing.Point(15, 64);
            this.jointTypeBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointTypeBox.Name = "jointTypeBox";
            this.jointTypeBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointTypeBox.Size = new System.Drawing.Size(163, 50);
            this.jointTypeBox.TabIndex = 2;
            this.jointTypeBox.TabStop = false;
            this.jointTypeBox.Text = "Joint Type";
            // 
            // jointTypeInput
            // 
            this.jointTypeInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointTypeInput.FormattingEnabled = true;
            this.jointTypeInput.Items.AddRange(new object[] {
            "(Not Selected)",
            "Drivetrain Wheel",
            "Mechanism Joint"});
            this.jointTypeInput.Location = new System.Drawing.Point(5, 21);
            this.jointTypeInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointTypeInput.Name = "jointTypeInput";
            this.jointTypeInput.Size = new System.Drawing.Size(151, 24);
            this.jointTypeInput.TabIndex = 0;
            this.jointTypeInput.SelectedIndexChanged += new System.EventHandler(this.JointTypeInput_SelectedIndexChanged);
            // 
            // weightBox
            // 
            this.weightBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.weightBox.Controls.Add(this.weightAmountInput);
            this.weightBox.Location = new System.Drawing.Point(15, 129);
            this.weightBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.weightBox.Name = "weightBox";
            this.weightBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.weightBox.Size = new System.Drawing.Size(163, 50);
            this.weightBox.TabIndex = 3;
            this.weightBox.TabStop = false;
            this.weightBox.Text = "Weight (lbs)";
            this.weightBox.Visible = false;
            // 
            // weightAmountInput
            // 
            this.weightAmountInput.DecimalPlaces = 1;
            this.weightAmountInput.Location = new System.Drawing.Point(5, 21);
            this.weightAmountInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.weightAmountInput.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.weightAmountInput.Name = "weightAmountInput";
            this.weightAmountInput.Size = new System.Drawing.Size(151, 22);
            this.weightAmountInput.TabIndex = 0;
            this.weightAmountInput.Value = new decimal(new int[] {
            225,
            0,
            0,
            65536});
            this.weightAmountInput.Visible = false;
            // 
            // jointDriverBox
            // 
            this.jointDriverBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.jointDriverBox.Controls.Add(this.jointDriverInput);
            this.jointDriverBox.Location = new System.Drawing.Point(15, 196);
            this.jointDriverBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointDriverBox.Name = "jointDriverBox";
            this.jointDriverBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointDriverBox.Size = new System.Drawing.Size(163, 50);
            this.jointDriverBox.TabIndex = 4;
            this.jointDriverBox.TabStop = false;
            this.jointDriverBox.Text = "Joint Driver";
            this.jointDriverBox.Visible = false;
            // 
            // jointDriverInput
            // 
            this.jointDriverInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.jointDriverInput.FormattingEnabled = true;
            this.jointDriverInput.Items.AddRange(new object[] {
            "Motor",
            "Servo",
            "Dual Motor"});
            this.jointDriverInput.Location = new System.Drawing.Point(5, 21);
            this.jointDriverInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.jointDriverInput.Name = "jointDriverInput";
            this.jointDriverInput.Size = new System.Drawing.Size(151, 24);
            this.jointDriverInput.TabIndex = 0;
            this.jointDriverInput.Visible = false;
            // 
            // advancedButton
            // 
            this.advancedButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.advancedButton.Location = new System.Drawing.Point(12, 405);
            this.advancedButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.advancedButton.Name = "advancedButton";
            this.advancedButton.Size = new System.Drawing.Size(115, 27);
            this.advancedButton.TabIndex = 6;
            this.advancedButton.Text = "Advanced...";
            this.advancedButton.UseVisualStyleBackColor = true;
            this.advancedButton.Visible = false;
            this.advancedButton.Click += new System.EventHandler(this.AdvancedButton_Click);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(370, 405);
            this.okButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(95, 27);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // panel1
            // 
            this.panel1.AutoSize = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel1.Controls.Add(this.nameBox);
            this.panel1.Controls.Add(this.wheelTypeBox);
            this.panel1.Controls.Add(this.driveSideBox);
            this.panel1.Controls.Add(this.limitsBox);
            this.panel1.Controls.Add(this.jointPreviewImage);
            this.panel1.Controls.Add(this.jointTypeBox);
            this.panel1.Controls.Add(this.weightBox);
            this.panel1.Controls.Add(this.jointDriverBox);
            this.panel1.Location = new System.Drawing.Point(12, 49);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(458, 343);
            this.panel1.TabIndex = 9;
            // 
            // nameBox
            // 
            this.nameBox.Controls.Add(this.nameInput);
            this.nameBox.Location = new System.Drawing.Point(15, 7);
            this.nameBox.Margin = new System.Windows.Forms.Padding(4);
            this.nameBox.Name = "nameBox";
            this.nameBox.Padding = new System.Windows.Forms.Padding(4);
            this.nameBox.Size = new System.Drawing.Size(163, 50);
            this.nameBox.TabIndex = 11;
            this.nameBox.TabStop = false;
            this.nameBox.Text = "Name";
            // 
            // nameInput
            // 
            this.nameInput.Location = new System.Drawing.Point(5, 18);
            this.nameInput.Margin = new System.Windows.Forms.Padding(4);
            this.nameInput.Name = "nameInput";
            this.nameInput.Size = new System.Drawing.Size(151, 22);
            this.nameInput.TabIndex = 0;
            // 
            // wheelTypeBox
            // 
            this.wheelTypeBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.wheelTypeBox.Controls.Add(this.wheelTypeInput);
            this.wheelTypeBox.Location = new System.Drawing.Point(15, 191);
            this.wheelTypeBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.wheelTypeBox.Name = "wheelTypeBox";
            this.wheelTypeBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.wheelTypeBox.Size = new System.Drawing.Size(163, 50);
            this.wheelTypeBox.TabIndex = 10;
            this.wheelTypeBox.TabStop = false;
            this.wheelTypeBox.Text = "Wheel Type";
            this.wheelTypeBox.Visible = false;
            // 
            // wheelTypeInput
            // 
            this.wheelTypeInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wheelTypeInput.FormattingEnabled = true;
            this.wheelTypeInput.Items.AddRange(new object[] {
            "Normal",
            "Omni",
            "Mecanum"});
            this.wheelTypeInput.Location = new System.Drawing.Point(5, 21);
            this.wheelTypeInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.wheelTypeInput.Name = "wheelTypeInput";
            this.wheelTypeInput.Size = new System.Drawing.Size(151, 24);
            this.wheelTypeInput.TabIndex = 0;
            this.wheelTypeInput.Visible = false;
            // 
            // driveSideBox
            // 
            this.driveSideBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.driveSideBox.Controls.Add(this.driveSideInput);
            this.driveSideBox.Location = new System.Drawing.Point(15, 124);
            this.driveSideBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.driveSideBox.Name = "driveSideBox";
            this.driveSideBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.driveSideBox.Size = new System.Drawing.Size(163, 50);
            this.driveSideBox.TabIndex = 6;
            this.driveSideBox.TabStop = false;
            this.driveSideBox.Text = "Drivetrain Side";
            this.driveSideBox.Visible = false;
            // 
            // driveSideInput
            // 
            this.driveSideInput.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.driveSideInput.FormattingEnabled = true;
            this.driveSideInput.Items.AddRange(new object[] {
            "Right",
            "Left",
            "H-Drive Center"});
            this.driveSideInput.Location = new System.Drawing.Point(5, 21);
            this.driveSideInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.driveSideInput.Name = "driveSideInput";
            this.driveSideInput.Size = new System.Drawing.Size(151, 24);
            this.driveSideInput.TabIndex = 0;
            this.driveSideInput.Visible = false;
            // 
            // limitsBox
            // 
            this.limitsBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.limitsBox.Controls.Add(this.animateMovementButton);
            this.limitsBox.Controls.Add(this.limitEndInput);
            this.limitsBox.Controls.Add(this.limitEndCheckbox);
            this.limitsBox.Controls.Add(this.limitStartInput);
            this.limitsBox.Controls.Add(this.limitStartCheckbox);
            this.limitsBox.Location = new System.Drawing.Point(15, 256);
            this.limitsBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.limitsBox.Name = "limitsBox";
            this.limitsBox.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.limitsBox.Size = new System.Drawing.Size(424, 85);
            this.limitsBox.TabIndex = 5;
            this.limitsBox.TabStop = false;
            this.limitsBox.Text = "Limits (linear)";
            this.limitsBox.Visible = false;
            // 
            // animateMovementButton
            // 
            this.animateMovementButton.Location = new System.Drawing.Point(281, 25);
            this.animateMovementButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.animateMovementButton.Name = "animateMovementButton";
            this.animateMovementButton.Size = new System.Drawing.Size(132, 49);
            this.animateMovementButton.TabIndex = 5;
            this.animateMovementButton.Text = "Animate Movement";
            this.animateMovementButton.UseVisualStyleBackColor = true;
            this.animateMovementButton.Visible = false;
            this.animateMovementButton.Click += new System.EventHandler(this.AnimateMovementButton_Click);
            // 
            // limitEndInput
            // 
            this.limitEndInput.Location = new System.Drawing.Point(141, 52);
            this.limitEndInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.limitEndInput.Name = "limitEndInput";
            this.limitEndInput.Size = new System.Drawing.Size(120, 22);
            this.limitEndInput.TabIndex = 4;
            this.limitEndInput.Visible = false;
            // 
            // limitEndCheckbox
            // 
            this.limitEndCheckbox.AutoSize = true;
            this.limitEndCheckbox.Checked = true;
            this.limitEndCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.limitEndCheckbox.Location = new System.Drawing.Point(141, 25);
            this.limitEndCheckbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.limitEndCheckbox.Name = "limitEndCheckbox";
            this.limitEndCheckbox.Size = new System.Drawing.Size(55, 21);
            this.limitEndCheckbox.TabIndex = 3;
            this.limitEndCheckbox.Text = "End";
            this.limitEndCheckbox.UseVisualStyleBackColor = true;
            this.limitEndCheckbox.Visible = false;
            // 
            // limitStartInput
            // 
            this.limitStartInput.Location = new System.Drawing.Point(9, 52);
            this.limitStartInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.limitStartInput.Name = "limitStartInput";
            this.limitStartInput.Size = new System.Drawing.Size(120, 22);
            this.limitStartInput.TabIndex = 2;
            this.limitStartInput.Visible = false;
            // 
            // limitStartCheckbox
            // 
            this.limitStartCheckbox.AutoSize = true;
            this.limitStartCheckbox.Checked = true;
            this.limitStartCheckbox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.limitStartCheckbox.Location = new System.Drawing.Point(9, 25);
            this.limitStartCheckbox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.limitStartCheckbox.Name = "limitStartCheckbox";
            this.limitStartCheckbox.Size = new System.Drawing.Size(60, 21);
            this.limitStartCheckbox.TabIndex = 1;
            this.limitStartCheckbox.Text = "Start";
            this.limitStartCheckbox.UseVisualStyleBackColor = true;
            this.limitStartCheckbox.Visible = false;
            // 
            // jointPreviewImage
            // 
            this.jointPreviewImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.jointPreviewImage.Location = new System.Drawing.Point(196, 18);
            this.jointPreviewImage.Margin = new System.Windows.Forms.Padding(3, 2, 19, 10);
            this.jointPreviewImage.Name = "jointPreviewImage";
            this.jointPreviewImage.Size = new System.Drawing.Size(243, 221);
            this.jointPreviewImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.jointPreviewImage.TabIndex = 5;
            this.jointPreviewImage.TabStop = false;
            // 
            // backButton
            // 
            this.backButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.backButton.Location = new System.Drawing.Point(12, 9);
            this.backButton.Margin = new System.Windows.Forms.Padding(4);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(95, 28);
            this.backButton.TabIndex = 10;
            this.backButton.Text = "< Previous";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.nextButton.Location = new System.Drawing.Point(376, 9);
            this.nextButton.Margin = new System.Windows.Forms.Padding(4);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(95, 28);
            this.nextButton.TabIndex = 11;
            this.nextButton.Text = "Next >";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.NextButton_Click);
            // 
            // SimpleEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(477, 444);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.backButton);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.advancedButton);
            this.Controls.Add(this.jointNavigator);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = global::InventorRobotExporter.Properties.Resources.SynthesisLogoIco;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimpleEditor";
            this.Text = "Joint Editor";
            this.jointTypeBox.ResumeLayout(false);
            this.weightBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.weightAmountInput)).EndInit();
            this.jointDriverBox.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.nameBox.ResumeLayout(false);
            this.nameBox.PerformLayout();
            this.wheelTypeBox.ResumeLayout(false);
            this.driveSideBox.ResumeLayout(false);
            this.limitsBox.ResumeLayout(false);
            this.limitsBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.limitEndInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.limitStartInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.jointPreviewImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox jointNavigator;
        private System.Windows.Forms.GroupBox jointTypeBox;
        private System.Windows.Forms.ComboBox jointTypeInput;
        private System.Windows.Forms.GroupBox weightBox;
        private System.Windows.Forms.GroupBox jointDriverBox;
        private System.Windows.Forms.ComboBox jointDriverInput;
        private System.Windows.Forms.Button advancedButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.NumericUpDown weightAmountInput;
        private System.Windows.Forms.PictureBox jointPreviewImage;
        private System.Windows.Forms.GroupBox wheelTypeBox;
        private System.Windows.Forms.ComboBox wheelTypeInput;
        private System.Windows.Forms.GroupBox driveSideBox;
        private System.Windows.Forms.ComboBox driveSideInput;
        private System.Windows.Forms.GroupBox limitsBox;
        private System.Windows.Forms.Button animateMovementButton;
        private System.Windows.Forms.NumericUpDown limitEndInput;
        private System.Windows.Forms.CheckBox limitEndCheckbox;
        private System.Windows.Forms.NumericUpDown limitStartInput;
        private System.Windows.Forms.CheckBox limitStartCheckbox;
        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.GroupBox nameBox;
        private System.Windows.Forms.TextBox nameInput;
    }
}