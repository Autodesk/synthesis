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
    public partial class SphereColliderPropertiesForm : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the SphereColliderPropertiesForm class.
        /// </summary>
        FieldDataType field;
        public SphereColliderPropertiesForm()
        {
            InitializeComponent();
        }
        public void scaleChanged(object sender, EventArgs e)
        {
            field.Scale = (double)scaleUpDown.Value;
        }
        public void readFromData(FieldDataType d)
        {
            field = d;
            scaleUpDown.Value = (decimal)field.Scale;
        }
    }
}
