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
            this.okButton.Location = new System.Drawing.Point(258, 45);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(96, 32);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(156, 45);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(96, 32);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // enterNameLabel
            // 
            this.enterNameLabel.AutoSize = true;
            this.enterNameLabel.Location = new System.Drawing.Point(12, 15);
            this.enterNameLabel.Name = "enterNameLabel";
            this.enterNameLabel.Size = new System.Drawing.Size(66, 13);
            this.enterNameLabel.TabIndex = 2;
            this.enterNameLabel.Text = "Enter Name:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(105, 12);
            this.nameTextBox.MaxLength = 20;
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(249, 20);
            this.nameTextBox.TabIndex = 3;
            // 
            // reservedLabel
            // 
            this.reservedLabel.ForeColor = System.Drawing.Color.Red;
            this.reservedLabel.Location = new System.Drawing.Point(12, 48);
            this.reservedLabel.Name = "reservedLabel";
            this.reservedLabel.Size = new System.Drawing.Size(123, 17);
            this.reservedLabel.TabIndex = 4;
            this.reservedLabel.Text = "Name is Reserved";
            // 
            // warningImage
            // 
            this.warningImage.Image = ((System.Drawing.Image)(resources.GetObject("warningImage.Image")));
            this.warningImage.Location = new System.Drawing.Point(105, 45);
            this.warningImage.Name = "warningImage";
            this.warningImage.Size = new System.Drawing.Size(45, 32);
            this.warningImage.TabIndex = 5;
            this.warningImage.TabStop = false;
            // 
            // EnterNameDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(367, 92);
            this.Controls.Add(this.warningImage);
            this.Controls.Add(this.reservedLabel);
            this.Controls.Add(this.nameTextBox);
            this.Controls.Add(this.enterNameLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
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