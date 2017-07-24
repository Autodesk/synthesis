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
        protected float friction;
        protected bool isDynamic;
        protected float mass = 0;

        public ColliderMeta Meta;

        /// <summary>
        /// The friction value of the NodeAttribute
        /// </summary>
        public float Friction => friction;
        /// <summary>
        /// Bool that states whether the Node is dynamic
        /// </summary>
        public bool IsDynamic => isDynamic;
        /// <summary>
        /// If the object is dynamic, this will be the mass of the object. This value defaults to 0 because nullables are gross
        /// </summary>
        public float Mass => mass;

        public virtual AttribType GetAttribType()
        {
            return AttribType.NO_COLLIDER;
        }

        /// <summary>
        /// A list containing all of the Nodes this attribute is attached to
        /// </summary>
        public List<GopherFieldNode> Nephews { get; }
    }
}
