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
    public partial class MeshColliderPropertiesForm : UserControl , ColliderPropertiesForm
    {
        /// <summary>
        /// Initializes a new instance of the MeshColliderPropertiesForm class
        /// </summary>
        public MeshColliderPropertiesForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// gets a collider from information contained in the MeshColliderPropertiesForm
        /// </summary>
        /// <returns>A new MeshCollider</returns>
        PropertySet.PropertySetCollider ColliderPropertiesForm.GetCollider()
        {
            return new PropertySet.MeshCollider(convexCheckBox.Checked);
        }
    }
}
