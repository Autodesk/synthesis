namespace FieldExporter.Components
{
    partial class PhysicsGroupsTabControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PhysicsGroupsTabControl));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "CreateImage.png");
            // 
            // PhysicsGroupsTabControl
            // 
            this.ImageList = this.imageList;
            this.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.PhysicsGroupsTabControl_Selecting);
            this.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.PhysicsGroupsTabControl_Deselecting);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PhysicsGroupsTabControl_MouseClick);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList;
    }
}
