namespace FieldExporter.Components
{
    partial class CreatePropertySetForm
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.createButton = new System.Windows.Forms.Button();
            this.createLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // createButton
            // 
            this.createButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.createButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.createButton.ImageKey = "CreateImage.png";
            this.createButton.Location = new System.Drawing.Point(268, 190);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(64, 23);
            this.createButton.TabIndex = 3;
            this.createButton.Text = "Create";
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.newPhysicsButton_Click);
            // 
            // createLabel
            // 
            this.createLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.createLabel.BackColor = System.Drawing.Color.Transparent;
            this.createLabel.Location = new System.Drawing.Point(6, 165);
            this.createLabel.Name = "createLabel";
            this.createLabel.Size = new System.Drawing.Size(588, 22);
            this.createLabel.TabIndex = 2;
            this.createLabel.Text = "You have not yet created any Property Sets.";
            this.createLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // CreatePropertySetForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.createLabel);
            this.DoubleBuffered = true;
            this.Name = "CreatePropertySetForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(600, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Label createLabel;
    }
}
