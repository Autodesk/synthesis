using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class MeshColliderPropertiesForm : UserControl, ColliderPropertiesForm
    {
        /// <summary>
        /// Initializes a new instance of the MeshColliderPropertiesForm class.
        /// </summary>
        public MeshColliderPropertiesForm()
        {
            InitializeComponent();

            infoPictureBox.Image = System.Drawing.SystemIcons.Information.ToBitmap();
            infoToolTip.SetToolTip(infoPictureBox, "Export may take longer when using mesh colliders.");
        }

        /// <summary>
        /// Used for getting a collider from information contained in the MeshColliderPropertiesForm.
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider ColliderPropertiesForm.Collider
        {
            get => new PropertySet.MeshCollider(convexCheckBox.Checked);

            set
            {
                if (value is PropertySet.MeshCollider mesh)
                    convexCheckBox.Checked = mesh.Convex;
            }
        }
    }
}
