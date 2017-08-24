using System;
using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class CrashForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the CrashForm class.
        /// </summary>
        public CrashForm(string error)
        {
            InitializeComponent();
            errorTextBox.Text = error;
        }

        /// <summary>
        /// Copies the error message to the clipboard.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(errorTextBox.Text);
        }

        /// <summary>
        /// Closes the window when the "Close" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
