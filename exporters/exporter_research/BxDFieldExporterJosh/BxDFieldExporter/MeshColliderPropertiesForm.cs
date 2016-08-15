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
    // contains the mesh collider options for the component props form, doesn't actually do anything
    public partial class MeshColliderPropertiesForm : UserControl
    {
        FieldDataType field;// just here incase we ever need to save stuff in here
        /// <summary>
        /// Initializes a new instance of the MeshColliderPropertiesForm class.
        /// </summary>
        public MeshColliderPropertiesForm()
        {
            InitializeComponent();// inits and populates the form

            infoPictureBox.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            infoToolTip.SetToolTip(infoPictureBox, "Export may take longer when using mesh colliders.");
        }
        public void readFromData(FieldDataType d)
        {// reads from the data so user can see the same values from the last time they entered them, just here so the calling methods doesn't throw a fit
            field = d;
        }
        /// <summary>
        /// Used for getting a collider from information contained in the MeshColliderPropertiesForm.
        /// </summary>
        /// <returns></returns
    }
}
