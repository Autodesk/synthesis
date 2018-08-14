using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.FieldWizard.Components
{
    public partial class BoxColliderPropertiesForm : UserControl, ColliderPropertiesForm
    {
        /// <summary>
        /// Inits a new instance of the BoxColliderProperitesform class
        /// </summary>
        public BoxColliderPropertiesForm()
        {
            InitializeComponent();
        }

        PropertySet.PropertySetCollider ColliderPropertiesForm.GetCollider()
        {
            return new PropertySet.BoxCollider(new BXDVector3(
                (float)xScaleUpDown.Value,
                (float)yScaleUpDown.Value,
                (float)zScaleUpDown.Value));
        }
    }
}
