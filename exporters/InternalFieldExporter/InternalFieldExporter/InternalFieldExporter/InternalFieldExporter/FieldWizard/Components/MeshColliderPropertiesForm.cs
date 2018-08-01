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
    public partial class MeshColliderPropertiesForm : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the MeshColliderPropertiesForm class
        /// </summary>
        public MeshColliderPropertiesForm()
        {
            InitializeComponent();

            infoPictureBox.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            infoTooltip.SetToolTip(infoPictureBox, "Export may take longer when using mesh colliders");
        }

        /// <summary>
        /// gets a collider form information contained in the MeshColliderPropertiesForm
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider ColliderPropertiesForm.GetCollider()
        {
            return new PropertySet.MeshCollider(convexCheckBox.Checked);
        }
    }
}
