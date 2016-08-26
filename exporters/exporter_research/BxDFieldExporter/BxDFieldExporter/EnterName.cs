using System;
using System.Windows.Forms;

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

        }

        /// <summary>
        /// Enables the OK button if the user has entered text, otherwise disables it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nameTextBox_TextChanged(object sender, EventArgs e) {
            if (nameTextBox.Text.Length > 0) {
                okButton.Enabled = true;
            }
            else {
                okButton.Enabled = false;
            }
        }
        private void OKButton_OnClick(object sender, EventArgs e) {
            name = nameTextBox.Text;
            this.Close();
            this.Dispose(true);
            StandardAddInServer.addComponent(name);
        }
        private void CancleButton_OnClick(object sender, EventArgs e) {
            this.Dispose(true);
        }
    }
}
