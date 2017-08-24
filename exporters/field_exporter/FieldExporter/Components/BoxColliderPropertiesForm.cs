using System.Windows.Forms;

namespace FieldExporter.Components
{
    public partial class BoxColliderPropertiesForm : UserControl, ColliderPropertiesForm
    {
        /// <summary>
        /// Initailizes a new instance of the BoxColliderPropertiesFormClass.
        /// </summary>
        public BoxColliderPropertiesForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Used for getting a collider from information contained in the BoxColliderPropertiesForm.
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider ColliderPropertiesForm.GetCollider()
        {
            return new PropertySet.BoxCollider(new BXDVector3(
                (float)xScaleUpDown.Value,
                (float)yScaleUpDown.Value,
                (float)zScaleUpDown.Value));
        }
    }
}
