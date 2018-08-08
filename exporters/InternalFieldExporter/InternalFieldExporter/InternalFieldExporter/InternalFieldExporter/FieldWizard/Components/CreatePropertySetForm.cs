using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.FieldWizard
{
    public partial class CreatePropertySetForm : UserControl
    {

        private CreatePropertySetTabPage parentTabPage;

        /// <summary>
        /// Inits a new instance of the CreatePhysicsGroupForm class
        /// </summary>
        public CreatePropertySetForm(CreatePropertySetTabPage tabPage)
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            parentTabPage = tabPage;
        }

        /// <summary>
        /// Adds a ComponentPropertiesTabPage when the "Create" button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newPhysicsButton_Click(object sender, EventArgs e)
        {
            parentTabPage.parentTabControl.AddComponentPropertiesTab();
        }
    }
}
