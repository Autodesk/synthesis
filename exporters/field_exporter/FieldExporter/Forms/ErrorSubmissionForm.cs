using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class ErrorSubmissionForm : Form
    {
        /// <summary>
        /// The error supplied by the program.
        /// </summary>
        private string error;

        /// <summary>
        /// Initializes a new instance of the ErrorSubmissionForm class.
        /// </summary>
        /// <param name="error"></param>
        public ErrorSubmissionForm(string error)
        {
            InitializeComponent();
            this.error = error;
        }

        /// <summary>
        /// Submits the report if the email address is valid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submitButton_Click(object sender, EventArgs e)
        {
            if (Regex.IsMatch(emailTextBox.Text, "^([\\w\\.\\-]+)@([\\w\\-]+)((\\.(\\w){2,3})+)$"))
            {
                MailMessage message = new MailMessage(emailTextBox.Text, "mackinnon.buck@autodesk.com");
                message.Subject = "Field Exporter Error Submission";
                message.Body =
                    "A user has encountered the following error in the Synthesis Field Exporter:\n\n" +
                    error +
                    (descriptionBox.Text.Length > 0 ?
                    "\n\nThe user's actions before the error occurred:\n\n" +
                    descriptionBox.Text + "\n\n" :
                    "\n\nThe user failed to provide a list of actions taken before the error occurred.");

                SmtpClient client = new SmtpClient("smtp.autodesk.com", 25);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Send(message);

                MessageBox.Show("Form has been successfully submitted.", "Success.");

                Close();
            }
            else
            {
                MessageBox.Show("Could not submit error form.", "Invalid email address.");
            }
        }

        /// <summary>
        /// Closes the form without submitting an error report.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Form was not submitted.", "Submission cancelled.");
            Close();
        }
    }
}
