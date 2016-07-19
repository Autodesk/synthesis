using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDFieldExporter
{
    
    public partial class MeshColliderPropertiesForm : UserControl
    {
        FieldDataType field;
        /// <summary>
        /// Initializes a new instance of the MeshColliderPropertiesForm class.
        /// </summary>
        public MeshColliderPropertiesForm()
        {
            InitializeComponent();

            infoPictureBox.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            infoToolTip.SetToolTip(infoPictureBox, "Export may take longer when using mesh colliders.");
        }
        public void readFromData(FieldDataType d)
        {
            field = d;
        }
        /// <summary>
        /// Used for getting a collider from information contained in the MeshColliderPropertiesForm.
        /// </summary>
        /// <returns></returns
    }
}
