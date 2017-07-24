using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GopherAPI.Nodes.Joint.Driver;

using GopherAPI.STL;

namespace GopherAPI.Nodes.Joint
{
    /// <summary>
    /// Enum defining the different types of joints
    /// </summary>
    public enum GopherJointType : short
    {
        ROTATIONAL = 0,
        LINEAR = 1,
        CYLINDRICAL = 2,
        PLANAR = 3,
        BALL = 4,
        NULL = 5
    }

    /// <summary>
    /// Base class for all joint types
    /// </summary>
    public class GopherJoint_Base
    {
        /// <summary>
        /// Gets the Joint type
        /// </summary>
        /// <returns></returns>
        public virtual GopherJointType GetJointType()
        {
            return GopherJointType.NULL;
        }

        /// <summary>
        /// Parent of the joint
        /// </summary>
        /// <remarks>
        /// For cylindrical joints, the exterior cylinder will be considered the parent
        /// </remarks>
        public GopherNode_Base Parent;
        /// <summary>
        /// Child of the joint
        /// </summary>
        public GopherNode_Base Child;

        /// <summary>
        /// The joint driver
        /// </summary>
        /// <remarks>
        /// Can be null
        /// </remarks>
        public GopherDriver_Base Driver = null;

        public JointMeta Meta;
    }
}