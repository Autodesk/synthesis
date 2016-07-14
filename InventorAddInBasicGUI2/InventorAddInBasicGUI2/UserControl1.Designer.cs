using System.ComponentModel;
using stdole;
using System.Windows.Forms;

namespace InventorAddInBasicGUI2
{
    partial class UserControl1
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
        public void InitializeComponent()
        {
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog(); saveFileDialog1.AddExtension = true;
            this.saveFileDialog1.FileName = "untitled.robot";
            this.saveFileDialog1.Filter = "ROBOT files (*.robot)|*.robot";
            this.saveFileDialog1.DefaultExt = "robot";
            this.saveFileDialog1.FileOk += new CancelEventHandler(saveFileDialog1_FileOk); 
        }
        private void saveFileDialog1_FileOk(object sender,
    System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("asdasasd");
        }
        #endregion

        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}
