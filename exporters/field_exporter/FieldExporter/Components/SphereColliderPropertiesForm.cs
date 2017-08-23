using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class SphereColliderPropertiesForm : UserControl, ColliderPropertiesForm
    {
        /// <summary>
        /// Initializes a new instance of the SphereColliderPropertiesForm class.
        /// </summary>
        public SphereColliderPropertiesForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used for getting a collider from information contained in the SphereColliderPropertiesForm.
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider ColliderPropertiesForm.GetCollider()
        {
            return new PropertySet.SphereCollider((float)scaleUpDown.Value);
        }
    }
}
