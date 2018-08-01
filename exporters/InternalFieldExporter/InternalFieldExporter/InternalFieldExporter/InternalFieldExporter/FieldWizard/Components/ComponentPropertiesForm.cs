using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

namespace InternalFieldExporter.FieldWizard
{
    public partial class ComponentPropertiesForm : UserControl
    {
        /// <summary>
        /// parent ComponentPropertiesTabPage
        /// </summary>
        public ComponentPropertiesTabPage ParentTabPage
        {
            get;
            private set;
        }

        /// <summary>
        /// the events caused by the user interaction with Inventor
        /// </summary>
        public InteractionEvents InteractionEvents
        {
            get;
            private set;
        }

        /// <summary>
        /// The events triggered by object selection in Inventor
        /// </summary>
        public SelectEvents SelectEvents
        {
            get;
            private set;
        }

        /// <summary>
        /// determines if Inventor Interaction is enabled
        /// </summary>
        public bool InteractionEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new ComponentPropertiesForm classd
        /// </summary>
        public ComponentPropertiesForm()
        {
            InitializeComponent();

            Dock = DockStyle.Fill; ;

            ParentTabPage = TabPage;

            colliderTypecombobox.SelectedIndex = 0

            InteractionEnabled = false;
        }

        public PropertySet.PropertySetCollider GetSetCollider()
        {
            return((colliderPropertiesForm)meshPropertiesTale.Controls[1]).GetCollider();
        }
    }
}
