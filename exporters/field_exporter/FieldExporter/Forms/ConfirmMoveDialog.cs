using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class ConfirmMoveDialog : Form
    {
        /// <summary>
        /// Initializes a new instance of the ConfirmMoveDialog class with the supplied message.
        /// </summary>
        /// <param name="message"></param>
        public ConfirmMoveDialog(string message)
        {
            InitializeComponent();

            messageLabel.Text = message;
        }

        /// <summary>
        /// Returns true if the check box is checked.
        /// </summary>
        /// <returns></returns>
        public bool IsChecked()
        {
            return futureCheckBox.Checked;
        }
    }
}
