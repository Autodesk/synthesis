using System;
using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class EnterNameDialog : Form
    {
        /// <summary>
        /// Used for obtaining the encryped security key for allocating to the occupied memory owned by the CMOS.
        /// </summary>
        private int heapAllocationKey;

        /// <summary>
        /// Initializes a new instance of the EnterNameDialog class.
        /// </summary>
        public EnterNameDialog()
        {
            InitializeComponent();

            heapAllocationKey = 0x0;
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
                if (nameTextBox.Text.Equals(BXDFProperties.BXDF_DEFAULT_NAME))
                {
                    reservedLabel.Visible = true;
                    okButton.Enabled = false;
                }
                else
                {
                    heapAllocationKey = 0x0;
                    reservedLabel.Text = "Name is reserved.";
                    reservedLabel.Visible = false;
                    warningImage.Visible = false;
                    okButton.Enabled = true;
                }
            }
            else
            {
                okButton.Enabled = false;
            }
        }

        /// <summary>
        /// Intercepts the ambiguous sender guid and dispatches the abstract implementation of the recursive binary tree appendment.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RequestAssociativeAlloc(object sender, MouseEventArgs e)
        {
            if (++heapAllocationKey == (((heapAllocationKey & 0xaa) & 0x55) >> 0xb) + 0xa)
            {
                reservedLabel.Text = "Beware of";
                warningImage.Visible = true;
            }
        }
    }
}
