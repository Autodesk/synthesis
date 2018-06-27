namespace EditorsLibrary
{
    partial class BXDAEditorForm
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
            this.fieldName = new System.Windows.Forms.TextBox();
            this.valueGen = new System.Windows.Forms.TextBox();
            this.valueX = new System.Windows.Forms.TextBox();
            this.valueZ = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // fieldName
            // 
            this.fieldName.Location = new System.Drawing.Point(9, 69);
            this.fieldName.Margin = new System.Windows.Forms.Padding(2);
            this.fieldName.Name = "fieldName";
            this.fieldName.ReadOnly = true;
            this.fieldName.Size = new System.Drawing.Size(90, 20);
            this.fieldName.TabIndex = 0;
            // 
            // valueGen
            // 
            this.valueGen.Location = new System.Drawing.Point(111, 69);
            this.valueGen.Margin = new System.Windows.Forms.Padding(2);
            this.valueGen.Name = "valueGen";
            this.valueGen.Size = new System.Drawing.Size(92, 20);
            this.valueGen.TabIndex = 1;
            // 
            // valueX
            // 
            this.valueX.Location = new System.Drawing.Point(111, 46);
            this.valueX.Margin = new System.Windows.Forms.Padding(2);
            this.valueX.Name = "valueX";
            this.valueX.Size = new System.Drawing.Size(92, 20);
            this.valueX.TabIndex = 2;
            // 
            // valueZ
            // 
            this.valueZ.Location = new System.Drawing.Point(111, 92);
            this.valueZ.Margin = new System.Windows.Forms.Padding(2);
            this.valueZ.Name = "valueZ";
            this.valueZ.Size = new System.Drawing.Size(92, 20);
            this.valueZ.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 53);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Field Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(111, 28);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Value(s)";
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(9, 154);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(2);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(89, 23);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(111, 154);
            this.buttonOK.Margin = new System.Windows.Forms.Padding(2);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(92, 23);
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // BXDAEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(212, 186);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.valueZ);
            this.Controls.Add(this.valueX);
            this.Controls.Add(this.valueGen);
            this.Controls.Add(this.fieldName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BXDAEditorForm";
            this.ShowIcon = false;
            this.Text = "Value Editor";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox fieldName;
        private System.Windows.Forms.TextBox valueGen;
        private System.Windows.Forms.TextBox valueX;
        private System.Windows.Forms.TextBox valueZ;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
    }
}