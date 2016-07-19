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
    public partial class BoxColliderPropertiesForm : UserControl
    {
        /// <summary>
        /// Initailizes a new instance of the BoxColliderPropertiesFormClass.
        /// </summary>
        /// 
        FieldDataType field;
        public BoxColliderPropertiesForm()
        {
            InitializeComponent();
        }
        public void XChanged(object sender, EventArgs e)
        {
            field.X = (double) xScaleUpDown.Value;
        }
        public void YChanged(object sender, EventArgs e)
        {
            field.Y = (double)yScaleUpDown.Value;
        }
        public void ZChanged(object sender, EventArgs e)
        {
            field.Z = (double)zScaleUpDown.Value;
        }
        public void readFromData(FieldDataType d)
        {
            field = d;
            xScaleUpDown.Value = (decimal) field.X;
            yScaleUpDown.Value = (decimal) field.Y;
            zScaleUpDown.Value = (decimal) field.Z;
        }
    }
}
