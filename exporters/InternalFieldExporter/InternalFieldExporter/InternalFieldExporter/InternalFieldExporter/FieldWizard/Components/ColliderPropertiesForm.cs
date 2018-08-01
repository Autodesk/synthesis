using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternalFieldExporter.FieldWizard.Components
{
    public interface ColliderPropertiesForm
    {
        /// <summary>
        /// Used to get a PropertySetCollider from information contained in the control
        /// </summary>
        /// <returns></returns>
        PropertySet.PropertySetCollider GetCollider();
    }
}
