using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// This exists to be attached to GopherAttribute objects
    /// </summary>
    public class ColliderMeta
    {
        public AttribType Type;
        /// <summary>
        /// AttributeID is the ID that will be referenced by the Field/Robot class when getting Colliders for an object
        /// </summary>
        public UInt32 AttributeID;
    }
}
