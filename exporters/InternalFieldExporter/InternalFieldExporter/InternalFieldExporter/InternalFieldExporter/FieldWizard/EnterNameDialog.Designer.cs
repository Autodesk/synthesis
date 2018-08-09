namespace InternalFieldExporter.FieldWizard
{
    partial class EnterNameDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterNameDialog));
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.enterNameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.reservedLabel = new System.Windows.Forms.Label();
            this.warningImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).BeginInit();
            this.SuspendLayout();
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.okButton.Location = new System.Drawing.Point(344, 55);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(128, 39);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cancelButton.Location = new System.Drawing.Point(208, 55);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(128, 39);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // enterNameLabel
            // 
            this.enterNameLabel.AutoSize = true;
            this.enterNameLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.enterNameLabel.Location = new System.Drawing.Point(16, 18);
            this.enterNameLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.enterNameLabel.Name = "enterNameLabel";
            this.enterNameLabel.Size = new System.Drawing.Size(87, 17);
            this.enterNameLabel.TabIndex = 2;
            this.enterNameLabel.Text = "Enter Name:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.nameTextBox.Location = new System.Drawing.Point(140, 15);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.nameTextBox.MaxLength = 20;
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(331, 22);
            this.nameTextBox.TabIndex = 3;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // reservedLabel
            // 
            this.reservedLabel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.reservedLabel.ForeColor = System.Drawing.Color.Red;
            this.reservedLabel.Location = new System.Drawing.Point(16, 59);
            this.reservedLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.reservedLabel.Name = "reservedLabel";
            this.reservedLabel.Size = new System.Drawing.Size(164, 21);
            this.reservedLabel.TabIndex = 4;
            this.reservedLabel.Text = "Name is Reserved";
            this.reservedLabel.Visible = false;
            this.reservedLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RequestAssociativeAlloc);
            // 
            // warningImage
            // 
            this.warningImage.Image = ((System.Drawing.Image)(resources.GetObject("warningImage.Image")));
            this.warningImage.Location = new System.Drawing.Point(140, 55);
            this.warningImage.Margin = new System.Windows.Forms.Padding(4);
            this.warningImage.Name = "warningImage";
            this.warningImage.Size = new System.Drawing.Size(60, 39);
            this.warningImage.TabIndex = 5;
            this.warningImage.TabStop = false;
            this.warningImage.Visible = false;
            // 
            // EnterNameDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(489, 113);
            this.Controls.Add(this.warningImage);
            this.Controls.Add(this.reservedLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.enterNameLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "EnterNameDialog";
            this.Text = "Enter Property Set Name";
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label enterNameLabel;
        public System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label reservedLabel;
        private System.Windows.Forms.PictureBox warningImage;
    }
}