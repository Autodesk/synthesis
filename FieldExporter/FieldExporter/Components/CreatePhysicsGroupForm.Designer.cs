namespace FieldExporter.Components
{
    partial class CreatePhysicsGroupForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreatePhysicsGroupForm));
            this.createButton = new System.Windows.Forms.Button();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.createLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // createButton
            // 
            this.createButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.createButton.ImageKey = "AddIcon.png";
            this.createButton.ImageList = this.imageList;
            this.createButton.Location = new System.Drawing.Point(268, 175);
            this.createButton.Name = "createButton";
            this.createButton.Size = new System.Drawing.Size(64, 40);
            this.createButton.TabIndex = 3;
            this.createButton.Text = "Create";
            this.createButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.createButton.UseVisualStyleBackColor = true;
            this.createButton.Click += new System.EventHandler(this.newPhysicsButton_Click);
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "AddIcon.png");
            // 
            // createLabel
            // 
            this.createLabel.AutoSize = true;
            this.createLabel.Location = new System.Drawing.Point(152, 155);
            this.createLabel.Name = "createLabel";
            this.createLabel.Size = new System.Drawing.Size(297, 17);
            this.createLabel.TabIndex = 2;
            this.createLabel.Text = "You have not yet created any PhysicsGroups.";
            // 
            // CreatePhysicsGroupForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.createButton);
            this.Controls.Add(this.createLabel);
            this.Name = "CreatePhysicsGroupForm";
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Size = new System.Drawing.Size(600, 371);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createButton;
        private System.Windows.Forms.Label createLabel;
        private System.Windows.Forms.ImageList imageList;
    }
}
