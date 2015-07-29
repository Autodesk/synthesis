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
    public partial class CreatePhysicsGroupForm : UserControl
    {
        private CreatePhysicsGroupTabPage parentTabPage;

        public CreatePhysicsGroupForm(CreatePhysicsGroupTabPage tabPage)
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            parentTabPage = tabPage;
        }

        private void newPhysicsButton_Click(object sender, EventArgs e)
        {
            parentTabPage.parentTabControl.AddComponentPropertiesTab();
        }
    }
}
