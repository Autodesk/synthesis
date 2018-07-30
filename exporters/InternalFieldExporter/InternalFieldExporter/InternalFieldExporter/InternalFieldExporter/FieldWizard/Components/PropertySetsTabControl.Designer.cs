namespace FieldExporter.Components
{
    partial class PropertySetsTabControl
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
            this.SuspendLayout();
            // 
            // PropertySetsTabControl
            // 
            this.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.PropertySetsTabControl_Selecting);
            this.Deselecting += new System.Windows.Forms.TabControlCancelEventHandler(this.PropertySetsTabControl_Deselecting);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PropertySetsTabControl_MouseClick);
            this.ResumeLayout(false);

        }

        #endregion

    }
}
