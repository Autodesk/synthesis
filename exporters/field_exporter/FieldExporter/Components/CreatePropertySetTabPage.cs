using System.Windows.Forms;

namespace FieldExporter.Components
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
        /// Initializes a new instance of the CreatePhysicsGroupTabPage class.
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
