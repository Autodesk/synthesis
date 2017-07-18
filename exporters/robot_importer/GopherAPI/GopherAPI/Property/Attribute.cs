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
    public struct STLAttribute
    {
        public readonly AttribType Type;
        /// <summary>
        /// AttributeID is the ID that will be referenced by the Field/Robot class when getting attributes for an object
        /// </summary>
        public readonly UInt32 AttributeID;
        public readonly float Friction;
        public readonly bool IsDynamic;
        /// <summary>
        /// Should be null unless IsDynamic is true
        /// </summary>
        private readonly float? mass;

        /// <summary>
        /// For Box Colliders
        /// </summary>
        private readonly float? xScale;
        /// <summary>
        /// For Box Colliders
        /// </summary>
        private readonly float? yScale;
        /// <summary>
        /// For Box Colliders
        /// </summary>
        private readonly float? zScale;
        /// <summary>
        /// For Sphere Colliders
        /// </summary>
        private readonly float? gScale;

        #region Properties
        public float XScale
        {
            get
            {
                if (xScale == null)
                    throw new Exception("ERROR: xScale is null");
                return (float)xScale;
            }
        }
        public float YScale
        {
            get
            {
                if (yScale == null)
                    throw new Exception("ERROR: yScale is null");
                return (float)yScale;
            }
        }
        public float ZScale
        {
            get
            {
                if (zScale == null)
                    throw new Exception("ERROR: zScale is null");
                return (float)zScale;
            }
        }
        public float GScale
        {
            get
            {
                if (gScale == null)
                    throw new Exception("ERROR: gScale is null");
                return (float)gScale;
            }
        }
        public float Mass
        {
            get
            {
                if (mass == null)
                    throw new Exception("ERROR: mass is null");
                return (float)mass;
            }
        }
        #endregion

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
        public STLAttribute(AttribType type, UInt32 attributeID, float friction, bool isDynamic, float? mass, float? xScale, float? yScale, float? zScale, float? gScale)
        {
            Type = type;
            AttributeID = attributeID;
            Friction = friction;
            IsDynamic = isDynamic;
            if(IsDynamic)
                this.mass = mass;
            else
                this.mass = null;

            switch (Type)
            {
                case AttribType.BOX_COLLIDER:
                    if (xScale == null)
                        throw new Exception("ERROR: Value expected for xScale");
                    if (yScale == null)
                        throw new Exception("ERROR: Value expected for yScale");
                    if (zScale == null)
                        throw new Exception("ERROR: Value expected for zScale");

                    this.xScale = xScale;
                    this.yScale = yScale;
                    this.zScale = zScale;
                    this.gScale = null;
                    break;
                case AttribType.SPHERE_COLLIDER:
                    if (gScale == null)
                        throw new Exception("ERROR: Value expected for gScale");
                    this.xScale = null;
                    this.yScale = null;
                    this.zScale = null;
                    this.gScale = gScale;
                    break;
                case AttribType.MESH_COLLIDER:
                    this.xScale = null;
                    this.yScale = null;
                    this.zScale = null;
                    this.gScale = null;
                    break;
                default:
                    throw new Exception("ERROR: Type undefined or null or something. Ya dun goofed");
            }
        }
    }
}
