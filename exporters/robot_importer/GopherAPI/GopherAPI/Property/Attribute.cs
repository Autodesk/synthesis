using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace GopherAPI.Properties
{
    /// <summary>
    /// Do you really need docs for this?
    /// </summary>
    public enum AttribType { BOX_COLLIDER, SPHERE_COLLIDER, MESH_COLLIDER }

    /// <summary>
    /// This struct will store all special attribute information except color, which is stored in meshes
    /// </summary>
    public struct Attribute
    {
        public readonly AttribType Type;
        /// <summary>
        /// AttributeID is the ID of the attribute itself for fields, but for robots it is the ID of the STL mesh it is attatched to.
        /// </summary>
        public readonly UInt32 AttributeID;
        public readonly float Friction;
        public readonly bool IsDynamic;
        public readonly float? Mass;

        /// <summary>
        /// For Box Colliders
        /// </summary>
        public readonly float? XScale;
        /// <summary>
        /// For Box Colliders
        /// </summary>
        public readonly float? YScale;
        /// <summary>
        /// For Box Colliders
        /// </summary>
        public readonly float? ZScale;
        /// <summary>
        /// For Sphere Colliders
        /// </summary>
        public readonly float? GScale;

        /// <summary>
        /// Note: mass, xScale, yScale, zScale, and gScale are nullable, but an exception will be thrown if they are null and values are expected. 
        /// Also, AttributeID is the ID of the attribute itself for fields, but for robots it is the ID of the STL mesh it is attatched to.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributeID"></param>
        /// <param name="friction"></param>
        /// <param name="isDynamic"></param>
        /// <param name="mass"></param>
        /// <param name="xScale"></param>
        /// <param name="yScale"></param>
        /// <param name="zScale"></param>
        /// <param name="gScale"></param>
        public Attribute(AttribType type, UInt32 attributeID, float friction, bool isDynamic, float? mass, float? xScale, float? yScale, float? zScale, float? gScale)
        {
            Type = type;
            AttributeID = attributeID;
            Friction = friction;
            IsDynamic = isDynamic;
            if(IsDynamic)
                Mass = mass;
            else
                Mass = null;

            switch (Type)
            {
                case AttribType.BOX_COLLIDER:
                    if (xScale == null)
                        throw new Exception("ERROR: Value expected for xScale");
                    if (yScale == null)
                        throw new Exception("ERROR: Value expected for yScale");
                    if (zScale == null)
                        throw new Exception("ERROR: Value expected for zScale");

                    XScale = xScale;
                    YScale = yScale;
                    ZScale = zScale;
                    GScale = null;
                    break;
                case AttribType.SPHERE_COLLIDER:
                    if (gScale == null)
                        throw new Exception("ERROR: Value expected for gScale");
                    XScale = null;
                    YScale = null;
                    ZScale = null;
                    GScale = gScale;
                    break;
                case AttribType.MESH_COLLIDER:
                    XScale = null;
                    YScale = null;
                    ZScale = null;
                    GScale = null;
                    break;
                default:
                    throw new Exception("ERROR: Type undefined or null or something. Ya dun goofed");
            }
        }
    }
}
