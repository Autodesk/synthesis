namespace FieldExporter.Forms
{
    partial class UnhandledExceptionForm
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
            this.MessageLabel = new System.Windows.Forms.Label();
            this.ErrorBox = new System.Windows.Forms.RichTextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // MessageLabel
            // 
            this.MessageLabel.Location = new System.Drawing.Point(12, 9);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(342, 70);
            this.MessageLabel.TabIndex = 0;
            this.MessageLabel.Text = "Please contact the BXD team at bxd@autodesk.com with a detailed list of actions t" +
    "aken before the bug occurred, along with the following information:";
            // 
            // ErrorBox
            // 
            this.ErrorBox.Location = new System.Drawing.Point(12, 82);
            this.ErrorBox.Name = "ErrorBox";
            this.ErrorBox.ReadOnly = true;
            this.ErrorBox.Size = new System.Drawing.Size(342, 202);
            this.ErrorBox.TabIndex = 1;
            this.ErrorBox.Text = "";
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(258, 295);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(96, 32);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // UnhandledExceptionForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(366, 339);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ErrorBox);
            this.Controls.Add(this.MessageLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "UnhandledExceptionForm";
            this.Text = "A fatal error has occurred.";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.RichTextBox ErrorBox;
        private System.Windows.Forms.Button OKButton;
    }
}