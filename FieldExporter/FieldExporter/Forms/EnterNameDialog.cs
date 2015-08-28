using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class EnterNameDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the EnterNameDialog class.
        /// </summary>
        public EnterNameDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Enables the OK button if the user has entered text, otherwise disables it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nameTextBox_TextChanged(object sender, EventArgs e)
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
    }
}
