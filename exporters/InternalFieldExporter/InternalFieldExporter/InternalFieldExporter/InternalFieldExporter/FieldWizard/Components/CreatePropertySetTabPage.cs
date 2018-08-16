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
    public partial class CreatePropertySetTabPage : TabPage
    {
        /// <summary>
        /// The parent PhysicsGroupsTabControl.
        /// </summary>
        public PropertySetsTabControl parentTabControl
        {
            get;
            private set;
        }

        /// <summary>
        /// The child CreatePhysicsGroupForm.
        /// </summary>
        private CreatePropertySetForm childForm;

        /// <summary>
        /// Initializes a new instance of the CreatePropertySetTabPage
        /// </summary>
        /// <param name="physicsGroupsTabControl"></param>
        /// <param name="name"></param>
        public CreatePropertySetTabPage(PropertySetsTabControl physicsGroupsTabControl, string name)
        {
            InitializeComponent();

            parentTabControl = physicsGroupsTabControl;

            childForm = new CreatePropertySetForm(this);
            Controls.Add(childForm);

            Text = Name = name;
        }
    }
}
