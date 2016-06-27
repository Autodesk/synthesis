using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter.Components
{
    public interface ColliderPropertiesForm
    {
        /// <summary>
        /// Used for getting a PropertySetCollider from information contained in the control.
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider GetCollider();
    }
}
