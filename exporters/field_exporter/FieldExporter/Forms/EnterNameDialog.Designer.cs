namespace FieldExporter.Forms
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
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.enterNameLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.reservedLabel = new System.Windows.Forms.Label();
            this.warningImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).BeginInit();
            this.SuspendLayout();
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(105, 12);
            this.nameTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.nameTextBox.MaxLength = 20;
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(249, 22);
            this.nameTextBox.TabIndex = 0;
            this.nameTextBox.TextChanged += new System.EventHandler(this.nameTextBox_TextChanged);
            // 
            // enterNameLabel
            // 
            this.enterNameLabel.AutoSize = true;
            this.enterNameLabel.Location = new System.Drawing.Point(12, 15);
            this.enterNameLabel.Name = "enterNameLabel";
            this.enterNameLabel.Size = new System.Drawing.Size(87, 17);
            this.enterNameLabel.TabIndex = 1;
            this.enterNameLabel.Text = "Enter Name:";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(156, 39);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(96, 32);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Enabled = false;
            this.okButton.Location = new System.Drawing.Point(258, 39);
            this.okButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(96, 32);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // reservedLabel
            // 
            this.reservedLabel.AutoSize = true;
            this.reservedLabel.ForeColor = System.Drawing.Color.Red;
            this.reservedLabel.Location = new System.Drawing.Point(12, 48);
            this.reservedLabel.Name = "reservedLabel";
            this.reservedLabel.Size = new System.Drawing.Size(123, 17);
            this.reservedLabel.TabIndex = 4;
            this.reservedLabel.Text = "Name is reserved.";
            this.reservedLabel.Visible = false;
            this.reservedLabel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RequestAssociativeAlloc);
            // 
            // warningImage
            // 
            this.warningImage.BackColor = System.Drawing.Color.Transparent;
            this.warningImage.Image = ((System.Drawing.Image)(resources.GetObject("warningImage.Image")));
            this.warningImage.ImageLocation = "";
            this.warningImage.InitialImage = null;
            this.warningImage.Location = new System.Drawing.Point(85, 39);
            this.warningImage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.warningImage.Name = "warningImage";
            this.warningImage.Size = new System.Drawing.Size(64, 32);
            this.warningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
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
            this.ClientSize = new System.Drawing.Size(365, 84);
            this.Controls.Add(this.warningImage);
            this.Controls.Add(this.reservedLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.enterNameLabel);
            this.Controls.Add(this.nameTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterNameDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Enter Property Set Name";
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label enterNameLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        public System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label reservedLabel;
        private System.Windows.Forms.PictureBox warningImage;
    }
}