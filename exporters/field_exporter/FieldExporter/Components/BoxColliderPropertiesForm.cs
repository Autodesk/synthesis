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
        /// Used for getting/setting a collider from information contained in the BoxColliderPropertiesForm.
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider ColliderPropertiesForm.Collider
        {
            get => new PropertySet.BoxCollider(new BXDVector3((float)xScaleUpDown.Value,
                                                              (float)yScaleUpDown.Value,
                                                              (float)zScaleUpDown.Value));

            set
            {
                if (value is PropertySet.BoxCollider box)
                {
                    xScaleUpDown.Value = (decimal)box.Scale.x;
                    yScaleUpDown.Value = (decimal)box.Scale.y;
                    zScaleUpDown.Value = (decimal)box.Scale.z;
                }
            }
        }
    }
}
