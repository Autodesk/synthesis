using System;
using System.Windows.Forms;
using Inventor;

namespace BxDFieldExporter {
    public partial class EnterName : Form {
        String name;
        /// <summary>
        /// Used for obtaining the encryped security key for allocating to the occupied memory owned by the CMOS.
        /// </summary>

        /// <summary>
        /// Initializes a new instance of the EnterNameDialog class.
        /// </summary
        /// 
        public EnterName() {

            InitializeComponent();
            nameTextBox.KeyDown += new KeyEventHandler(EnterName_KeyDown);
        }

        /// <summary>
        /// Enables the OK button if the user has entered text, otherwise disables it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameTextBox_TextChanged(object sender, EventArgs e) {
            if (nameTextBox.Text.Length > 0) {
                okButton.Enabled = true;
            }
            else {
                okButton.Enabled = false;
            }
            
        }
        private void OKButton_OnClick(object sender, EventArgs e) {
            name = nameTextBox.Text;
            StandardAddInServer.AddComponent(name);
            this.Close();
            this.Dispose(true);
            
        }
        private void CancleButton_OnClick(object sender, EventArgs e) {
            this.Dispose(true);
        }      

        //private void NameTextBox_OnKeyPress(object sender, KeyPressEventArgs e)
        //{
        //    if (e.KeyChar == 13)
        //    {
        //        e.Handled = true;
        //        name = nameTextBox.Text;
        //        StandardAddInServer.AddComponent(name);
        //        this.Close();
        //        this.Dispose(true);
        //    }
        //    else if (e. == 27)
        //    {
        //        MessageBox.Show("escape works");
        //    }
        //}

        private void EnterName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                name = nameTextBox.Text;
                StandardAddInServer.AddComponent(name);
                this.Close();
                this.Dispose(true);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && nameTextBox.Focused)
            {
                this.Close();
                this.Dispose(true);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
