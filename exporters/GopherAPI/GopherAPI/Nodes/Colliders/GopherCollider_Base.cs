using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GopherAPI.Nodes.Colliders
{
    /// <summary>
    /// The class which the colliders all derive from
    /// </summary>
    public class GopherCollider_Base 
    {
        public ColliderMeta Meta;

        /// <summary>
        /// The friction value of the NodeAttribute
        /// </summary>
        public float Friction { get; internal set; }
        /// <summary>
        /// Bool that states whether the Node is dynamic
        /// </summary>
        public bool IsDynamic { get; internal set; }
        /// <summary>
        /// If the object is dynamic, this will be the mass of the object.
        /// </summary>
        public float Mass { get; internal set; }

        public virtual ColliderType GetAttribType()
        {
            return ColliderType.NO_COLLIDER;
        }

        /// <summary>
        /// A list containing all of the Nodes this attribute is attached to
        /// </summary>
        public List<GopherFieldNode> Nephews { get; }
    }
}
