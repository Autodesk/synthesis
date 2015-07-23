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
        public PhysicsGroupsTabControl parentTabControl
        {
            get;
            private set;
        }

        private CreatePhysicsGroupForm childForm;

        private ImageList imageList;

        public CreatePhysicsGroupTabPage(PhysicsGroupsTabControl physicsGroupsTabControl,
            string name, Image image)
        {
            InitializeComponent();

            parentTabControl = physicsGroupsTabControl;

            childForm = new CreatePhysicsGroupForm(this);
            Controls.Add(childForm);

            Text = Name = name;

            imageList = new ImageList();
            imageList.Images.Add(image);
            ImageIndex = 0;
        }

        
    }
}
