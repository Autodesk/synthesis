using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.FieldWizard
{
    public partial class FieldSelectForm : Form
    {

        /// <summary>
        /// Used for accessing the selected field.
        /// </summary>
        public string SelectedField
        {
            get
            {
                return AssemblyListBox.SelectedItem.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the FieldSelectForm
        /// </summary>
        public FieldSelectForm()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Adds a list of visible Assemblies to the assemblyListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldSelectForm_Load(object sender, EventArgs e)
        {
            foreach (Inventor.AssemblyDocument doc in Program.INVENTOR_APPLICATION.Documents.VisibleDocuments.OfType<Inventor.AssemblyDocument>())
            {
                AssemblyListBox.Items.Add(doc.DisplayName);
            }
        }

        /// <summary>
        /// Enables or disables the OK button depending on the selected index.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void assemblyListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (AssemblyListBox.SelectedIndex == -1)
            {
                Ok_Button.Enabled = false;
            }
            else
            {
                Ok_Button.Enabled = true;
            }
        }


        private void Ok_Button_Click(object sender, EventArgs e)
        {
            Program.StartMainWindow();
            this.Visible = false;
        }

    }
}
