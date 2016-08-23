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
    // contains the sphere collider options for the component props form
    public partial class SphereColliderPropertiesForm : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the SphereColliderPropertiesForm class.
        /// </summary>
        FieldDataComponent field;
        public SphereColliderPropertiesForm()
        {
            InitializeComponent();// inits and populates the form
        }
        // this method reacts to changes in the fields so we can save the data
        public void scaleChanged(object sender, EventArgs e)
        {
            field.Scale = (double)scaleUpDown.Value;
        }
        public void readFromData(FieldDataComponent d)
        {// reads from the data so user can see the same values from the last time they entered them
            field = d;
            scaleUpDown.Value = (decimal)field.Scale;
        }
    }
}
