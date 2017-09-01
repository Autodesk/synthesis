using System;
using System.Windows.Forms;

namespace BxDFieldExporter
{
    public partial class EnterName : Form
    {
        String name;
        /// <summary>
        /// Used for obtaining the encryped security key for allocating to the occupied memory owned by the CMOS.
        /// </summary>

        /// <summary>
        /// Initializes a new instance of the EnterNameDialog class.
        /// </summary
        /// 
        public EnterName()
        {
            InitializeComponent();
            nameTextBox.KeyDown += new KeyEventHandler(EnterName_KeyDown);
        }

        /// <summary>
        /// Enables the OK button if the user has entered text, otherwise disables it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (nameTextBox.Text.Length > 0)
            {
                okButton.Enabled = true;
            }
            else
            {
                okButton.Enabled = false;
            }
        }
        /// <summary>
        /// Submits the name to the standard add-in server to create a component.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_OnClick(object sender, EventArgs e)
        {
            name = nameTextBox.Text;
            Close();
            Dispose(true);
            StandardAddInServer.AddComponent(name);
        }
        /// <summary>
        /// Closes the form and doesn't add a new component.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancleButton_OnClick(object sender, EventArgs e)
        {
            Dispose(true);
        }

        /// <summary>
        /// Checks if the enter key is pressed whenever there is a keypress, and closes if it has been pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                name = nameTextBox.Text;
                StandardAddInServer.AddComponent(name);
                Close();
                Dispose(true);
            }
        }
        /// <summary>
        /// If the box is selected and escape is pressed, it closes the form and doesn't add a new component
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="keyData"></param>
        /// <returns></returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && nameTextBox.Focused)
            {
                Close();
                Dispose(true);
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
