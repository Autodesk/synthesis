using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class CreatePhysicsGroupTabPage : TabPage
    {
        /// <summary>
        /// The parent PhysicsGroupsTabControl.
        /// </summary>
        public PhysicsGroupsTabControl parentTabControl
        {
            get;
            private set;
        }

        /// <summary>
        /// The child CreatePhysicsGroupForm.
        /// </summary>
        private CreatePhysicsGroupForm childForm;

        /// <summary>
        /// Initializes a new instance of the CreatePhysicsGroupTabPage class.
        /// </summary>
        /// <param name="physicsGroupsTabControl"></param>
        /// <param name="name"></param>
        public CreatePhysicsGroupTabPage(PhysicsGroupsTabControl physicsGroupsTabControl, string name)
        {
            InitializeComponent();

            parentTabControl = physicsGroupsTabControl;

            childForm = new CreatePhysicsGroupForm(this);
            Controls.Add(childForm);

            Text = Name = name;
        }
        
    }
}
