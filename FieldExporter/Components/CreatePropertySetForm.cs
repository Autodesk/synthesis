using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FieldExporter.Controls;

namespace FieldExporter.Components
{
    public partial class CreatePropertySetForm : UserControl
    {
        /// <summary>
        /// The parent CreatePhysicsGroupTabPage.
        /// </summary>
        private CreatePropertySetTabPage parentTabPage;

        /// <summary>
        /// Initializes a new instance of the CreatePhysicsGroupForm class.
        /// </summary>
        /// <param name="tabPage"></param>
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
